using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Diagnostics;
public class PlayerSpawnController : MonoBehaviour
{
    public FadeEventSO fadeEvent;

    [Header("核心组件引用")]
    private Rigidbody2D rb;                      // 主角刚体（控制物理）
    private Animator anim;                      // 主角动画器（倒地/起身/移动动画）
    public TextMeshProUGUI tutorialTMP;        // 教学文本（TextMeshPro，弹字幕用）
    public GameObject npcPrefab;               // NPC预制体（对话用）
    public Transform npcSpawnPos;              // NPC生成位置（场景中指定）
    public Transform nextSectionTrigger;       // 第一段终点触发点（进入“承”阶段用）
    public CinemachineVirtualCamera vCam;      // Cinemachine 2D摄像机（主角始终在屏幕中）
    public GameObject stateBar;

    [Header("流程参数配置")]
    public float standUpDuration = 2f;         // 起身动画时长（秒，匹配“缓缓站立”）
    public float textFlickerInterval = 0.5f;   // 起身时画面闪烁间隔（秒）
    private string[] npcDialogues = {           // NPC对话（按策划“交代基本设定”填写）
        "NPC: 啊呀…又一个在崩溃的边缘，听见回响的人。",
        "主角：这里…是哪里",
        "NPC：啊…这里被称为...失律之境。是被现实遗忘的乐章，最终的归所。",
        "NPC: 每一个错误的音符，都会变成噬人的怪物。每一次错误的节拍，都会让这个世界扭曲一点…",
        "NPC：你最好…能快点把你的音乐想起来…"

    };
    public float dialogueInterval = 4f;      // NPC对话间隔（秒，控制语速）
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
        tutorialTMP.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");

        // 设置背景颜色
        tutorialTMP.fontSharedMaterial.SetColor("_UnderlayColor", Color.black);
        fadeEvent.RaiseEvent(1f, 0f);
        // 启动第一段核心流程：等待空格起身
        StartCoroutine(SpawnInitCoroutine());
    }

    void Update()
    {
        if (isSectionDone) return;  // 流程完成后不响应输入

        //// 起身完成后，检测玩家操作（触发教学）
        //if (!isStandingUp && !isNpcTalking && !isWaitingForStandUp)
        //{
        //    CheckTutorialInput();
        //}

        // 玩家完成所有操作尝试后，生成NPC对话（仅一次）
        //if (!isNpcTalking && !hasNpcTalked &&hasTriedMove && hasTriedJump && spawnedNPC == null)
        //{
        //    //Debug.Log("NPC Spawn");
        //    StartCoroutine(StartNpcDialogue());
        //}
    }

    public void FadeInText(string str, float duration = 2f)
    {
        if (tutorialTMP == null) return;

        // 确保文本初始状态为完全透明
        Color originalColor = tutorialTMP.color;
        tutorialTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        tutorialTMP.text = str;
        // 执行渐入动画
        tutorialTMP.DOFade(1f, duration)
            .SetEase(Ease.OutQuad);
            
    }

    // 渐出效果
    public void FadeOutText(float duration = 2f)
    {
        if (tutorialTMP == null) return;

        tutorialTMP.DOFade(0f, duration)
            .SetEase(Ease.OutQuad);
            
    }

    IEnumerator SpawnInitCoroutine()
    {
        // 显示“按空格起身”提示
        tutorialTMP.gameObject.SetActive(true);
        //Color originalColor = tutorialTMP.color;
        //tutorialTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        //tutorialTMP.text = "现实的舞台将你驱逐\r\n那份承载希冀的录取信函，终究未能如期而至…\r\n";
        //tutorialTMP.DOFade(1f, 2f).SetEase(Ease.OutQuad);
        //tutorialTMP.DOFade(1f, 2f).SetEase(Ease.OutQuad);
        FadeInText("现实的舞台将你驱逐\r\n那份承载希冀的录取信函，终究未能如期而至…\r\n");
        yield return new WaitForSeconds(5f);
        FadeOutText();
       

        yield return new WaitForSeconds(2f);

        fadeEvent.RaiseEvent(0f, 3f);

        // 起身流程：播放动画+画面闪烁（文本闪烁模拟画面效果）
        isWaitingForStandUp = false;
        isStandingUp = true;

        stateBar.SetActive(true);

        FadeInText("这里是…哪里…");
        FadeOutText();

        FadeInText("按下space起身");
        //等待玩家按下空格（旧Input System）
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        FadeOutText();

        FadeInText("身体…好重…要喘不过气来了");
        FadeOutText();

        rb.bodyType = RigidbodyType2D.Dynamic; // 恢复物理（起身完成后可移动）
        FadeInText("按下a d移动，space跳跃");

        yield return new WaitForSeconds(5f);

        FadeOutText();

        yield return new WaitForSeconds(2f);
        isStandingUp = false;
        StartCoroutine(StartNpcDialogue());
        //anim.Play("Player_StandUp");           // 播放起身动画

        //float standUpTimer = 0;
        //while (standUpTimer < standUpDuration)
        //{
        //    // 文本在“显示-隐藏”间切换，模拟画面闪烁
        //    tutorialTMP.color = tutorialTMP.color.a == 0
        //        ? originalTextColor
        //        : new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0);
        //    yield return new WaitForSeconds(textFlickerInterval);
        //    standUpTimer += textFlickerInterval;
        //}

        // 起身完成：重置文本，显示操作教学（策划“教玩家AD移动，空格跳跃”）
       
        //tutorialTMP.color = originalTextColor;
        //tutorialTMP.text = "按【A/D】移动 | 按【空格】跳跃";
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
        //tutorialTMP.text = $"NPC：{npcDialogues[0]}";  // 显示第一句对话
        //FadeInText($"NPC：{npcDialogues[0]}");

        // 生成NPC并播放入场动画
        spawnedNPC = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        Animator npcAnim = spawnedNPC.GetComponent<Animator>();
        //if (npcAnim != null)
        //{
        //    npcAnim.Play("NPC_WalkIn");  // NPC入场动画
        //}

        // 等待入场动画完成（避免对话与动画不同步）
        yield return new WaitForSeconds(1.5f);

        rb.bodyType = RigidbodyType2D.Static;
        // 逐句播放NPC对话
        for (int i = 0; i < npcDialogues.Length; i++)
        {
            //tutorialTMP.text = "";

            //tutorialTMP.text = $"NPC：{npcDialogues[i]}";
            FadeInText($"{npcDialogues[i]}");
            //yield return new WaitForSeconds(dialogueInterval);
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
            FadeOutText();
            yield return new WaitForSeconds(2f);
            //tutorialTMP.DOText($"NPC：{npcDialogues[i]}", 1.5f) // 1.5秒完成打字
            //.SetEase(Ease.Linear);
        }

        // NPC退场（策划“NPC退场”）
        //if (npcAnim != null)
        //{
        //    npcAnim.Play("NPC_WalkOut");  // NPC退场动画
        //}
        //tutorialTMP.text = "NPC：继续前进吧！";
        npcAnim.SetTrigger("fade");
        yield return new WaitForSeconds(1.5f);
        FadeInText("主角：什么意思…完全没懂");
        yield return new WaitForSeconds(3f);
        FadeOutText();
        yield return new WaitForSeconds(2f);
        FadeInText("主角：这里喘不上气…还是…继续往前走吧。");
        yield return new WaitForSeconds(3f);
        FadeOutText();
        rb.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(2f);
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
        //Vector3 startCamPos = vCam.transform.position;
        //Vector3 targetCamPos = vCamLockPos;
        //float elapsedTime = 0f;

        //while (elapsedTime < camMoveDuration)
        //{
        //    // 计算移动进度（0→1，Lerp插值用）
        //    float t = elapsedTime / camMoveDuration;
        //    // 平滑因子（可选：添加缓动效果，让移动先慢后快再慢，更自然）
        //    t = Mathf.SmoothStep(0f, 1f, t);

        //    // 逐帧更新镜头位置
        //    vCam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t);
        //    elapsedTime += Time.deltaTime;

        //    yield return null; // 等待下一帧，确保平滑
        //}

        //vCam.transform.position = targetCamPos;
        vCam.transform.DOMove(vCamLockPos, camMoveDuration)
            .SetEase(Ease.InOutCubic);

        // 等待移动完成
        yield return new WaitForSeconds(camMoveDuration);

        // 等待一帧
        yield return null;

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
