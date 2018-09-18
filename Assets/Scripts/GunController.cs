using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public bool IsAiming
    {
        get
        {
            return isAiming;
        }
        set
        {
            isAiming = value;
            anim.SetBool("Aiming", value);
        }
    }

    public Sprite CrosshairImage
    {
        get
        {
            return crosshairImage;
        }
    }

    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float shotDelay = 1f;
    [SerializeField] protected float ballSpeed = 20f; // TODO maybe give this into the GetBall() function from GameManager
    [Range(0, 100), SerializeField] protected float critchance = 10f;
    [SerializeField] protected float recoil = 1f;
    [SerializeField] protected float numberOfShots = 1f;
    [Range(0, 100), SerializeField] protected float chanceToMiss = 5f;
    protected bool isAiming = false;
    [SerializeField] Transform mainMuzzle;
    bool isShooting = false;

    [SerializeField] Sprite crosshairImage;

    Animator anim;

	// Use this for initialization
	protected virtual void Start () {
        anim = GetComponent<Animator>();
	}

    public virtual void Equip()
    {

    }

    public virtual void Unequip()
    {

    }

    public virtual void Shoot()
    {
        if(!isShooting)
        {
            anim.SetTrigger("Shoot");
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo, 1000f);
            Vector3 direction;
            if (hitInfo.collider != null)
            {
                direction = hitInfo.point - mainMuzzle.transform.position;
            }
            else
            {
                direction = (mainMuzzle.transform.position + ray.direction * 10f) - mainMuzzle.transform.position;
            }
            GameManager.Instance.GetBall(mainMuzzle.position, direction);
            isShooting = true;
        }
    }

    public void ResetShoot()
    {
        anim.ResetTrigger("Shoot");
        isShooting = false;
    }

    public virtual void Reload()
    {
        anim.SetTrigger("Reload");
    }
}
