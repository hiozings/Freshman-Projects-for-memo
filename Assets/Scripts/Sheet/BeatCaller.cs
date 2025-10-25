using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatCaller : MonoBehaviour
{
    public bool enableBeat;
    public GameObject sheetJudge;
    private void OnEnable()
    {
        BeatManager.Instance.OnSheetOpen += ActivateSheet;
        Debug.Log("listen");
    }

    private void OnDisable()
    {
        BeatManager.Instance.OnSheetOpen -= ActivateSheet;
    }

    private void ActivateSheet()
    {
        sheetJudge.SetActive(true);
        Debug.Log("sheet");
    }

    public void SetOnBeat(bool state)
    {
        BeatManager.Instance.SetOnBeat(state);
    }
}
