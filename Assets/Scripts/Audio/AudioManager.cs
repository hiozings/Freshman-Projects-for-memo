using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public PlayAudioEventSO FXEvent;
    public PlayAudioEventSO BGMEvent;
    public PlayAudioEventSO pianoEvent;
    public PlayAudioEventSO drumEvent;

    public AudioSource BGMSource;
    public AudioSource FXSource;
    public AudioSource pianoSource;
    public AudioSource drumSource;

    private void OnEnable()
    {
        FXEvent.OnEventRaised += OnFXEvent;

    }

    private void OnDisable()
    {
        FXEvent.OnEventRaised -= OnFXEvent;
    }

    private void OnFXEvent(AudioClip clip)
    {
        FXSource.clip = clip;
        FXSource.Play();
    }
}
