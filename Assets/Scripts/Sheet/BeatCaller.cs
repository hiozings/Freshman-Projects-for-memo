using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatCaller : MonoBehaviour
{
    public bool enableBeat;

    public void SetOnBeat(bool state)
    {
        BeatManager.Instance.SetOnBeat(state);
    }
}
