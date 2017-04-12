﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour {

    public AbstractWeapon primaryWeapon;
    public AbstractWeapon secondaryWeapon;
    public AbstractWeapon tertiaryWeapon;

    private int ammoReservesPrimary;
    private int ammoReservesSecondary;
    private int ammoReservesTertiary;

    private int magazinePrimary;
    private int magazineSecondary;
    private int magazineTertiary;

    private AbstractWeapon equippedWeapon;
    private string equippedSlot;

    void Start()
    {
        equippedWeapon = primaryWeapon;
        equippedSlot = "Primary";

        ammoReservesPrimary   = primaryWeapon.ammunitionCapacity;
        ammoReservesSecondary = secondaryWeapon.ammunitionCapacity;
        ammoReservesTertiary  = tertiaryWeapon.ammunitionCapacity;

        magazinePrimary   = primaryWeapon.magazineSize;
        magazineSecondary = secondaryWeapon.magazineSize;
        magazineTertiary  = tertiaryWeapon.magazineSize;
    }

    public int roundsInMagazine()
    {
        switch(equippedSlot)
        {
            case "Primary":
                return magazinePrimary;
            case "Secondary":
                return magazineSecondary;
            case "Tertiary":
                return magazineTertiary;
            default:
                return 0;
        }
    }

    public void decrementMagazine(int amount)
    {
        switch (equippedSlot)
        {
            case "Primary":
                magazinePrimary -= amount;
                return;
            case "Secondary":
                magazineSecondary -= amount;
                return;
            case "Tertiary":
                magazineTertiary -= amount;
                return;
            default:
                return;
        }
    }

    public void equipWeapon(string slot)
    {
        switch (slot)
        {
            case "Secondary":
                equippedWeapon = secondaryWeapon;
                equippedSlot = "Secondary";
                return;

            case "Tertiary":
                equippedWeapon = tertiaryWeapon;
                equippedSlot = "Tertiary";
                return;

            case "Primary":
            default:
                equippedWeapon = primaryWeapon;
                equippedSlot = "Primary";
                return;
        }
    }

    public void reloadWeapon()
    {
        int roundsNeeded = 0;
        switch (equippedSlot)
        {

            case "Primary":
                roundsNeeded = primaryWeapon.magazineSize - magazinePrimary;
                if ((ammoReservesPrimary - roundsNeeded) >= 0)
                {
                    magazinePrimary += roundsNeeded;
                    ammoReservesPrimary -= roundsNeeded;
                }
                else
                {
                    magazinePrimary += ammoReservesPrimary;
                    ammoReservesPrimary = 0;
                }
                return;

            case "Secondary":
                roundsNeeded = secondaryWeapon.magazineSize - magazineSecondary;
                if ((ammoReservesSecondary - roundsNeeded) >= 0)
                {
                    magazineSecondary += roundsNeeded;
                    ammoReservesSecondary -= roundsNeeded;
                }
                else
                {
                    magazineSecondary += ammoReservesSecondary;
                    ammoReservesSecondary = 0;
                }
                return;

            case "Tertiary":
                roundsNeeded = tertiaryWeapon.magazineSize - magazineTertiary;
                if ((ammoReservesTertiary - roundsNeeded) >= 0)
                {
                    magazineTertiary += roundsNeeded;
                    ammoReservesTertiary -= roundsNeeded;
                }
                else
                {
                    magazineTertiary += ammoReservesTertiary;
                    ammoReservesTertiary = 0;
                }
                return;

            default:
                return;
        }
    }

    public float hipAimAngle()
    {
        float val = (0.5f * equippedWeapon.hipAccuracy);
        return Random.Range(-val, +val);
    }

    public float adsAimAngle()
    {
        float val = (0.5f * equippedWeapon.adsAccuracy);
        return Random.Range(-val, +val);
    }

    /* Getters and Setters */

    public int AmmoReservesPrimary
    {
        get
        {
            return ammoReservesPrimary;
        }

        set
        {
            ammoReservesPrimary = value;
        }
    }

    public int AmmoReservesSecondary
    {
        get
        {
            return ammoReservesSecondary;
        }

        set
        {
            ammoReservesSecondary = value;
        }
    }

    public int AmmoReservesTertiary
    {
        get
        {
            return ammoReservesTertiary;
        }

        set
        {
            ammoReservesTertiary = value;
        }
    }

    public int MagazinePrimary
    {
        get
        {
            return magazinePrimary;
        }

        set
        {
            magazinePrimary = value;
        }
    }

    public int MagazineSecondary
    {
        get
        {
            return magazineSecondary;
        }

        set
        {
            magazineSecondary = value;
        }
    }

    public int MagazineTertiary
    {
        get
        {
            return magazineTertiary;
        }

        set
        {
            magazineTertiary = value;
        }
    }

    public AbstractWeapon EquippedWeapon
    {
        get
        {
            return equippedWeapon;
        }
    }
}
