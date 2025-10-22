using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    public AudioClip takeDamageFX;
    public AudioClip dieFX;

    public int maxHealth;
    public int currentHealth;
    public int maxShield;
    public int currentShield;
    public int permanentShield;
    public int maxPower;
    public int currentPower;
    public float invulnerableDuration;
    public float invulnerableConunter;
    public bool invulnerable;

    public UnityEvent<Character> OnHealthChange;
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    public PlayAudioEventSO PlayAudioEvent;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
        currentPower = maxPower;
    }

    private void Update()
    {
        if (invulnerable)
        {
            invulnerableConunter -= Time.deltaTime;
            if (invulnerableConunter <= 0)
            {
                invulnerable = false;
            }
        }
    }

    public void TakeDamage(Attack attacker)
    {
        if (invulnerable)
            return;
        //Debug.Log(attacker.damage);
        int d = attacker.damage;
        if(permanentShield >= d)
        {
            permanentShield -= d;
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker.transform);
            return;
        }
        else if(permanentShield > 0)
        {
            d -= permanentShield;
            permanentShield = 0;
        }
        if (currentShield >= d)
        {
            currentShield -= d;
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker.transform);
        }
        else
        {
            d -= currentShield;
            currentShield = 0;

            if (currentHealth - d > 0)
            {
                currentHealth -= d;
                TriggerInvulnerable();
                OnTakeDamage?.Invoke(attacker.transform);
                //PlayAudioEvent.RaiseEvent(takeDamageFX);
                OnHealthChange?.Invoke(this);
            }
            else
            {
                currentHealth = 0;
                OnHealthChange?.Invoke(this);
                OnDie?.Invoke();
                //PlayAudioEvent.RaiseEvent(dieFX);
            }
        }
    }

    public void AddMaxHealth(int health)
    {
        maxHealth += health;
        currentHealth = maxHealth;
    }

    public void AddMaxPower(int power)
    {
        maxPower += power;
        currentPower = maxPower;
    }

    public void AddPermanentShield(int shield)
    {
        permanentShield = shield;

    }

    public void AddShield(int shield)
    {
        currentShield += shield;
        if(currentShield > maxShield)
        {
            currentShield = maxShield;
        }
    }

    public void AddPower(int power)
    {
        currentPower += power;
        if(currentPower > maxPower)
        {
            currentPower = maxPower;
        }
    }

    private void TriggerInvulnerable()
    {
        if (!invulnerable)
        {
            invulnerable = true;
            invulnerableConunter = invulnerableDuration;
        }
    }
}
