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
        public float ForwardSpeed = 8.0f;   // Speed when walking forward // Speed when walking sideways
        public float RunMultiplier = 2.0f;   // Speed when sprinting
	    public KeyCode RunKey = KeyCode.LeftShift;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
        private bool m_Running;
#endif


        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
	        if (input == Vector2.zero) return;
			CurrentTargetSpeed = ForwardSpeed;
#if !MOBILE_INPUT
	        if (Input.GetKey(RunKey))
	        {
		        CurrentTargetSpeed *= RunMultiplier;
		        m_Running = true;
	        }
	        else
	        {
		        m_Running = false;
	        }
#endif
        }

#if !MOBILE_INPUT
        public bool Running
        {
            get { return m_Running; }
        }
#endif
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

    private Rigidbody m_RigidBody;
    private CapsuleCollider m_Capsule;
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;


    public Vector3 Velocity
    {
        get { return m_RigidBody.velocity; }
    }

    public bool Grounded
    {
        get { return m_IsGrounded; }
    }

    public bool Jumping
    {
        get { return m_Jumping; }
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
        m_RigidBody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        mouseLook.Init (transform, cam.transform);
        input = GetComponent<PlayerInput>();
        equippedGun = GetComponentInChildren<GunController>();
        anim = GetComponent<Animator>();
    }


    private void Update()
    {
        RotateView();

        if (input.Jump && !m_Jump)
        {
            m_Jump = true;
        }
        if(equippedGun)
        {
            if (input.Shoot)
            {
                anim.SetBool("Aiming", true);
                equippedGun.Shoot();
            }
            else if (!input.Shoot && !input.Aim)
            {
                anim.SetBool("Aiming", false);
            }
            if (input.Reload)
            {
                equippedGun.Reload();
            }
            if (input.Aim && !equippedGun.IsAiming)
            {
                equippedGun.IsAiming = true;
                anim.SetBool("Aiming", true);
            }
            else if (!input.Aim && equippedGun.IsAiming)
            {
                equippedGun.IsAiming = false;
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

        if ((Mathf.Abs(inputDir.x) > float.Epsilon || Mathf.Abs(inputDir.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = cam.transform.forward*inputDir.y + cam.transform.right*inputDir.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
            if (m_RigidBody.velocity.sqrMagnitude <
                (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
            {
                m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
            }
        }

        if (m_IsGrounded)
        {
            m_RigidBody.drag = 5f;

            if (m_Jump)
            {
                m_RigidBody.drag = 0f;
                m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                m_Jumping = true;
            }

            if (!m_Jumping && Mathf.Abs(inputDir.x) < float.Epsilon && Mathf.Abs(inputDir.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
            {
                m_RigidBody.Sleep();
            }
        }
        else
        {
            m_RigidBody.drag = 0f;
            if (m_PreviouslyGrounded && !m_Jumping)
            {
                StickToGroundHelper();
            }
        }
        m_Jump = false;
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
        equippedGun = gun.gameObject.GetComponent<GunController>();
        equippedGun.transform.parent = gunHolder;
        equippedGun.transform.localPosition = Vector3.zero;
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
        if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                ((m_Capsule.height/2f) - m_Capsule.radius) +
                                advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
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

        if (m_IsGrounded || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
        {
            m_Jumping = false;
        }
    }
}
