using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour
{
    public GameObject Cards;
    private bool canTouch;

    private SpriteRenderer spriteRenderer;

    public Sprite afterTouch;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        canTouch = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(canTouch)
        {
            
            Cards.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Cards.SetActive(false);
    }

    public void UsedFountain()
    {
        canTouch = false;
        spriteRenderer.sprite = afterTouch;
    }
}
