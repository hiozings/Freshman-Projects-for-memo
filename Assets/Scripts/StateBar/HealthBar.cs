using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public List<Image> stars;
    public Sprite starsOn;
    public Sprite starsOff;


    public void ChangeHealthBar(Character character)
    {
        int curentHealth = character.currentHealth;
        int maxHealth = character.maxHealth;
        for(int i = 0; i < curentHealth ; i++)
        {
            stars[i].sprite = starsOn;
        }
        for(int i = curentHealth; i <  maxHealth; i++)
        {
            stars[i].sprite = starsOff;
        }
    }

    public void ChangeMaxHealth()
    {
        for(int i = 10; i < stars.Count; i++)
        {
            stars[i].gameObject.SetActive(true);
        }

    }
}
