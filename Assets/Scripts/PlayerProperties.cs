﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerProperties : NetworkBehaviour {

    [Tooltip("Player Camera")]
    public GameObject playerCamera;

    public GameObject playerHUD;

    [Tooltip("Pointer object")]
    public GameObject aimChildPointer;

    [Tooltip("Player material")]
    public Material selfMaterial;
    [Tooltip("Ally material")]
    public Material allyMaterial;
    [Tooltip("Enemy material")]
    public Material enemyMaterial;

    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().material = selfMaterial;
        aimChildPointer.GetComponent<SpriteRenderer>().material = selfMaterial;
    }

    void Start()
    {
        if(!isLocalPlayer)
        {
            playerCamera.SetActive(false);
            playerHUD.SetActive(false);
        }
    }

}
