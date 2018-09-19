using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour {

    [SerializeField] Image crosshair;

    FirstPersonController player;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj)
        {
            player = playerObj.GetComponent<FirstPersonController>();
        }
        if(player)
        {
            player.OnCrosshairChanged += ChangeCrosshair;
        }
    }

    void ChangeCrosshair(Sprite newCrosshair)
    {
        crosshair.sprite = newCrosshair;
    }

}
