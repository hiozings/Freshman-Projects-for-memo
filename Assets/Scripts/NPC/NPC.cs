using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : MonoBehaviour
{

    private AudioSource audioSource;

    public AudioClip emergeFX;
    public AudioClip fadeFX;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void NPCFade()
    {
        Destroy(this.gameObject);
    }

    public void PlayEmergeFX()
    {
        audioSource.clip = emergeFX;
        audioSource.Play();
    }

    public void PlayFadeFX()
    {
        audioSource.clip = fadeFX;
        audioSource.Play();
    }
}
