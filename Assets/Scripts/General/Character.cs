using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    public List<AudioClip> takeDamageFXs;
    public AudioClip shieldDamageFX;
    public AudioClip dieFX;
    public AudioClip buildShieldFX;

    public int maxHealth;
    public int currentHealth;
    //public int maxShield;
    public int currentShield;
    public int permanentShield;
    public int maxPower;
    public int currentPower;
    public float invulnerableDuration;
    public float invulnerableConunter;
    public bool invulnerable;

    public GameObject shieldPrefab;
    public GameObject manaPrefab;
    public GameObject shieldObj;
    private GameObject manaObj;
    public Transform shieldTrans;

    public UnityEvent<Character> OnHealthChange;
    public UnityEvent<Character> OnPowerChange;
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    public PlayAudioEventSO playAudioEvent;

    private void Awake()
    {
        currentHealth = maxHealth;
        //currentShield = maxShield;
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
            playAudioEvent.RaiseEvent(shieldDamageFX);
            return;
        }
        else if(permanentShield > 0)
        {
            d -= permanentShield;
            permanentShield = 0;
            playAudioEvent.RaiseEvent(shieldDamageFX);
            if(currentShield == 0 && shieldObj != null)
            {
                Destroy(shieldObj);
            }
        }
        if (currentShield >= d)
        {
            currentShield -= d;
            if(currentShield <= 0 && shieldObj != null)
            {
                Destroy(shieldObj);
            }
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker.transform);
            playAudioEvent.RaiseEvent(shieldDamageFX);
        }
        else
        {
            d -= currentShield;
            currentShield = 0;
            if(shieldObj != null)
            {
                Destroy(shieldObj);
            }
            if (currentHealth - d > 0)
            {
                currentHealth -= d;
                TriggerInvulnerable();
                OnTakeDamage?.Invoke(attacker.transform);
                //PlayAudioEvent.RaiseEvent(takeDamageFX);
                OnHealthChange?.Invoke(this);
                //playAudioEvent?.RaiseEvent(takeDamageFX1);
            }
            else
            {
                currentHealth = 0;
                OnHealthChange?.Invoke(this);
                OnDie?.Invoke();
                //PlayAudioEvent.RaiseEvent(dieFX);
                //playAudioEvent.RaiseEvent(takeDamageFX2);
            }
            playAudioEvent?.RaiseEvent(chooseFX());
        }
    }

    private AudioClip chooseFX()
    {
        int randomNum = Random.Range(0, 3);
        return takeDamageFXs[randomNum];
    }

    public void AddMaxHealth(int health)
    {
        maxHealth += health;
        currentHealth = maxHealth - 3;
        OnHealthChange?.Invoke(this);
    }

    public void AddMaxPower(int power)
    {
        FormMana();
        maxPower += power;
        currentPower = maxPower;
        OnPowerChange?.Invoke(this);
    }

    public void AddPermanentShield(int shield)
    {
        permanentShield = shield;
        if (currentShield == 0)
        {
            FormShield();
        }
        playAudioEvent.RaiseEvent(buildShieldFX);

    }

    public void AddShield(int shield)
    {
        if(currentShield == 0)
        {

            Debug.Log("add shield");
            FormShield();
        }
        currentShield += shield;
        playAudioEvent.RaiseEvent(buildShieldFX);
        //Debug.Log("play fx");
        //if(currentShield > maxShield)
        //{
        //    currentShield = maxShield;
        //}
    }

    public void FormShield()
    {
        if (shieldObj != null) return;
        shieldObj = Instantiate(shieldPrefab, shieldTrans.position, Quaternion.identity);
        shieldObj.transform.SetParent(transform);
        Debug.Log("SHIELD ON");
        if (shieldObj == null) Debug.Log("no shieldObj");
    }

    public void AddPower(int power)
    {
        FormMana();
        currentPower += power;
        if(currentPower > maxPower)
        {
            currentPower = maxPower;
        }
        OnPowerChange?.Invoke(this);
    }

    public void FormMana()
    {
        if(manaObj != null) return;
        manaObj = Instantiate(manaPrefab, shieldTrans.position, Quaternion.identity);
        manaObj.transform.SetParent(transform);
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
