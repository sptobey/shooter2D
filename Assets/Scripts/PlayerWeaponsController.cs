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

    private float fireButtonInput;
    private Vector2 aimDirection;

    private bool waitingForFire;
    private bool waitingForReload;
    private bool waitingForEquip;

    private PlayerWeapons playerWeapons;

    void Start ()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerWeapons.equipWeapon("Primary");

        waitingForFire = false;
        waitingForReload = false;
        waitingForEquip = false;
    }

    void Update ()
    {
        /* Input */
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

        Debug.Log("Firing: " + waitingForFire +
                ", Reloading: " + waitingForReload +
                ", Equipping: " + waitingForEquip +
                ", Magazine: " + playerWeapons.roundsInMagazine());

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
        waitingForFire = true;

        yield return new WaitForSeconds(fireTime);
        Cmd_fireWeapon();
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

    [Command]
    private void Cmd_fireWeapon()
    {
        /* Bullet prefab */
        GameObject bullet = Instantiate(
            bulletPrefab, fireLocation.position, fireLocation.rotation);
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * bulletSpeed;
        /* Spawn the bullet on the Clients */
        NetworkServer.Spawn(bullet);
        Destroy(bullet, bulletLifetime);

        /* Raycast */
        aimDirection = (fireLocation.position - playerCenter.position);
        aimDirection.Normalize();
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
                Debug.Log("Hit: " + hit.collider.name);
                PlayerLife opponentLife = hit.transform.GetComponent<PlayerLife>();
                if (opponentLife != null)
                {
                    float damage = 0.0f;
                    float distance = (this.transform.position - hit.transform.position).magnitude;
                    if (distance <= playerWeapons.EquippedWeapon.hipMinRange)
                    {
                        damage = playerWeapons.EquippedWeapon.roundMaxDamage;
                    }
                    else if(distance >= playerWeapons.EquippedWeapon.hipMaxRange)
                    {
                        damage = playerWeapons.EquippedWeapon.roundMinDamage;
                    }
                    else
                    {
                        float lerp = Mathf.Clamp(
                            (distance - playerWeapons.EquippedWeapon.hipMinRange) /
                            (playerWeapons.EquippedWeapon.hipMaxRange - playerWeapons.EquippedWeapon.hipMinRange),
                            0.0f, 1.0f);
                        damage = Mathf.Lerp(playerWeapons.EquippedWeapon.roundMaxDamage, playerWeapons.EquippedWeapon.roundMinDamage, lerp);
                    }
                    opponentLife.Cmd_applyDamage(damage);
                }
            }
        }

        /* Editor debug raycast */
        Debug.DrawRay(fireLocation.position, aimDirection * maxShootDistance, Color.magenta, 0.1f);

        /* Update weapon
         * TODO: recoil */
        playerWeapons.decrementMagazine(1);
        if (playerWeapons.roundsInMagazine() <= 0)
        {
            StartCoroutine(handleReloadWeapon());
        }
    } /* Cmd_fireWeapon() */
}
