using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mana : MonoBehaviour
{
    public void DestroyAfterAnim()
    {
        Destroy(this.gameObject);
    }
}
