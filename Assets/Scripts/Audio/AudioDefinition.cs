using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDefinition : MonoBehaviour
{
    public PlayAudioEventSO PlayAudioEvent;

    public AudioClip Clip;

    public void PlayAudioClip()
    {
        PlayAudioEvent.RaiseEvent(Clip);
    }
}
