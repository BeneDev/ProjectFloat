using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public bool IsShooting
    {
        get
        {
            return isShooting;
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
    //[Range(0, 100), SerializeField] protected float critchance = 10f;
    //[SerializeField] protected float recoil = 1f;
    [SerializeField] protected float numberOfShots = 1f;
    [Range(0, 100), SerializeField] protected float accuracy = 5f;
    [SerializeField] Transform mainMuzzle;
    bool isShooting = false;

    [SerializeField] Sprite crosshairImage;

    public virtual void Equip()
    {

    }

    public virtual void Unequip()
    {

    }

    public virtual void Shoot()
    {
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
        Invoke("ResetShooting", shotDelay);
    }

    void ResetShooting()
    {
        isShooting = false;
    }
}
