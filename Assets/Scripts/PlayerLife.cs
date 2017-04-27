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

    [SyncVar(hook = "OnChangeHealth")]
    public float currentHealth;
    private bool isHealthDamaged;
    private float timeHealthUndamaged;

    [SyncVar(hook ="OnChangeShield")]
    public float currentShield;
    private bool isShieldDamaged;
    private float timeShieldUndamaged;

    public RectTransform healthBar;
    public RectTransform shieldBar;
    public Text textHealth;
    public Text textSheild;
    public GameObject damageTextPrefab;
    public float damageTextLifetime = 2.0f;
    public Color regularDamageTextColor;
    public Color precisionDamageTextColor;

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

    private void OnChangeHealth(float health)
    {
        healthBar.sizeDelta = new Vector2(
            (health / maxHealth) * 100,
            healthBar.sizeDelta.y);

        textHealth.text = health.ToString("F0");
    }

    private void OnChangeShield(float shield)
    {
        shieldBar.sizeDelta = new Vector2(
            (shield / maxShield) * 100,
            shieldBar.sizeDelta.y);

        textSheild.text = shield.ToString("F0");
    }

    [Command]
    public void Cmd_applyDamage(float damage, bool isPrecisionDamage)
    {
        /* Apply damage only on Server */
        if (!isServer) { return; }

        /* Ignore damage of 0.0 or less */
        if(damage <= 0.0f) { return; }

        /* Bad design... hard-coded to prefab */
        GameObject damageTextObj = Instantiate(damageTextPrefab, transform.position, transform.rotation);
        DamageTextUpdate damageScript = damageTextObj.GetComponent<DamageTextUpdate>();
        if (damageScript != null)
        {
            damageScript.textDamage = "-" + damage.ToString("F0");
            Text dmgText = damageTextObj.GetComponentInChildren<Text>();
            if (isPrecisionDamage)
            {
                dmgText.color = precisionDamageTextColor;
                dmgText.fontStyle = FontStyle.Bold;
            }
            else
            {
                dmgText.color = regularDamageTextColor;
            }
        }
        NetworkServer.Spawn(damageTextObj);
        Destroy(damageTextObj, damageTextLifetime);

        //Debug.Log(transform.name + " takes " + damage.ToString("F2") + " damage.");

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
            Rpc_handleDeathRespawn();
        }
    }

    [ClientRpc]
    private void Rpc_handleDeathRespawn()
    {
        if(isLocalPlayer)
        {
            transform.position = Vector3.zero;
        }
        currentShield = maxShield;
        currentHealth = maxHealth;
        isShieldDamaged = false;
        isHealthDamaged = false;
        timeShieldUndamaged = float.PositiveInfinity;
        timeHealthUndamaged = float.PositiveInfinity;
    }
}
