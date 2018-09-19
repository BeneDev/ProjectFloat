using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    public int Damage
    {
        get
        {
            return damage;
        }
        set
        {
            rb.mass = value * 10;
            damage = value;
        }
    }

    Rigidbody rb;
    [SerializeField] float speed = 35f;
    int damage = 1;
    bool isDamaging = true;
    [SerializeField] float damagingTime = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnEnable() {
        rb.velocity = transform.forward * speed;
        isDamaging = true;
        Invoke("NoDamageAnymore", damagingTime);
	}

    void NoDamageAnymore()
    {
        isDamaging = false;
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
        if (enemy && isDamaging)
        {
            enemy.TakeDamage(damage);
        }
    }
}
