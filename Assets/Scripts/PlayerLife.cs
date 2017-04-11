using System.Collections;
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

    private float health;
    private bool isHealthDamaged;
    private float timeHealthUndamaged;

    private float shield;
    private bool isShieldDamaged;
    private float timeShieldUndamaged;

    public RectTransform healthBar;
    public RectTransform shieldBar;
    public Text textHealth;
    public Text textSheild;

    void Start()
    {
        health = maxHealth;
        isHealthDamaged = false;
        timeHealthUndamaged = float.PositiveInfinity;

        shield = maxShield;
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

        healthBar.sizeDelta = new Vector2(
            (health / maxHealth) * 100,
            healthBar.sizeDelta.y);

        shieldBar.sizeDelta = new Vector2(
            (shield / maxShield) * 100,
            shieldBar.sizeDelta.y);

        textHealth.text = health.ToString("F0");
        textSheild.text = shield.ToString("F0");

        /* Health recovery */
        if (isHealthDamaged)
        {
            if(timeHealthUndamaged >= healthRecoveryDelay)
            {
                health += healthRecoveryRate * Time.deltaTime;
                if(health >= maxHealth)
                {
                    health = maxHealth;
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
                shield += shieldRecoveryRate * Time.deltaTime;
                if(shield >= maxShield)
                {
                    shield = maxShield;
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
        /* Ignore damage of 0.0 or less */
        if(damage <= 0.0f) { return; }

        Debug.Log(transform.name + " takes " + damage.ToString("F2") + " damage.");

        /* Apply damage to shield */
        shield -= damage;
        isShieldDamaged = true;
        timeShieldUndamaged = 0.0f;
        if (shield < 0)
        {
            damage = Mathf.Abs(shield);
            shield = 0.0f;
        }
        else{ damage = 0.0f; }

        /* Ignore remaining damage of 0.0 or less */
        if (damage <= 0.0f) { return; }

        /* Apply remaining damage to health */
        health -= damage;
        isHealthDamaged = true;
        timeHealthUndamaged = 0.0f;

        /* Player has less than 0.5 health */
        if (health < +0.5f)
        {
            health = 0.0f;
            handleDeath();
        }
    }

    private void handleDeath()
    {
        Debug.Log("Player Died.");
    }
}
