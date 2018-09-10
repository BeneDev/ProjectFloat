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

    public int Jump
    {
        get
        {
            if (Input.GetButtonDown("Jump"))
            {
                return 1;
            }
            else if (Input.GetButton("Jump"))
            {
                return 2;
            }
            return 0;
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

    public bool Interact
    {
        get
        {
            return Input.GetButton("Interact");
        }
    }
}
