using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour {

    [SerializeField] Image crosshair;

    FirstPersonController player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        player.OnCrosshairChanged += ChangeCrosshair;
    }

    void ChangeCrosshair(Sprite newCrosshair)
    {
        crosshair.sprite = newCrosshair;
    }

}
