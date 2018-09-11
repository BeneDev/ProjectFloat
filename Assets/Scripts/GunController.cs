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

    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float shotDelay = 1f;
    [Range(0, 100), SerializeField] protected float critchance = 10f;
    [SerializeField] protected float recoil = 1f;
    [SerializeField] protected float numberOfShots = 1f;
    [Range(0, 100), SerializeField] protected float chanceToMiss = 5f;
    protected bool isAiming = false;

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
        anim.SetTrigger("Shoot");
    }

    public void ResetShoot()
    {
        anim.ResetTrigger("Shoot");
    }

    public virtual void Reload()
    {
        anim.SetTrigger("Reload");
    }
}
