using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead;
    private Animator anim;
    private CapsuleCollider2D capsuleCollider;

    [Header("受伤特效")]
    public float hurtFlashDuration = 0.5f; // 受伤闪烁总时长
    public float hurtFlashInterval = 0.1f; // 闪烁间隔
    public Color hurtColor = Color.red; // 受伤时的颜色
    private Color _originalColor;
    private bool _isFlashing = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = spriteRenderer.color;
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

    public void GetHurt()
    {
        if (_isFlashing) return; // 如果已经在闪烁，不再重复触发

        StartCoroutine(HurtFlashCoroutine());
    }

    public IEnumerator HurtFlashCoroutine()
    {
        _isFlashing = true;
        float timer = 0f;
        bool showHurtColor = true;

        while (timer < hurtFlashDuration)
        {
            // 颜色闪烁
            spriteRenderer.color = showHurtColor ? hurtColor : _originalColor;
            showHurtColor = !showHurtColor;

            //// 震动效果
            //if (enableShake)
            //{
            //    Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            //    transform.position = _originalPosition + shakeOffset;
            //}

            yield return new WaitForSeconds(hurtFlashInterval);
            timer += hurtFlashInterval;
        }

        // 恢复原始状态
        spriteRenderer.color = _originalColor;
        //transform.position = _originalPosition;
        _isFlashing = false;
    }
}
