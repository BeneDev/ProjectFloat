using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineController : MonoBehaviour {

    [SerializeField] float upForce = 5f;
    [SerializeField] float jumpDuration = 1f;

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
        rb.useGravity = false;
        rb.AddForce(Vector3.up * upForce, ForceMode.Impulse);
        for (float t = 0f; t < jumpDuration; t += Time.deltaTime)
        {
            //rb.velocity = new Vector3(rb.velocity.x, -upForce, rb.velocity.z);
            rb.AddForce((Vector3.up * upForce) * (1 - (t / jumpDuration)), ForceMode.Force);
            yield return new WaitForEndOfFrame();
        }
        rb.useGravity = true;
    }
}
