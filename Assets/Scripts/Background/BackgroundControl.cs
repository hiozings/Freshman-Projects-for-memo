using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundControl : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform player;            // 玩家引用
    public Vector2 followRatio = new Vector2(0.5f, 0.3f); // XY轴跟随比例
    public Vector3 offset = Vector3.zero; // 位置偏移

    private Vector3 initialBackgroundPos;
    private Vector3 initialPlayerPos;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        initialBackgroundPos = transform.position;
        initialPlayerPos = player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 计算玩家相对于初始位置的移动量
        Vector3 playerMovement = player.position - initialPlayerPos;

        // 按比例计算背景应该移动的距离
        Vector3 backgroundMovement = new Vector3(
            playerMovement.x * followRatio.x,
            playerMovement.y * followRatio.y,
            0
        );

        // 应用移动（保持初始偏移）
        transform.position = initialBackgroundPos + backgroundMovement + offset;
    }

    // 动态更新初始位置（用于场景切换等）
    public void UpdateReferencePosition()
    {
        initialPlayerPos = player.position;
        initialBackgroundPos = transform.position - offset;
    }
}
