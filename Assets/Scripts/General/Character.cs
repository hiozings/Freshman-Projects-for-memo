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
        if (currentShield >= d)
        {
            currentShield -= d;
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker.transform);
        }
        else
        {
            currentShield = 0;
            d -= currentShield;

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

    private void TriggerInvulnerable()
    {
        if (!invulnerable)
        {
            invulnerable = true;
            invulnerableConunter = invulnerableDuration;
        }
    }
}
