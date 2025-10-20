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
        if (currentHealth - attacker.damage > 0)
        {
            currentHealth -= attacker.damage;
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker.transform);
            //PlayAudioEvent.RaiseEvent(takeDamageFX);
        }
        else
        {
            currentHealth = 0;
            OnDie?.Invoke();
            //PlayAudioEvent.RaiseEvent(dieFX);
        }
        OnHealthChange?.Invoke(this);
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
