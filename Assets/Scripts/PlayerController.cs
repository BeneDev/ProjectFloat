using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour {

    Camera cam;
    [SerializeField] float speed = 5f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float camSensitivityX = 1f;
    [SerializeField] float camSensitivityY = 1f;

    [SerializeField] float minimumX = -360F;
    [SerializeField] float maximumX = 360F;

    [SerializeField] float minimumY = -60F;
    [SerializeField] float maximumY = 60F;

    float rotationY = 0f;

    PlayerInput input;
    Rigidbody rb;

    private void Awake()
    {
        cam = Camera.main;
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Make the camera move
        float rotationX = cam.transform.localEulerAngles.y + Input.GetAxis("MouseX") * camSensitivityX;

        rotationY += Input.GetAxis("MouseY") * camSensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        cam.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

        // Make the player move Camera Relative
        Vector3 forward = cam.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        float h = input.Horizontal;
        float v = input.Vertical;

        Vector3 moveDirection = (h * right + v * forward);
        if(!input.Run)
        {
            //transform.position += moveDirection * speed * Time.deltaTime;
            rb.velocity = moveDirection * speed * Time.deltaTime;
        }
        else
        {
            //transform.position += moveDirection * runSpeed * Time.deltaTime;
            rb.velocity = moveDirection * runSpeed * Time.deltaTime;
        }
    }

}
