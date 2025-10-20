using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead;
    private Animator anim;
    private CapsuleCollider2D capsuleCollider;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    public virtual void EnemyDead()
    {
        if(isDead) return;
        isDead = true;
        anim.SetBool("isDead", true);
        capsuleCollider.enabled = false;

    }

    public void DestroyAfterAnimation()
    {
        Destroy(this.gameObject);

    }
}
