using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    Rigidbody rb;
    [SerializeField] float speed = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnEnable() {
        rb.velocity = transform.forward * speed;
	}
}
