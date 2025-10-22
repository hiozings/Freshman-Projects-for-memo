using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SheetJudgeEventSO")]
public class SheetJudgeEventSO : ScriptableObject
{
    public UnityAction<char, string> OnEventRaised;

    public void RaiseEvent(char comm, string precision)
    {
        OnEventRaised?.Invoke(comm, precision);
    }
}
