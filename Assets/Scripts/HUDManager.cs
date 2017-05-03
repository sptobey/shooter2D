using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HUDManager : NetworkBehaviour {

    /*
     * This is a temporary solution
     * Should use observer pattern (or something smarter) for updating UI
     */

    public Text weaponEquipped;
    public Text ammoCapacity;
    public Text ammoMagazine;
    public Image isSprinting;
    public Image isReloading;
    public Image isEquipping;
    public Image isFiring;

    private PlayerWeapons weapons;
    private PlayerWeaponsController weaponsController;
    private PlayerController controller;

	void Start ()
    {
        weapons = GetComponent<PlayerWeapons>();
        weaponsController = GetComponent<PlayerWeaponsController>();
        controller = GetComponent<PlayerController>();
	}
	
	void Update ()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        weaponEquipped.text = weapons.EquippedWeapon.type;
        ammoCapacity.text = weapons.getAmmoReserves.ToString();
        ammoMagazine.text = weapons.getAmmoMagazine.ToString();

        /* Equipping indicator */
        Color color = isEquipping.color;
        color.a = (weaponsController.getIsEquipping()) ? 0.50f : 0.0f;
        isEquipping.color = color;

        /* Reloading indicator */
        color = isReloading.color;
        color.a = (weaponsController.getIsReloading()) ? 0.50f : 0.0f;
        isReloading.color = color;

        /* Firing indicator */
        color = isFiring.color;
        color.a = (weaponsController.getIsFiring()) ? 0.50f : 0.0f;
        isFiring.color = color;

        /* Sprinting indicator */
        color = isSprinting.color;
        color.a = (controller.getIsSprintLocked()) ? 0.0f : 0.50f;
        isSprinting.color = color;
	}
}
