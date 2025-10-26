using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public PlayAudioEventSO FXEvent;
    public PlayAudioEventSO BGMEvent;
    public PlayAudioEventSO pianoEvent;
    public PlayAudioEventSO drumEvent;
    public VoidEventSO stopAudioEvent;

    public AudioSource BGMSource;
    public AudioSource FXSource;
    public AudioSource pianoSource;
    public AudioSource drumSource;

    private void OnEnable()
    {
        FXEvent.OnEventRaised += OnFXEvent;
        BGMEvent.OnEventRaised += OnBGM;
        stopAudioEvent.OnEventRaised += OnStop;
    }

    private void OnDisable()
    {
        FXEvent.OnEventRaised -= OnFXEvent;
        BGMEvent.OnEventRaised -= OnBGM;
        stopAudioEvent.OnEventRaised -= OnStop;
    }

    private void OnStop()
    {
        //BGMSource.Stop();
        BGMSource.DOFade(0f, 3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                BGMSource.Stop();
                BGMSource.volume = 1f; // 重置音量，便于下次播放
                Debug.Log("BGM渐出停止完成");
            });
    }

    private void OnBGM(AudioClip clip)
    {
        BGMSource.clip = clip;
        BGMSource.Play();
    }

    private void OnFXEvent(AudioClip clip)
    {
        FXSource.clip = clip;
        FXSource.Play();
        CancelInvoke(nameof(StopFX));

        // 在指定时间后停止
        Invoke(nameof(StopFX), 1.6f);
    }

    private void StopFX()
    {
        if (FXSource.isPlaying)
        {
            FXSource.Stop();
        }
    }
}
