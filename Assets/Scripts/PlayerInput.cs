using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour, IInput {

    public float Horizontal
    {
        get
        {
            return Input.GetAxis("Horizontal");
        }
    }

    public float Vertical
    {
        get
        {
            return Input.GetAxis("Vertical");
        }
    }

    public Vector3 MouseEulers
    {
        get
        {
            return new Vector3(Input.GetAxis("MouseY"), Input.GetAxis("MouseX"), 0f);
        }
    }

    public bool Jump
    {
        get
        {
            return Input.GetButtonDown("Jump");
        }
    }

    public bool Run
    {
        get
        {
            return Input.GetButton("Run");
        }
    }

    public bool Shoot
    {
        get
        {
            return Input.GetButton("Fire1");
        }
    }

    public bool Reload
    {
        get
        {
            return Input.GetButtonDown("Reload");
        }
    }

    public bool Interact
    {
        get
        {
            return Input.GetButton("Interact");
        }
    }
}
