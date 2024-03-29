﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeaponsController : NetworkBehaviour {

    public LayerMask allShootableLayers;
    public string precisionTagName;
    public Transform fireLocation;
    public Transform playerCenter;

    public GameObject bulletPrefab;
    public float bulletSpeed = 100.0f;
    public float bulletLifetime = 0.5f;
    
    [Tooltip("Hit penetration depth"), Range(1,10)]
    public int maxHits = 5;

    [Tooltip("Range for raycast"), Range(0.0f, 500.0f)]
    public float maxShootDistance = 100.0f;

    [Tooltip("How hard to press 'Aim' to aim"), Range(-1.0f, 1.0f)]
    public float aimThreshold = 0.01f;

    [Tooltip("Degrees offset from positive z-direction")]
    public float aimAngleOffset = -90;

    [Tooltip("Aim Cone (should be drawn in local space)")]
    public LineRenderer aimLine;
    private Vector3 lineStartPos;

    /* Audio */
    private AudioSource audioSource;
    public AudioClip primaryWeaponAudioClip;
    public AudioClip secondaryWeaponAudioClip;

    /* Clent-specific variables */
    private bool waitingForFire;
    private bool waitingForReload;
    private bool waitingForEquip;

    private float fireButtonInput;
    private Vector3 aimDirection;

    /* Variables to communicate between Server and Clients */
    private bool isAiming;
    private float fireAxis;
    private bool fireButton;
    private bool swapButton;
    private bool reloadButton;
    private string equippedSlot;
    private PlayerWeapons playerWeapons; /* Handled implicitly */

    void Start ()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerWeapons.equipWeapon("Primary");

        audioSource = GetComponent<AudioSource>();

        fireButtonInput = -1.0f;
                
        waitingForFire = false;
        waitingForReload = false;
        waitingForEquip = false;

        isAiming = false;
        fireAxis = -1.0f;
        fireButton = false;
        swapButton = false;
        reloadButton = false;
        equippedSlot = "Primary";

        lineStartPos = aimLine.GetPosition(0);
        drawAimRange();
    }

    [Command]
    private void Cmd_sendInput(bool c_isAim, float c_fireAxis,bool c_fireBtn, 
        bool c_swapBtn, bool c_reloadBtn, string c_equipSlot)
    {
        isAiming = c_isAim;
        fireAxis = c_fireAxis;
        fireButton = c_fireBtn;
        swapButton = c_swapBtn;
        reloadButton = c_reloadBtn;
        equippedSlot = c_equipSlot;

        /* Update other Clients */
        Rpc_sendInput(c_isAim, c_fireAxis, c_fireBtn,
        c_swapBtn, c_reloadBtn, c_equipSlot);
    }

    [Command]
    private void Cmd_reloadWeapon()
    {
        playerWeapons.reloadWeapon();

        /* Update other Clients */
        Rpc_reloadWeapon();
    }

    [Command]
    private void Cmd_equipWeapon(string slot)
    {
        playerWeapons.equipWeapon(slot);
        equippedSlot = slot;

        /* Update other Clients */
        Rpc_equipWeapon(slot);
    }

    [Command]
    private void Cmd_PlayWeaponAudio()
    {
        if(playerWeapons.EquippedSlot == "Primary")
        {
            audioSource.clip = primaryWeaponAudioClip;
        }
        else if(playerWeapons.EquippedSlot == "Secondary")
        {
            audioSource.clip = secondaryWeaponAudioClip;
        }

        audioSource.Play();

        Rpc_PlayWeaponAudio();
    }

    [ClientRpc]
    private void Rpc_sendInput(bool s_isAim, float s_fireAxis, bool s_fireBtn,
        bool s_swapBtn, bool s_reloadBtn, string s_equipSlot)
    {
        isAiming = s_isAim;
        fireAxis = s_fireAxis;
        fireButton = s_fireBtn;
        swapButton = s_swapBtn;
        reloadButton = s_reloadBtn;
        equippedSlot = s_equipSlot;
    }

    [ClientRpc]
    private void Rpc_reloadWeapon()
    {
        playerWeapons.reloadWeapon();
    }

    [ClientRpc]
    private void Rpc_equipWeapon(string slot)
    {
        playerWeapons.equipWeapon(slot);
        equippedSlot = slot;
    }

    [ClientRpc]
    private void Rpc_UpdatePlayerWeapons(int roundsFired)
    {
        playerWeapons.decrementMagazine(roundsFired);
        if (playerWeapons.roundsInMagazine() <= 0)
        {
            StartCoroutine(handleReloadWeapon());
        }
    }

    [ClientRpc]
    private void Rpc_PlayWeaponAudio()
    {
        if (playerWeapons.EquippedSlot == "Primary")
        {
            audioSource.clip = primaryWeaponAudioClip;
        }
        else if (playerWeapons.EquippedSlot == "Secondary")
        {
            audioSource.clip = secondaryWeaponAudioClip;
        }

        audioSource.Play();
    }

    void Update ()
    {
        /* Input from Client (or Server/Host) */
        if (isLocalPlayer)
        {
            isAiming = (Input.GetAxis("L2_Axis") >= aimThreshold) ? true : false;
            fireAxis = Input.GetAxis("R2_Axis");
            fireButton = Input.GetButtonDown("R2_Button");
            swapButton = Input.GetButtonDown("Triangle");
            reloadButton = Input.GetButtonDown("Square");
            // equippedSlot handled elsewhere in coroutine

            /* Tell Server about Client input */
            if(!isServer)
            {
                Cmd_sendInput(isAiming, fireAxis, fireButton,
                    swapButton, reloadButton, equippedSlot);
            }
            /* Tell Clients about Server (i.e. host) input */
            else
            {
                Rpc_sendInput(isAiming, fireAxis, fireButton,
                    swapButton, reloadButton, equippedSlot);
            }
        }

        /* Aim */
        drawAimRange();

        /* Fire */
        if (playerWeapons.EquippedWeapon.isAutomatic)
        {
            fireButtonInput = fireAxis;
        }
        else
        {
            fireButtonInput = (fireButton) ? 1.0f : -1.0f;
        }

        /* Swap weapons */
        if (isLocalPlayer && swapButton)
        {
            string slot;
            switch (playerWeapons.EquippedSlot)
            {
                case "Secondary":
                    slot = "Primary";
                    break;
                case "Tertiary":
                    slot = "Primary";
                    break;
                case "Primary":
                default:
                    slot = "Secondary";
                    break;
            }
                if (!waitingForEquip && !waitingForReload) //&& !waitingForFire
                {
                    StartCoroutine(handleEquipWeapon(slot));
                }
        }

        /* Manual reload */
        if (isLocalPlayer && reloadButton)
        {
            if (!waitingForEquip && !waitingForReload) //&& !waitingForFire
            {
                StartCoroutine(handleReloadWeapon());
            }
        }

        /* Temporary UI/HUD */
        //Debug.Log("Firing: " + waitingForFire +
        //        ", Reloading: " + waitingForReload +
        //        ", Equipping: " + waitingForEquip +
        //        ", Magazine: " + playerWeapons.roundsInMagazine());

        /* Fire weapon */
        if (isLocalPlayer &&
            fireButtonInput >= playerWeapons.EquippedWeapon.fireThreshold)
        {
            /* Fire rate enforced, not reloading, has ammunition */
            if (!waitingForFire &&
                !waitingForReload &&
                !waitingForEquip &&
                playerWeapons.roundsInMagazine() >= 1)
            {
                StartCoroutine(handleFireWeapon());
            }          
        }
    }

    private IEnumerator handleFireWeapon()
    {
        float fireTime = (1 / playerWeapons.EquippedWeapon.roundsPerSecond);
        waitingForFire = true;
        Cmd_fireWeapon();

        yield return new WaitForSeconds(fireTime);
        waitingForFire = false;
    }

    private IEnumerator handleReloadWeapon()
    {
        /*  Weapon does not need to (or cannot) be reloaded */
        if (!playerWeapons.needsReloading()) { yield break; }

        waitingForReload = true;

        yield return new WaitForSeconds(playerWeapons.EquippedWeapon.reloadSpeed);
        playerWeapons.reloadWeapon();

        if(!isServer) { Cmd_reloadWeapon(); }
        else { Rpc_reloadWeapon(); }
        
        waitingForReload = false;

        /* Allow immediate fire after reload */
        waitingForFire = false;
    }

    private IEnumerator handleEquipWeapon(string slot)
    {
        float equipTime = 0.0f;
        switch (slot)
        {
            case "Primary":
                equipTime = playerWeapons.primaryWeapon.equipTime;
                break;
            case "Secondary":
                equipTime = playerWeapons.secondaryWeapon.equipTime;
                break;
            case "Tertiary":
                equipTime = playerWeapons.tertiaryWeapon.equipTime;
                break;
            default:
                equipTime = 0.0f;
                break;
        }
        waitingForEquip = true;

        yield return new WaitForSeconds(equipTime);
        playerWeapons.equipWeapon(slot);
        equippedSlot = slot;

        if (!isServer) { Cmd_equipWeapon(slot); }
        else { Rpc_equipWeapon(slot); }

        waitingForEquip = false;

        /* Allow immediate fire after equip */
        waitingForFire = false;
    }

    private void drawAimRange()
    {
        float range, angle, lineWidthFactor;
        if(!isAiming)
        {
            range = playerWeapons.EquippedWeapon.hipMinRange;
            angle = playerWeapons.EquippedWeapon.hipAccuracy;
        }
        else
        {
            range = playerWeapons.EquippedWeapon.adsMaxRange;
            angle = playerWeapons.EquippedWeapon.adsAccuracy;
        }

        lineWidthFactor = 2.0f * range * Mathf.Tan( Mathf.Deg2Rad * 0.5f * angle );

        /* Note - Line drawn in local space */
        Vector3 lineEndPos = new Vector3(
            lineStartPos.x,
            lineStartPos.y + range,
            lineStartPos.z);
        aimLine.SetPosition(0, lineStartPos);
        aimLine.SetPosition(1, lineEndPos);
        aimLine.widthMultiplier = lineWidthFactor;
    }

    [Command]
    private void Cmd_fireWeapon()
    {
        /* Direction and accuracy */
        aimDirection = (fireLocation.position - playerCenter.position);
        aimDirection.Normalize();
        float inaccuracy = (!isAiming) ?
            playerWeapons.hipAimAngle() :
            playerWeapons.adsAimAngle();
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) + (Mathf.Deg2Rad * inaccuracy);
        //Debug.Log("aim angle: " + aimAngle);
        aimDirection.x = Mathf.Cos(aimAngle);
        aimDirection.y = Mathf.Sin(aimAngle);
        aimDirection.Normalize();

        Quaternion bulletRotation = Quaternion.AngleAxis(
            (aimAngle * Mathf.Rad2Deg) + aimAngleOffset,
            Vector3.forward);

        /* Bullet prefab */
        GameObject bullet = Instantiate(
            bulletPrefab, fireLocation.position, bulletRotation);
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * bulletSpeed;
        /* Spawn the bullet on the Clients */
        NetworkServer.Spawn(bullet);
        Destroy(bullet, bulletLifetime);

        /* Audio */
        if (!isServer) { Cmd_PlayWeaponAudio(); }
        else { Rpc_PlayWeaponAudio(); }

        /* Raycast */
        RaycastHit2D[] hits = new RaycastHit2D[maxHits];
        int numHits = Physics2D.RaycastNonAlloc(
            fireLocation.position,
            aimDirection,
            hits,
            maxShootDistance,
            allShootableLayers.value);

        /* Detect precision damage.  Keep track of names of precision objects 
         * hit and the parent object.  Hard-coded based on player heirarchy:
         * Player_vX (contains the life script and principal collider)
         *   -->PointerBody
         *        -->PlayerCenterCriticalSpot
         */
        HashSet<int> needsPrecisionDamage = new HashSet<int>();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                int root_id = hit.transform.root.GetInstanceID();
                if (hit.collider.tag == precisionTagName)
                {
                    needsPrecisionDamage.Add(root_id);
                }
                Debug.Log("Hit: " + hit.collider.name + " (" + hit.collider.GetInstanceID() + ").  Root: " + 
                    hit.transform.root.name + " (" + hit.transform.root.GetInstanceID() + ").  Tag: " + 
                    hit.collider.tag);
            }
        }

        /* Variable to prevent damage through walls.
         * Relies on the order of the raycast results array. */
        bool wasWallHit= false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                int root_id = hit.transform.root.GetInstanceID();
                bool isPrecisionDamage = needsPrecisionDamage.Contains(root_id);

                /* Apply damage only to players, for now */
                PlayerLife opponentLife = hit.transform.GetComponent<PlayerLife>();
                if (opponentLife != null && !wasWallHit)
                {
                    float damage = 0.0f;
                    float minRange, maxRange;

                    /* Determine range falloff damage based on if ADS or not */
                    if (!isAiming)
                    {
                        minRange = playerWeapons.EquippedWeapon.hipMinRange;
                        maxRange = playerWeapons.EquippedWeapon.hipMaxRange;
                    }
                    else
                    {
                        minRange = playerWeapons.EquippedWeapon.adsMinRange;
                        maxRange = playerWeapons.EquippedWeapon.adsMaxRange;
                    }

                    /* Calculate the amount of damage to do based on range and precsion hits */
                    float distance = (this.transform.position - hit.transform.position).magnitude;
                    if (distance <= minRange)
                    {
                        damage = (isPrecisionDamage) ?
                            playerWeapons.EquippedWeapon.roundMaxPrecisionDamage :
                            playerWeapons.EquippedWeapon.roundMaxDamage;
                    }
                    else if(distance >= maxRange)
                    {
                        damage = (isPrecisionDamage) ?
                            playerWeapons.EquippedWeapon.roundMinPrecisionDamage :
                            playerWeapons.EquippedWeapon.roundMinDamage;
                    }
                    else
                    {
                        float lerp = Mathf.Clamp(
                            (distance - minRange) / (maxRange - minRange),
                            0.0f, 1.0f);
                        float damMax = (isPrecisionDamage) ?
                            playerWeapons.EquippedWeapon.roundMaxPrecisionDamage :
                            playerWeapons.EquippedWeapon.roundMaxDamage;
                        float damMin = (isPrecisionDamage) ?
                            playerWeapons.EquippedWeapon.roundMinPrecisionDamage :
                            playerWeapons.EquippedWeapon.roundMinDamage;
                        damage = Mathf.Lerp(damMax, damMin, lerp);
                    }

                    Debug.Log("Hit Player: " + hit.collider.name);

                    /* Apply Damage */
                    opponentLife.Cmd_applyDamage(damage, isPrecisionDamage);
                }
                /* Hit non-player */
                else
                {
                    wasWallHit = true;
                }
            }
        }

        /* Editor debug raycast */
        Debug.DrawRay(
            fireLocation.position,
            aimDirection * maxShootDistance,
            Color.magenta, 0.1f);

        /* Update weapon TODO: recoil */
        Rpc_UpdatePlayerWeapons(1);

    } /* Cmd_fireWeapon */

    public bool getIsEquipping()
    {
        return waitingForEquip;
    }

    public bool getIsReloading()
    {
        return waitingForReload;
    }

    public bool getIsFiring()
    {
        return waitingForFire;
    }
}
