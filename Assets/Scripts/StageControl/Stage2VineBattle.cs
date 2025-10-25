using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
public class Stage2VineBattle : MonoBehaviour
{
    public VoidEventSO voidEvent;
    public PlayAudioEventSO playAudioEvent;
    public VoidEventSO stopAudioEvent;


    [Header("核心组件引用（关联第一段与场景资源）")]
    public PlayerSpawnController stage1Controller; // 第一段控制器（用于监听第一段完成）
    public CinemachineVirtualCamera vCam;          // 全局Cinemachine摄像机
    public TextMeshProUGUI battleTMP;             // 战斗提示字幕（复用或新建TextMeshPro）
    //public Canvas cardCanvas;                     // （预留）后续卡牌选择UI，第二段暂禁用
    public Transform PlayerTrans;
    public Rigidbody2D rb;
    public Animator anim;
    public Character character;

    [Header("第二段关键资源（按策划需求配置）")]
    public GameObject vineEnemyPrefab;            // 黑色藤蔓敌人预制体（固定阻拦单位）
    public GameObject npcPrefab;                  // NPC预制体（复用第一段或新制）
    public Transform vineSpawnPos;                // 藤蔓生成位置（主角前进路径上）
    public Transform npcSpawnPos;                 // NPC第二次入场位置（主角右侧3米）
    public GameObject battleMarkPrefab;           // 战斗标记预制体（屏幕中间显示）
    public AudioClip battleBGM;

    [Header("流程参数（匹配策划节奏）")]
    public string[] npcBattleTips = {             // NPC战斗教学台词（攻击/回蓝/防御）
        "前方有藤蔓阻拦！按J键攻击，K键回蓝",
        "敌人攻击时按L键防御，别忘记节奏哦～",
        "准备好了吗？战斗要开始了！"
    };
    public float tipInterval = 3f;                // 教学台词间隔（秒）
    public float battleMarkShowTime = 1.5f;       // 战斗标记显示时长（秒）
    public float vineSpawnDelay = 2f;             // 摄像机到位后延迟生成藤蔓（秒）

    [Header("状态标记")]
    private bool isStage2Start = false;           // 第二段是否开始
    private bool isNpcTeaching = false;           // NPC是否在播放战斗教学
    private GameObject spawnedVine;               // 生成的藤蔓敌人实例
    private GameObject spawnedNPC;                // 生成的NPC实例
    private GameObject spawnedBattleMark;         // 生成的战斗标记实例

    void Start()
    {
        // 初始化：禁用战斗UI，监听第一段完成事件
        battleTMP.gameObject.SetActive(false);
        //cardCanvas.gameObject.SetActive(false); // 第三段卡牌UI暂禁用
        stage1Controller.isSectionDone = false; // 确保第一段初始状态正确
    }
    void Update()
    {
        // 第一段摄像机移动到位且未开始第二段时，触发第二段流程
        if (!isStage2Start && stage1Controller.isSectionDone && !stage1Controller.isCamMoving)
        {
            StartCoroutine(Stage2InitiateCoroutine());
            isStage2Start = true;
        }
    }

    void OnEnable()
    {
        voidEvent.OnEventRaised += OnVineDefeated;
    }

    void OnDisable()
    {
        voidEvent.OnEventRaised -= OnVineDefeated;
    }

    public void FadeInText(string str, float duration = 2f)
    {
        if (battleTMP == null) return;

        // 确保文本初始状态为完全透明
        Color originalColor = battleTMP.color;
        battleTMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        battleTMP.text = str;
        // 执行渐入动画
        battleTMP.DOFade(1f, duration)
            .SetEase(Ease.OutQuad);

    }

    // 渐出效果
    public void FadeOutText(float duration = 2f)
    {
        if (battleTMP == null) return;

        battleTMP.DOFade(0f, duration)
            .SetEase(Ease.OutQuad);

    }
    IEnumerator Stage2InitiateCoroutine()
    {
        battleTMP.text = "";
        battleTMP.color = new Color(battleTMP.color.r, battleTMP.color.g, battleTMP.color.b, 1f);
        // 1. 摄像机锁定后，延迟生成藤蔓（给玩家缓冲时间，匹配策划“主角入场被阻拦”）
        battleTMP.gameObject.SetActive(true);
        //battleTMP.text = "前方发现异常...";
        rb.bodyType = RigidbodyType2D.Static;
        //yield return new WaitForSeconds(vineSpawnDelay);

        // 2. 生成黑色藤蔓敌人（固定阻拦单位，有接触伤害）
        spawnedVine = Instantiate(vineEnemyPrefab, vineSpawnPos.position, Quaternion.identity);
        //battleTMP.text = "黑色藤蔓挡住了去路！";
        FadeInText("主角：这是…什么…", 0.5f);
        yield return new WaitForSeconds(1.5f);
        FadeOutText(0.5f);




        

        // 3. NPC第二次入场（教学战斗指令，策划“NPC再次入场教攻击/回蓝/防御”）
        spawnedNPC = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        //Animator npcAnim = spawnedNPC.GetComponent<Animator>();
        //if (npcAnim != null) npcAnim.Play("NPC_WalkIn"); // 复用入场动画
        yield return new WaitForSeconds(1f); // 等待NPC入场动画完成

        // 4. NPC播放战斗教学台词
        isNpcTeaching = true;
        for (int i = 0; i < npcBattleTips.Length; i++)
        {
            //battleTMP.text = $"NPC：{npcBattleTips[i]}";
            FadeInText($"NPC：{npcBattleTips[i]}", 0.5f);
            yield return new WaitForSeconds(tipInterval);
            FadeOutText(0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        FadeInText("按下J键", 0.5f);
        while (!Input.GetKeyDown(KeyCode.J))
        {
            yield return null;
        }
        anim.SetTrigger("attack");
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("主角：这…", 0.5f);
        yield return new WaitForSeconds(3f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("NPC：对的！就是这样", 0.5f);
        yield return new WaitForSeconds(3f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("NPC：这个世界里不同的音乐片段都会产生不同的东西呢…", 0.5f);
        yield return new WaitForSeconds(3f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("按下K键试试…", 0.5f);
        while (!Input.GetKeyDown(KeyCode.K))
        {
            yield return null;
        }
        character.FormShield();
        yield return new WaitForSeconds(2f);
        Destroy(character.shieldObj);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("再按下L键试试...", 0.5f);
        while (!Input.GetKeyDown(KeyCode.L))
        {
            yield return null;
        }
        character.FormMana();
        yield return new WaitForSeconds(1f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("NPC：好强力的共鸣…你在另一边肯定是一名很厉害的乐师…", 0.5f);
        yield return new WaitForSeconds(3f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("NPC：就说这么多吧…哦，记住，你的演奏要跟上这个世界的“呼吸”", 0.5f);
        yield return new WaitForSeconds(3f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("主角：什么？", 0.5f);
        yield return new WaitForSeconds(2f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);
        FadeInText("NPC：跟上“呼吸”！");
        yield return new WaitForSeconds(2f);
        spawnedNPC.GetComponent<Animator>()?.SetTrigger("fade");
        FadeOutText(0.5f);
        yield return new WaitForSeconds(0.5f);



        isNpcTeaching = false;
        battleTMP.gameObject.SetActive(false);

        BeatManager.Instance.SetOnBeat(true);
        playAudioEvent.RaiseEvent(battleBGM);

        // 5. NPC退场，准备显示战斗标记
        //if (npcAnim != null) npcAnim.Play("NPC_WalkOut");
        //battleTMP.text = "NPC：加油！我先退下了～";
        //yield return new WaitForSeconds(2f);
        //Destroy(spawnedNPC, 1f); // 等待退场动画后销毁NPC
        //battleTMP.gameObject.SetActive(false);
        //isNpcTeaching = false;

        // 6. 屏幕中间显示战斗标记（策划“屏幕中间展示战斗标记，节拍开始”）
        //spawnedBattleMark = Instantiate(battleMarkPrefab, vCam.transform.position, Quaternion.identity);
        //spawnedBattleMark.transform.SetParent(vCam.transform); // 跟随摄像机，确保在屏幕中间
        //spawnedBattleMark.transform.localPosition = new Vector3(0, 0, 5); // 屏幕中心深度
        //yield return new WaitForSeconds(battleMarkShowTime);

        // 7. 战斗标记消失，正式开启战斗（触发藤蔓敌人AI，关联回合制战斗系统）
        //Destroy(spawnedBattleMark);
        //battleTMP.gameObject.SetActive(true);
        //battleTMP.text = "战斗开始！按节奏操作～";
        //StartCoroutine(SmoothMoveCameraToLockPos());
        //vCam.Follow = PlayerTrans;
        //vCam.LookAt = PlayerTrans;

        rb.bodyType = RigidbodyType2D.Dynamic;
        EnableVineBattleAI(); // 启用藤蔓攻击逻辑
        //Debug.Log("第二段战斗开启，符合策划“承·转”阶段需求");
    }

    private IEnumerator SmoothMoveCameraToLockPos()
    {
        // 解除跟随
        vCam.Follow = null;
        vCam.LookAt = null;

        // 使用 DoTween 平滑移动
        float moveDuration = 1.5f;
        vCam.transform.DOMove(PlayerTrans.position, moveDuration)
            .SetEase(Ease.InOutCubic);

        // 等待移动完成
        yield return new WaitForSeconds(moveDuration);

        //// 等待一帧
        //yield return null;

        // 重新设置跟随
        vCam.Follow = PlayerTrans;
        vCam.LookAt = PlayerTrans;
    }

    private void EnableVineBattleAI()
    {
        
    }

    public void  OnVineDefeated()
    {
        battleTMP.gameObject.SetActive(true);
        FadeInText("主角：这是什么…好强力的…反应…", 0.5f);
        rb.bodyType = RigidbodyType2D.Static;
        //battleTMP.text = "藤蔓缩回地下了！继续前进吧～";
        //spawnedNPC.GetComponent<Animator>()?.SetTrigger("fade");
        StartCoroutine(ShowNextStageTip(3f));
    }

    IEnumerator ShowNextStageTip(float showTime)
    {
        yield return new WaitForSeconds(showTime);
        vCam.Follow = PlayerTrans;
        vCam.LookAt = PlayerTrans;
        rb.bodyType = RigidbodyType2D.Dynamic;

        battleTMP.gameObject.SetActive(false);
        stage1Controller.enabled = false;

        BeatManager.Instance.SetOnBeat(false);
        stopAudioEvent.RaiseEvent();
        // （后续扩展）触发第三段流程
        // Stage3Reward.Instance.StartStage3();
        Destroy(this.gameObject);
    }
}
