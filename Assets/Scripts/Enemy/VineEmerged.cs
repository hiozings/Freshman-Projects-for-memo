using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineEmerged : MonoBehaviour
{
    public void DeadAfterAttack()
    {
        Destroy(this.gameObject);
    }
}
