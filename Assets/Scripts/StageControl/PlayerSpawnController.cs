using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpawnController : MonoBehaviour
{
    [Header("核心组件引用")]
    private Rigidbody2D rb;                      // 主角刚体（控制物理）
    private Animator anim;                      // 主角动画器（倒地/起身/移动动画）
    public TextMeshProUGUI tutorialTMP;        // 教学文本（TextMeshPro，弹字幕用）
    public GameObject npcPrefab;               // NPC预制体（对话用）
    public Transform npcSpawnPos;              // NPC生成位置（场景中指定）
    public Transform nextSectionTrigger;       // 第一段终点触发点（进入“承”阶段用）
    public CinemachineVirtualCamera vCam;      // Cinemachine 2D摄像机（主角始终在屏幕中）

    [Header("流程参数配置")]
    public float standUpDuration = 2f;         // 起身动画时长（秒，匹配“缓缓站立”）
    public float textFlickerInterval = 0.5f;   // 起身时画面闪烁间隔（秒）
    public string[] npcDialogues = {           // NPC对话（按策划“交代基本设定”填写）
        "欢迎来到音乐世界！",
        "按A/D键左右移动，按空格跳跃",
        "遇到藤蔓记得灵活躲避～"
    };
    public float dialogueInterval = 2.5f;      // NPC对话间隔（秒，控制语速）
    public Vector3 vCamLockPos;                // 下一关卡摄像机锁定位置（策划“镜头移到关卡中心”）
    public float camMoveDuration = 1.5f;

    [Header("状态标记")]
    public bool isStandingUp = false;          // 是否正在起身
    public bool hasTriedMove = false;          // 是否尝试过AD移动
    public bool hasTriedJump = false;          // 是否尝试过空格跳跃
    public bool isNpcTalking = false;          // NPC是否在对话
    public bool isSectionDone = false;         // 第一段是否完成
    public bool hasNpcTalked = false;
    public bool isWaitingForStandUp = true;
    public bool isCamMoving = false;

    private Color originalTextColor;           // 教学文本原始颜色（用于闪烁效果）
    private GameObject spawnedNPC;             // 生成的NPC实例


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // 初始化初始状态（匹配策划“主角出生在左侧，倒地状态”）
        originalTextColor = tutorialTMP.color;
        rb.bodyType = RigidbodyType2D.Static;  // 倒地时禁用物理（避免移动）
        //anim.Play("Player_LieDown");           // 播放倒地动画
        tutorialTMP.gameObject.SetActive(false);
        SetCinemachineFollow(true);            // 开启摄像机跟随（主角始终在屏幕中）

        // 启动第一段核心流程：等待空格起身
        StartCoroutine(SpawnInitCoroutine());
    }

    void Update()
    {
        if (isSectionDone) return;  // 流程完成后不响应输入

        // 起身完成后，检测玩家操作（触发教学）
        if (!isStandingUp && !isNpcTalking && !isWaitingForStandUp)
        {
            CheckTutorialInput();
        }

        // 玩家完成所有操作尝试后，生成NPC对话（仅一次）
        if (!isNpcTalking && !hasNpcTalked &&hasTriedMove && hasTriedJump && spawnedNPC == null)
        {
            Debug.Log("NPC Spawn");
            StartCoroutine(StartNpcDialogue());
        }
    }

    IEnumerator SpawnInitCoroutine()
    {
        // 显示“按空格起身”提示
        tutorialTMP.text = "按【空格】起身";
        tutorialTMP.gameObject.SetActive(true);

        // 等待玩家按下空格（旧Input System）
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // 起身流程：播放动画+画面闪烁（文本闪烁模拟画面效果）
        isWaitingForStandUp = false;
        isStandingUp = true;
        tutorialTMP.text = "正在起身...";
        //anim.Play("Player_StandUp");           // 播放起身动画
        rb.bodyType = RigidbodyType2D.Dynamic; // 恢复物理（起身完成后可移动）

        float standUpTimer = 0;
        while (standUpTimer < standUpDuration)
        {
            // 文本在“显示-隐藏”间切换，模拟画面闪烁
            tutorialTMP.color = tutorialTMP.color.a == 0
                ? originalTextColor
                : new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0);
            yield return new WaitForSeconds(textFlickerInterval);
            standUpTimer += textFlickerInterval;
        }

        // 起身完成：重置文本，显示操作教学（策划“教玩家AD移动，空格跳跃”）
        isStandingUp = false;
        tutorialTMP.color = originalTextColor;
        tutorialTMP.text = "按【A/D】移动 | 按【空格】跳跃";
    }

    void CheckTutorialInput()
    {
        // 检测AD移动（只要输入就标记“已尝试”）
        if (!hasTriedMove && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            hasTriedMove = true;
            //anim.Play("Player_Move");  // 播放移动动画
            UpdateTutorialText();      // 更新教学提示
        }

        // 检测空格跳跃（按下就标记“已尝试”）
        if (!hasTriedJump && Input.GetKeyDown(KeyCode.Space))
        {
            hasTriedJump = true;
            //anim.Play("Player_Jump");  // 播放跳跃动画
            UpdateTutorialText();      // 更新教学提示
        }
    }

    IEnumerator StartNpcDialogue()
    {
        isNpcTalking = true;
        hasNpcTalked = true;
        tutorialTMP.text = $"NPC：{npcDialogues[0]}";  // 显示第一句对话

        // 生成NPC并播放入场动画
        spawnedNPC = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        Animator npcAnim = spawnedNPC.GetComponent<Animator>();
        //if (npcAnim != null)
        //{
        //    npcAnim.Play("NPC_WalkIn");  // NPC入场动画
        //}

        // 等待入场动画完成（避免对话与动画不同步）
        yield return new WaitForSeconds(1.5f);

        // 逐句播放NPC对话
        for (int i = 0; i < npcDialogues.Length; i++)
        {
            tutorialTMP.text = $"NPC：{npcDialogues[i]}";
            yield return new WaitForSeconds(dialogueInterval);
        }

        // NPC退场（策划“NPC退场”）
        //if (npcAnim != null)
        //{
        //    npcAnim.Play("NPC_WalkOut");  // NPC退场动画
        //}
        tutorialTMP.text = "NPC：继续前进吧！";
        npcAnim.SetTrigger("fade");
        yield return new WaitForSeconds(1f);
        // 清理NPC与教学文本
        //Destroy(spawnedNPC, 1f);  // 等待退场动画完成后销毁
        tutorialTMP.gameObject.SetActive(false);
        isNpcTalking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == nextSectionTrigger && !isSectionDone && !isCamMoving)
        {
            isCamMoving = true;
            isSectionDone = true;
            SetCinemachineFollow(false);              // 关闭摄像机跟随
            //vCam.transform.position = vCamLockPos;    // 移动镜头到关卡中心
            StartCoroutine(SmoothMoveCameraToLockPos());
            //tutorialTMP.gameObject.SetActive(true);
            //tutorialTMP.text = "准备进入下一区域...";
            //Debug.Log("第一段“起”完成，触发“承”阶段");

        }
    }

    private IEnumerator SmoothMoveCameraToLockPos()
    {
        Vector3 startCamPos = vCam.transform.position;
        Vector3 targetCamPos = vCamLockPos;
        float elapsedTime = 0f;

        while (elapsedTime < camMoveDuration)
        {
            // 计算移动进度（0→1，Lerp插值用）
            float t = elapsedTime / camMoveDuration;
            // 平滑因子（可选：添加缓动效果，让移动先慢后快再慢，更自然）
            t = Mathf.SmoothStep(0f, 1f, t);

            // 逐帧更新镜头位置
            vCam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t);
            elapsedTime += Time.deltaTime;

            yield return null; // 等待下一帧，确保平滑
        }

        vCam.transform.position = targetCamPos;
        isCamMoving = false;
    }

    void UpdateTutorialText()
    {
        if (hasTriedMove && hasTriedJump)
        {
            tutorialTMP.text = "操作完成！NPC即将到来...";
        }
        else if (hasTriedMove)
        {
            tutorialTMP.text = "已尝试移动！再按【空格】跳跃";
        }
        else if (hasTriedJump)
        {
            tutorialTMP.text = "已尝试跳跃！再按【A/D】移动";
        }
    }

    void SetCinemachineFollow(bool isFollow)
    {
        vCam.Follow = isFollow ? transform : null;  // 正确：控制跟随目标
        vCam.LookAt = isFollow ? transform : null;  // 正确：控制看向目标
    }

    //IEnumerator HideSurpriseText(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    tutorialTMP.gameObject.SetActive(false);
    //}
}
