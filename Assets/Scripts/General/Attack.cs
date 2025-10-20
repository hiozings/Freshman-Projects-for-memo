using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int damage;
    public float attackRange;
    public float attackRate;
    public bool disenableAfterAttack;
    public bool noTrigger;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!noTrigger)
        {
            Debug.Log("Attack");
            Debug.Log(collision);
            Debug.Log(this);
            collision.GetComponent<Character>()?.TakeDamage(this);
            if (disenableAfterAttack)
            {
                damage = 0;
            }
        }
    }
}
