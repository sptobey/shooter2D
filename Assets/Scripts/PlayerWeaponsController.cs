using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeaponsController : NetworkBehaviour {

    public LayerMask shootableLayers;
    public Transform fireLocation;
    public Transform playerCenter;

    public GameObject bulletPrefab;
    public float bulletSpeed = 100.0f;
    public float bulletLifetime = 0.5f;
    
    [Tooltip("Hit penetration depth"), Range(1,5)]
    public int maxHits = 5;

    [Tooltip("Range for raycast"), Range(0.0f, 500.0f)]
    public float maxShootDistance = 100.0f;

    [Tooltip("How hard to press 'Aim' to aim"), Range(-1.0f, 1.0f)]
    public float aimThreshold = 0.01f;

    [Tooltip("Degrees offset from positive z-direction")]
    public float aimAngleOffset = -90;

    private PlayerWeapons playerWeapons;

    private float fireButtonInput;
    private Vector3 aimDirection;
    private bool isAiming;

    private bool waitingForFire;
    private bool waitingForReload;
    private bool waitingForEquip;

    [Tooltip("Aim Cone (should be drawn in local space)")]
    public LineRenderer aimLine;

    void Start ()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerWeapons.equipWeapon("Primary");

        fireButtonInput = -1.0f;
        isAiming = false;
        drawAimRange();
        
        waitingForFire = false;
        waitingForReload = false;
        waitingForEquip = false;
    }

    void Update ()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        /* Input */

        /* Aim */
        if(Input.GetAxis("L2_Axis") >= aimThreshold)
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }
        drawAimRange();

        /* Fire */
        if (playerWeapons.EquippedWeapon.isAutomatic)
        {
            fireButtonInput = Input.GetAxis("R2_Axis");
            //Debug.Log("R2 Automatic Trigger: " + fireButtonInput);
        }
        else
        {
            if (Input.GetButtonDown("R2_Button"))
            {
                fireButtonInput = +1.0f;
                //Debug.Log("R2 Button: " + fireButtonInput);
            }
            else { fireButtonInput = -1.0f; }
        }

        /* Swap weapons */
        if(Input.GetButtonDown("Triangle"))
        {
            string slot;
            switch(playerWeapons.EquippedSlot)
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
            if(!waitingForEquip && !waitingForFire && !waitingForReload)
            {
                StartCoroutine(handleEquipWeapon(slot));
            }
        }

        /* Manual reload */
        if (Input.GetButtonDown("Square"))
        {
            if (!waitingForEquip && !waitingForFire && !waitingForReload)
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
        if (fireButtonInput >= playerWeapons.EquippedWeapon.fireThreshold
            && isLocalPlayer)
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
        Cmd_fireWeapon();
        waitingForFire = true;

        yield return new WaitForSeconds(fireTime);
        waitingForFire = false;
    }

    private IEnumerator handleReloadWeapon()
    {
        float reloadTime = playerWeapons.EquippedWeapon.reloadSpeed;
        waitingForReload = true;

        yield return new WaitForSeconds(reloadTime);
        playerWeapons.reloadWeapon();
        waitingForReload = false;
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
        waitingForEquip = false;
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
        Vector3 lineStartPos = aimLine.GetPosition(0);
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

        /* Raycast */
        RaycastHit2D[] hits = new RaycastHit2D[maxHits];
        int numHits = Physics2D.RaycastNonAlloc(
            fireLocation.position,
            aimDirection,
            hits,
            maxShootDistance,
            shootableLayers.value);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                //Debug.Log("Hit: " + hit.collider.name);
                PlayerLife opponentLife = hit.transform.GetComponent<PlayerLife>();
                if (opponentLife != null)
                {
                    float damage = 0.0f;
                    float minRange, maxRange;
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

                    float distance = (this.transform.position - hit.transform.position).magnitude;
                    if (distance <= minRange)
                    {
                        damage = playerWeapons.EquippedWeapon.roundMaxDamage;
                    }
                    else if(distance >= maxRange)
                    {
                        damage = playerWeapons.EquippedWeapon.roundMinDamage;
                    }
                    else
                    {
                        float lerp = Mathf.Clamp(
                            (distance - minRange) / (maxRange - minRange),
                            0.0f, 1.0f);
                        damage = Mathf.Lerp(
                            playerWeapons.EquippedWeapon.roundMaxDamage,
                            playerWeapons.EquippedWeapon.roundMinDamage,
                            lerp);
                    }
                    opponentLife.Cmd_applyDamage(damage);
                }
            }
        }

        /* Editor debug raycast */
        Debug.DrawRay(
            fireLocation.position,
            aimDirection * maxShootDistance,
            Color.magenta, 0.1f);

        /* Update weapon
         * TODO: recoil */
        playerWeapons.decrementMagazine(1);
        if (playerWeapons.roundsInMagazine() <= 0)
        {
            StartCoroutine(handleReloadWeapon());
        }
    } /* Cmd_fireWeapon() */
}
