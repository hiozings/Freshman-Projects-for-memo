using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour
{
    public List<Image> powers;
    public Sprite starsOn;
    public Sprite starsOff;

    public void ChangePowerBar(Character character)
    {
        int currentPower = character.currentPower;
        int maxPower = character.maxPower;
        for (int i = 0; i < currentPower; i++)
        {
            powers[i].sprite = starsOn;
        }
        for (int i = currentPower; i < maxPower; i++)
        {
            powers[i].sprite = starsOff;
        }
    }

    public void ChangeMaxPower()
    {
        for(int i = 6; i < powers.Count; i++)
        {
            powers[i].gameObject.SetActive(true);
        }
    }
}
