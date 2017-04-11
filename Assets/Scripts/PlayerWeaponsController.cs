using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeaponsController : NetworkBehaviour {

    public AbstractWeapon primaryWeaopn;
    public AbstractWeapon secondaryWeaopn;

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

    private AbstractWeapon equippedWeapon;
    private float fireTime;
    private float timerRateOfFire;
    private int ammunition;
    private float timerReloading;

    void Start ()
    {
        equipWeapon(primaryWeaopn);
    }
	
	void Update ()
    {
        /* Update fire-rate timer */
        if (timerRateOfFire >= 0.0f) { timerRateOfFire -= Time.deltaTime; }
        //Debug.Log("Fire Timer: " + timerRateOfFire);

        /* Update reload timer */
        if (timerReloading >= 0.0f) { timerReloading -= Time.deltaTime; }
        //Debug.Log("Reload Timer: " + timerReloading);

        /* Input */
        if (equippedWeapon.isAutomatic)
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

        /* Fire weapon */
        if (fireButtonInput >= equippedWeapon.fireThreshold
            && isLocalPlayer)
        {
            /* Fire rate enforced, not reloading, has ammunition */
            if (timerRateOfFire <= 0.0f &&
                timerReloading <= 0.0f &&
                ammunition >= 1)
            {
                Cmd_fireWeapon();
                timerRateOfFire = fireTime;
            }          
        }
    }

    private void equipWeapon(AbstractWeapon weapon)
    {
        equippedWeapon = weapon;
        fireTime = 1.0f / weapon.roundsPerSecond;
        /* Weapon is ready to fire 
         * TODO: add weapon equip time and fire delay */
        timerRateOfFire = 0.0f;
        ammunition = weapon.magazineSize;
        timerReloading = 0.0f;
    }

    private void reloadWeapon()
    {
        ammunition = equippedWeapon.magazineSize;
        timerReloading = equippedWeapon.reloadSpeed;
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
                    if (distance <= equippedWeapon.hipMinRange)
                    {
                        damage = equippedWeapon.roundMaxDamage;
                    }
                    else if(distance >= equippedWeapon.hipMaxRange)
                    {
                        damage = equippedWeapon.roundMinDamage;
                    }
                    else
                    {
                        float lerp = Mathf.Clamp(
                            (distance - equippedWeapon.hipMinRange) /
                            (equippedWeapon.hipMaxRange - equippedWeapon.hipMinRange),
                            0.0f, 1.0f);
                        damage = Mathf.Lerp(equippedWeapon.roundMaxDamage, equippedWeapon.roundMinDamage, lerp);
                    }
                    opponentLife.Cmd_applyDamage(damage);
                }
            }
        }

        /* Editor debug raycast */
        Debug.DrawRay(fireLocation.position, aimDirection * maxShootDistance, Color.magenta, 0.1f);

        /* Update weapon
         * TODO: recoil */
        ammunition -= 1;
        if (ammunition <= 0)
        {
            reloadWeapon();
        }
    } /* Cmd_fireWeapon() */
}
