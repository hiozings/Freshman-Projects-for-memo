using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public void NPCFade()
    {
        Destroy(this.gameObject);
    }
}
