using System;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
    [Serializable]
    public class MovementSettings
    {
        public float Speed = 8.0f;   // Speed when walking forward // Speed when walking sideways
        public float RunMultiplier = 2.0f;   // Speed when sprinting
	    public KeyCode RunKey = KeyCode.LeftShift;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector] public float CurrentTargetSpeed = 8f;
        
        private bool m_Running;

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
	        if (input == Vector2.zero) return;
			CurrentTargetSpeed = Speed;
	        if (Input.GetKey(RunKey))
	        {
		        CurrentTargetSpeed *= RunMultiplier;
		        m_Running = true;
	        }
	        else
	        {
		        m_Running = false;
	        }
        }

        public bool Running
        {
            get { return m_Running; }
        }
    }


    [Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
        public bool airControl; // can the user control the direction that is being moved in the air
        [Tooltip("set it to 0.1 or more if you get stuck in wall")]
        public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    }

    public event System.Action<Sprite> OnCrosshairChanged;


    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook = new MouseLook();
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    PlayerInput input;
    GunController equippedGun;
    Animator anim;

    [SerializeField] LayerMask gunLayer;
    [SerializeField] Vector3 gunGrabExtents;
    [SerializeField] Transform gunHolder;

    int groundLayer;

    private Rigidbody rb;
    private CapsuleCollider capColl;
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool jump, isPreviouslyGrounded, isJumping, isGrounded, isOnSlope;


    public Vector3 Velocity
    {
        get { return rb.velocity; }
    }

    public bool Grounded
    {
        get { return isGrounded; }
    }

    public bool Jumping
    {
        get { return isJumping; }
    }

    public bool Running
    {
        get
        {
            anim.SetBool("Running", movementSettings.Running);
			return movementSettings.Running;
        }
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capColl = GetComponent<CapsuleCollider>();
        mouseLook.Init (transform, cam.transform);
        input = GetComponent<PlayerInput>();
        equippedGun = GetComponentInChildren<GunController>();
        anim = GetComponent<Animator>();
        groundLayer = LayerMask.NameToLayer("Default");
    }


    private void Update()
    {
        RotateView();

        if (input.Jump && !jump)
        {
            jump = true;
        }
        if(equippedGun)
        {
            if (input.Shoot && !equippedGun.IsShooting)
            {
                anim.SetTrigger("Shoot");
                equippedGun.Shoot();
            }
            if (input.Aim)
            {
                anim.SetBool("Aiming", true);
            }
            else if (!input.Aim)
            {
                anim.SetBool("Aiming", false);
            }
        }
        if(input.Interact)
        {
            LookForGunsToEquip();
        }
    }


    private void FixedUpdate()
    {
        GroundCheck();
        Vector2 inputDir = GetInput();

        if ((Mathf.Abs(inputDir.x) > float.Epsilon || Mathf.Abs(inputDir.y) > float.Epsilon) && (advancedSettings.airControl || isGrounded))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = cam.transform.forward*inputDir.y + cam.transform.right*inputDir.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;

            // Get the velocity
            Vector3 horizontalMove = desiredMove;
            // Don't use the vertical velocity
            horizontalMove.y = 0f;
            // Calculate the approximate distance that will be traversed
            float distance = horizontalMove.magnitude * Time.fixedDeltaTime;
            // Normalize horizontalMove since it should be used to indicate direction
            horizontalMove.Normalize();
            RaycastHit hit;

            // Check if the body's current velocity will result in a collision
            if (rb.SweepTest(horizontalMove, out hit, distance))
            {
                print(hit.collider.gameObject.layer + "||" + groundLayer);
                if(hit.collider.gameObject.layer == groundLayer)
                {
                    if (hit.point.y > transform.position.y - capColl.height * 0.25f)
                    {
                        // If so, stop the movement
                        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                    }
                    else if (isGrounded)
                    {
                        // Move the character up, so the character takes a step up the collider
                        transform.position += new Vector3(0f, (hit.point.y - (transform.position.y - (capColl.height * 0.5f))) * 1.6f, 0f);
                    }
                }
            }
            else if (rb.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
            {
                if(!isOnSlope)
                {
                    rb.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }
        }

        if (isGrounded)
        {
            rb.drag = 5f;

            if (jump)
            {
                rb.drag = 0f;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                isJumping = true;
            }

            if (!isJumping && Mathf.Abs(inputDir.x) < float.Epsilon && Mathf.Abs(inputDir.y) < float.Epsilon && rb.velocity.magnitude < 1f)
            {
                rb.Sleep();
            }
        }
        else
        {
            rb.drag = 0f;
            if (isPreviouslyGrounded && !isJumping)
            {
                StickToGroundHelper();
            }
        }
        if(isOnSlope)
        {
            rb.drag = 0f;
        }
        jump = false;
    }

    void LookForGunsToEquip()
    {
        Collider[] guns = Physics.OverlapBox(cam.transform.position, gunGrabExtents, cam.transform.rotation, gunLayer.value);
        foreach (Collider gun in guns)
        {
            if (gun.gameObject.GetComponent<GunController>())
            {
                EquipGun(gun);
                break;
            }
        }
    }

    private void EquipGun(Collider gun)
    {
        if(equippedGun)
        {
            equippedGun.Unequip();
        }
        equippedGun = gun.gameObject.GetComponent<GunController>();
        equippedGun.transform.parent = gunHolder;
        equippedGun.transform.localPosition = Vector3.zero;
        equippedGun.transform.localRotation = Quaternion.identity;
        equippedGun.Equip(gameObject);
        if(OnCrosshairChanged != null)
        {
            OnCrosshairChanged(equippedGun.CrosshairImage);
        }
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }


    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capColl.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                ((capColl.height/2f) - capColl.radius) +
                                advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hitInfo.normal);
            }
        }
    }


    private Vector2 GetInput()
    {
            
        Vector2 inputDir = new Vector2
            {
                x = input.Horizontal,
                y = input.Vertical
            };
		movementSettings.UpdateDesiredTargetSpeed(inputDir);
        return inputDir;
    }


    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        mouseLook.LookRotation (transform, cam.transform);

        if (isGrounded || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            rb.velocity = velRotation*rb.velocity;
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        isPreviouslyGrounded = isGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capColl.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                ((capColl.height/2f) - capColl.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
            if(hitInfo.collider.gameObject.tag == "Slope")
            {
                isOnSlope = true;
            }
            else
            {
                isOnSlope = false;
            }
        }
        else
        {
            isGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if (!isPreviouslyGrounded && isGrounded && isJumping)
        {
            isJumping = false;
        }
    }
}
