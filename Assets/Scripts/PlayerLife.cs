﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerLife : NetworkBehaviour
{
    [Tooltip("Amount of health")]
    public float maxHealth = 95.0f;
    [Tooltip("Health recovered per second")]
    public float healthRecoveryRate = 30.0f;
    [Tooltip("Time to start recovering health (seconds)")]
    public float healthRecoveryDelay = 7.0f;

    [Tooltip("Amount of shield")]
    public float maxShield = 95.0f;
    [Tooltip("Shield recovered per second")]
    public float shieldRecoveryRate = 30.0f;
    [Tooltip("Time to start recovering shield (seconds)")]
    public float shieldRecoveryDelay = 5.0f;

    [SyncVar]
    public float currentHealth;
    private bool isHealthDamaged;
    private float timeHealthUndamaged;

    [SyncVar]
    public float currentShield;
    private bool isShieldDamaged;
    private float timeShieldUndamaged;

    public RectTransform healthBar;
    public RectTransform shieldBar;
    public Text textHealth;
    public Text textSheild;

    void Start()
    {
        currentHealth = maxHealth;
        isHealthDamaged = false;
        timeHealthUndamaged = float.PositiveInfinity;

        currentShield = maxShield;
        isShieldDamaged = false;
        timeShieldUndamaged = float.PositiveInfinity;
    }

    void Update()
    {
        /*if(isHealthDamaged || isShieldDamaged)
        {
            Debug.Log(this.transform.name + " has " +
            shield.ToString("F2") + " shield and " +
            health.ToString("F2") + " health.");
        }*/

        /* Display health/shield */
        healthBar.sizeDelta = new Vector2(
            (currentHealth / maxHealth) * 100,
            healthBar.sizeDelta.y);

        shieldBar.sizeDelta = new Vector2(
            (currentShield / maxShield) * 100,
            shieldBar.sizeDelta.y);

        textHealth.text = currentHealth.ToString("F0");
        textSheild.text = currentShield.ToString("F0");

        /* Health recovery */
        if (isHealthDamaged)
        {
            if(timeHealthUndamaged >= healthRecoveryDelay)
            {
                currentHealth += healthRecoveryRate * Time.deltaTime;
                if(currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;
                    isHealthDamaged = false;
                    timeHealthUndamaged = float.PositiveInfinity;
                }
            }
            else { timeHealthUndamaged += Time.deltaTime; }
        }
        /* Shield recovery */
        else if(isShieldDamaged)
        {
            if(timeShieldUndamaged >= shieldRecoveryDelay)
            {
                currentShield += shieldRecoveryRate * Time.deltaTime;
                if(currentShield >= maxShield)
                {
                    currentShield = maxShield;
                    isShieldDamaged = false;
                    timeShieldUndamaged = float.PositiveInfinity;
                }
            }
            else { timeShieldUndamaged += Time.deltaTime; }
        }
    }

    [Command]
    public void Cmd_applyDamage(float damage)
    {
        /* Apply damage only on Server */
        if (!isServer) { return; }

        /* Ignore damage of 0.0 or less */
        if(damage <= 0.0f) { return; }

        Debug.Log(transform.name + " takes " + damage.ToString("F2") + " damage.");

        /* Apply damage to shield */
        currentShield -= damage;
        isShieldDamaged = true;
        timeShieldUndamaged = 0.0f;
        if (currentShield < 0)
        {
            damage = Mathf.Abs(currentShield);
            currentShield = 0.0f;
        }
        else{ damage = 0.0f; }

        /* Ignore remaining damage of 0.0 or less */
        if (damage <= 0.0f) { return; }

        /* Apply remaining damage to health */
        currentHealth -= damage;
        isHealthDamaged = true;
        timeHealthUndamaged = 0.0f;

        /* Player has less than 0.5 health */
        if (currentHealth < +0.5f)
        {
            currentHealth = 0.0f;
            handleDeath();
        }
    }

    private void handleDeath()
    {
        Debug.Log("Player Died.");
    }
}