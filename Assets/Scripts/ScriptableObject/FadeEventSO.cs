using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/FadeEventSO")]
public class FadeEventSO : ScriptableObject
{
    public UnityAction<float, float> OnEventRaised;


    public void RaiseEvent(float alpha, float duration)
    {
        OnEventRaised?.Invoke(alpha, duration);
    }
}
