using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineController : MonoBehaviour {

    [SerializeField] float upForce = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if(rb)
        {
            StartCoroutine(ApplyForce(rb));
        }
    }

    IEnumerator ApplyForce(Rigidbody rb)
    {
        yield return new WaitForSeconds(0.2f);
        rb.AddForce(Vector3.up * upForce, ForceMode.Impulse);
    }
}
