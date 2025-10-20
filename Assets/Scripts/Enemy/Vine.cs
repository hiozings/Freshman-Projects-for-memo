using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : Enemy
{
    [Header("攻击参数")]
    public float attackRange; // 敌人攻击范围（米）
    public float attackInterval; // 攻击间隔（秒），每过该时间生成一次荆棘
    public float checkPlayerInterval; // 检测玩家是否在范围内的间隔（优化性能）
    public bool canAttack;

    [Header("荆棘配置")]
    public GameObject thornPrefab; // 荆棘预制体（拖拽赋值）
    public float thornLifetime; // 荆棘存在时间（秒），超时自动销毁
    public Vector3 thornOffset; // 荆棘生成位置偏移（避免埋入地面）

    [Header("目标引用")]
    public Transform playerTransform; // 玩家Transform（拖拽赋值，或自动查找）

    private float _attackTimer; // 攻击间隔计时器
    private float _checkRangeTimer; // 范围检测计时器
    private bool _isPlayerInRange; // 玩家是否在攻击范围内

    private void Start()
    {
        if(playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        _attackTimer = attackInterval; 
        _checkRangeTimer = 0;
        _isPlayerInRange = false;
    }

    private void Update()
    {
        if(canAttack)
        {
            if (playerTransform == null) return;

            // 定时检测玩家是否在攻击范围内（降低检测频率优化性能）
            _checkRangeTimer += Time.deltaTime;
            if (_checkRangeTimer >= checkPlayerInterval)
            {
                CheckPlayerInRange();
                _checkRangeTimer = 0;
            }

            // 玩家在范围内时，启动攻击计时器
            if (_isPlayerInRange)
            {
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= attackInterval)
                {
                    SpawnThorn(); // 生成荆棘
                    _attackTimer = 0; // 重置计时器
                }
            }
        }
    }

    private void CheckPlayerInRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        _isPlayerInRange = distanceToPlayer <= attackRange;
    }

    private void SpawnThorn()
    {
        if (thornPrefab == null)
        {
            Debug.LogError("未赋值荆棘预制体！请给thornPrefab拖拽荆棘GameObject");
            return;
        }

        Vector3 spawnPosition = playerTransform.position + thornOffset;
        spawnPosition.y = -0.3f;
        GameObject thorn = Instantiate(thornPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Spawn");
        //Destroy(thorn, thornLifetime);
        // PlayThornSpawnEffect(spawnPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // 红色线框表示攻击范围
    }
}
