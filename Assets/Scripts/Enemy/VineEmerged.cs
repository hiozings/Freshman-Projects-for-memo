using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineEmerged : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayEmergeFX()
    {
        audioSource.Play();
    }

    public void DeadAfterAttack()
    {
        Destroy(this.gameObject);
    }
}
