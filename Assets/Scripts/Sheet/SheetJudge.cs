using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Diagnostics;
using DG.Tweening;
public class SheetJudge : MonoBehaviour
{
    public bool isInputEnabled =true;

    public SheetJudgeEventSO sheetJudgeEventSO;
    public PlayAudioEventSO playAudioEvent;
    private AudioSource audioSource;

    public Character character;

    // 扫描线相关
    [Header("扫描线设置")]
    public Image scanLine; // 拖拽UIImage作为扫描线
    public float scanSpeed; // 扫描线移动速度（像素/秒）
    public float leftStartX; // 扫描线起始X（谱面左侧外）
    public float rightEndX; // 扫描线结束X（谱面右侧外）
    public float leftFarStartX;
    public float rightFarEndX;
    private float currentScanX; // 扫描线当前X坐标
    private bool isPlayerPhase = true;

    // 判定线相关
    [Header("判定线设置")]
    public GameObject judgeLinePrefab; // 判定线预制体（UIImage）
    public List<Vector2> judgeLinePositions; // 判定线在Sheet内的坐标（相对Sheet的局部坐标）
    public List<Image> spawnedJudgeLines = new List<Image>(); // 已生成的判定线列表

    // 按键与标记相关
    [Header("按键与标记设置")]
    //public KeyCode[] rhythmKeys = { KeyCode.J, KeyCode.K, KeyCode.L }; // 节奏指令键
    //private RhythmInput input;
    private PlayerInputControl input;
    private InputAction jKeyAction;
    private InputAction kKeyAction;
    private InputAction lKeyAction;
    public GameObject[] markPrefabs; // 按键标记预制体（J/K/L对应不同预制体）
    public GameObject[] evalPrefabs; // 评价标识预制体（Good/Great/Perfect等）
    private List<GameObject> spawnedMarks = new List<GameObject>(); // 已生成的标记列表
    private List<GameObject> spawnedEvals = new List<GameObject>(); // 已生成的评价列表
    private string collectedCommand = "";

    // 判定参数（时间判定，参考音游标准）
    [Header("判定标准（毫秒）")]
    public int justTime; // Perfect判定窗口
    public int goodTime; // Great判定窗口
    public int normalTime; // Good判定窗口
    private float scanLinePassTime; // 扫描线经过判定线的时间戳

    [Header("音效")]
    public AudioClip jFX;
    public AudioClip kFX;
    public AudioClip lFX;
    private void Awake()
    {
        input = new PlayerInputControl();
        
        var gameplay = input.Gameplay;
        jKeyAction = gameplay.JKey;
        kKeyAction = gameplay.KKey;
        lKeyAction = gameplay.LKey;

        audioSource = GetComponent<AudioSource>();

        float sheetWidth = Mathf.Abs(rightEndX - leftStartX);
        float timeForBeats = BeatManager.Instance.beatInterval * 4;
        scanSpeed = sheetWidth / timeForBeats;
    }

    void OnEnable()
    {
        // 启用输入监听
        input.Enable();
        BeatManager.Instance.OnPlayerPhaseStart += OnPlayerPhaseStart;
        BeatManager.Instance.OnEnemyPhaseStart += OnEnemyPhaseStart;
        BeatManager.Instance.OnRestPhaseStart += OnRestPhaseStart;
    }

    void OnDisable()
    {
        // 禁用输入监听
        input.Disable();
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.OnPlayerPhaseStart -= OnPlayerPhaseStart;
            BeatManager.Instance.OnEnemyPhaseStart -= OnEnemyPhaseStart;
            BeatManager.Instance.OnRestPhaseStart -= OnRestPhaseStart;
        }
    }

    void Start()
    {
        // 初始化扫描线位置
        currentScanX = leftFarStartX;
        UpdateScanLinePos();

        // 生成初始判定线
        //SpawnJudgeLines();
    }

    void Update()
    {
        //// 1. 扫描线移动逻辑
        //MoveScanLine();

        //// 2. 检测节奏按键输入
        //CheckRhythmInput();
        if (BeatManager.Instance.currentPhase == BeatManager.GamePhase.PlayerPhase &&
        BeatManager.Instance.currentPhaseBeat < 4)
        {
            MoveScanLine();
            UnityEngine.Debug.Log("update");
            CheckRhythmInput();
        }

        // 3. 扫描线循环（回到左侧时重置）
        if (currentScanX > rightFarEndX)
        {
            //ResetSheet();
            ResetSheetFar();
        }
    }

    private void OnPlayerPhaseStart()
    {
        isPlayerPhase = true;
        ResetSheet();
        GenerateJudgeLines();
    }

    private void OnEnemyPhaseStart()
    {
        isPlayerPhase = false;
        scanLine.rectTransform.DOAnchorPos(new Vector2(rightFarEndX - 500, scanLine.rectTransform.localPosition.y), 2f).SetEase(Ease.Linear);
    }

    private void OnRestPhaseStart()
    {
        ResetSheetFar();
        scanLine.rectTransform.DOAnchorPos(new Vector2(leftStartX - 500, scanLine.rectTransform.localPosition.y), 2f).SetEase(Ease.Linear);
    }

    private void GenerateJudgeLines()
    {
        // 清除旧判定线
        ClearJudgeLines();
        UnityEngine.Debug.Log("generate lines");

        // 生成3条判定线，对应3个玩家行动节拍
        float sheetWidth = Mathf.Abs(rightEndX - leftStartX);
        float beatDistance = sheetWidth / 4f;

        for (int i = 0; i < 3; i++)
        {
            float xPos = leftStartX + (beatDistance * (i + 0.5f)); // 在节拍中间位置
            Vector2 judgeLinePos = new Vector2(xPos, 0);

            GameObject judgeLineObj = Instantiate(judgeLinePrefab, transform);
            Image judgeLine = judgeLineObj.GetComponent<Image>();
            judgeLine.rectTransform.localPosition = judgeLinePos;
            spawnedJudgeLines.Add(judgeLine);
        }
    }


    public void EnableInput(bool isEnable)
    {
        this.isInputEnabled = isEnable; // 新增bool变量控制输入开关
    }

    // 扫描线移动
    private void MoveScanLine()
    {
        currentScanX += scanSpeed * Time.deltaTime;
        UnityEngine.Debug.Log("move");
        UpdateScanLinePos();
    }

    // 更新扫描线UI位置（基于Sheet的局部坐标）
    private void UpdateScanLinePos()
    {
        scanLine.rectTransform.localPosition = new Vector2(currentScanX, scanLine.rectTransform.localPosition.y);
    }

    // 生成判定线（根据预设坐标列表）
    private void SpawnJudgeLines()
    {
        // 先销毁旧判定线
        ClearJudgeLines();

        foreach (var pos in judgeLinePositions)
        {
            GameObject judgeLineObj = Instantiate(judgeLinePrefab, transform); // 父物体设为Sheet
            Image judgeLine = judgeLineObj.GetComponent<Image>();
            judgeLine.rectTransform.localPosition = pos; // 设置局部坐标
            spawnedJudgeLines.Add(judgeLine);
        }
    }

    // 清除所有判定线
    private void ClearJudgeLines()
    {
        foreach (var line in spawnedJudgeLines)
        {
            Destroy(line.gameObject);
        }
        spawnedJudgeLines.Clear();
    }

    // 检测节奏键输入（仅扫描线在谱面内时响应）
    private void CheckRhythmInput()
    {
        // 判断扫描线是否在谱面内（可根据实际谱面宽度调整范围）
        bool isScanInSheet = currentScanX >= 0 && currentScanX <= 1000;
        if (!isScanInSheet) return;
        //if(character.currentPower <= 0) return;

        //// 遍历检测按键
        //for (int i = 0; i < rhythmKeys.Length; i++)
        //{
        //    if (Input.GetKeyDown(rhythmKeys[i]))
        //    {
        //        // 生成对应按键的标记（位置=当前扫描线位置）
        //        SpawnMarkAndEval(i);
        //        break;
        //    }
        //}
        if (jKeyAction.WasPressedThisFrame())
        {
            if (character.currentPower <= 0) return;
            character.currentPower -= 1;
            //playAudioEvent.RaiseEvent(jFX);
            PlayTokenFX(jFX);
            SpawnMarkAndEval(0, 'J');
            collectedCommand += 'J';
        }
        else if (kKeyAction.WasPressedThisFrame())
        {
            if (character.currentPower <= 0) return;
            character.currentPower -= 1;
            //playAudioEvent.RaiseEvent(kFX);
            PlayTokenFX(kFX);
            UnityEngine.Debug.Log("k");
            SpawnMarkAndEval(1, 'K');
            collectedCommand += 'K';
        }
        else if (lKeyAction.WasPressedThisFrame())
        {
            if (character.currentPower <= 0) character.currentPower = 1;
            character.currentPower -= 1;
            //playAudioEvent.RaiseEvent(lFX);
            PlayTokenFX(lFX);
            SpawnMarkAndEval(2, 'L');
            collectedCommand += 'L';
        }

    }

    private void PlayTokenFX(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
        Invoke(nameof(StopFX), 1.6f);
    }

    private void StopFX()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public string GetPlayerCommand()
    {
        string command = collectedCommand;
        collectedCommand = ""; // 重置指令，准备下一回合
        return command;
    }


    // 生成按键标记与评价标识
    private void SpawnMarkAndEval(int keyIndex, char comm)
    {
        // 1. 生成标记
        Vector2 markPos = new Vector2(currentScanX, scanLine.rectTransform.localPosition.y);
        GameObject mark = Instantiate(markPrefabs[keyIndex], transform);
        RectTransform markReck = mark.GetComponent<RectTransform>();
        markReck.localPosition = markPos;
        spawnedMarks.Add(mark);

        // 2. 计算最近的判定线与时间差（核心判定逻辑）
        float closestTimeDiff = float.MaxValue;
        foreach (var judgeLine in spawnedJudgeLines)
        {
            // 计算扫描线到达该判定线的时间（距离/速度）
            float distanceToJudge = Mathf.Abs(judgeLine.rectTransform.localPosition.x - currentScanX);
            float timeDiff = distanceToJudge / scanSpeed * 1000; // 转换为毫秒

            if (timeDiff < closestTimeDiff)
            {
                closestTimeDiff = timeDiff;
            }
        }

        // 3. 根据时间差生成评价标识
        SpawnEvaluation(closestTimeDiff, markPos, comm);
    }

    // 根据时间差生成评价（Perfect>Great>Good>Miss）
    private void SpawnEvaluation(float timeDiff, Vector2 spawnPos, char comm)
    {
        GameObject evalPrefab = null;
        if (timeDiff <= justTime)
        {
            evalPrefab = evalPrefabs[0]; // just预制体
            sheetJudgeEventSO.RaiseEvent(comm, "just");

        }
        else if (timeDiff <= goodTime)
        {
            evalPrefab = evalPrefabs[1]; // good预制体
            sheetJudgeEventSO.RaiseEvent(comm, "good");
        }
        else if (timeDiff <= normalTime)
        {
            evalPrefab = evalPrefabs[2]; // normal预制体
            sheetJudgeEventSO.RaiseEvent(comm, "normal");
        }
        // 未达Good则视为Miss，不生成评价（可根据需求补充Miss标识）

        if (evalPrefab != null)
        {
            GameObject eval = Instantiate(evalPrefab, transform);
            RectTransform evalRect = eval.GetComponent<RectTransform>();
            evalRect.localPosition = new Vector2(spawnPos.x, spawnPos.y + 50); // 评价在标记上方
            spawnedEvals.Add(eval);
        }
    }

    // 扫描线循环时重置谱面（清除标记、评价，更新判定线）
    private void ResetSheet()
    {
        UnityEngine.Debug.Log("reset");
        // 重置扫描线位置
        currentScanX = leftStartX;
        UpdateScanLinePos();

        // 清除标记和评价
        ClearMarksAndEvals();

        // （可选）更新判定线坐标（文档需求，暂留接口，后续扩展）
        // UpdateJudgeLinePositions();
    }

    private void ResetSheetFar()
    {
        currentScanX = leftFarStartX;
        UpdateScanLinePos();
        ClearMarksAndEvals();
    }

    // 清除所有标记和评价
    private void ClearMarksAndEvals()
    {
        // 清除标记
        foreach (var mark in spawnedMarks)
        {
            Destroy(mark.gameObject);
        }
        spawnedMarks.Clear();

        // 清除评价
        foreach (var eval in spawnedEvals)
        {
            Destroy(eval.gameObject);
        }
        spawnedEvals.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawLine(new Vector2(leftStartX, this.GetComponent<Image>().rectTransform.position.y), new Vector2(rightEndX, this.GetComponent<Image>().rectTransform.position.y));
        float worldY = transform.position.y;

        // 将X坐标转换为世界坐标，保持Y坐标一致
        Vector3 worldStart = transform.TransformPoint(new Vector3(leftStartX, 0, 0));
        Vector3 worldEnd = transform.TransformPoint(new Vector3(rightEndX, 0, 0));

        // 保持Y坐标一致
        worldStart.y = worldY;
        worldEnd.y = worldY;

        Gizmos.DrawLine(worldStart, worldEnd);
    }
}
