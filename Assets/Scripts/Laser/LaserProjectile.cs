using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("激光属性")]
    public int damage = 10;
    public Transform target;
    public float searchRadius = 1f; // 检测碰撞的半径
    public LayerMask enemyLayer;

    [Header("视觉效果")]
    public GameObject hitEffect;

    private Attack attack;

    private void Awake()
    {
       attack = GetComponent<Attack>();
    }

    private void Update()
    {
        // 检测碰撞
        CheckCollision();
    }

    private void CheckCollision()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            Debug.Log(hitCollider);
            // 如果击中目标敌人
            if (target != null && hitCollider.transform == target)
            {
                OnHitEnemy(hitCollider);
                break;
            }
            // 或者击中任何敌人（可选）
            else if (target == null)
            {
                OnHitEnemy(hitCollider);
                break;
            }
        }
    }

    private void OnHitEnemy(Collider2D enemyCollider)
    {
        // 应用伤害
        Character enemyCharacter = enemyCollider.GetComponent<Character>();
        if (enemyCharacter != null)
        {
            attack.damage = damage;
            enemyCharacter.TakeDamage(attack);
        }

        // 播放击中效果
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 销毁激光
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
