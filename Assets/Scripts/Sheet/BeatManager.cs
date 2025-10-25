using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;

    [Header("节拍设置")]
    public int BPM = 120;
    public int beatsPerPhase = 4; // 每个阶段4拍

    [Header("当前状态")]
    public int currentBeat = 0;
    public int currentPhaseBeat = 0; // 当前阶段内的节拍数
    public GamePhase currentPhase = GamePhase.RestPhase;
    public bool isOnBeat;
    public float beatInterval { get; private set; }

    private float beatTimer = 0f;

    public System.Action<int> OnBeat;
    public System.Action OnPlayerPhaseStart;
    public System.Action OnEnemyPhaseStart;
    public System.Action OnRestPhaseStart;
    public System.Action OnSheetOpen;

    public enum GamePhase
    {
        PlayerPhase,    // 玩家阶段：前4拍
        EnemyPhase,     // 敌人阶段：中间4拍  
        RestPhase       // 休息阶段：最后4拍
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 计算节拍间隔：BPM 120 = 120拍/分钟 = 2拍/秒 = 0.5秒/拍
        beatInterval = 60f / BPM;
        
    }

    //private void Start()
    //{
    //    // 游戏开始时进入玩家阶段
    //    OnPlayerPhaseStart?.Invoke();
    //}

    //private void OnEnable()
    //{
    //    beatTimer = 0f;
    //    currentBeat = 0;
    //    currentPhaseBeat = 0;
    //    currentPhase = GamePhase.PlayerPhase;
    //    OnPlayerPhaseStart?.Invoke();
        
    //}

    private void Update()
    {
        if(isOnBeat)
        {
            beatTimer += Time.deltaTime;

            if (beatTimer >= beatInterval)
            {
                beatTimer = 0f;
                currentBeat++;
                currentPhaseBeat++;
                Debug.Log($"currentBeat: {currentBeat}, currentPhaseBeat: {currentPhaseBeat}");

                // 每4拍切换阶段
                if (currentPhaseBeat >= beatsPerPhase)
                {
                    currentPhaseBeat = 0;
                    SwitchToNextPhase();
                }

                OnBeat?.Invoke(currentBeat);
            }
        }
    }

    private void SwitchToNextPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.PlayerPhase:
                currentPhase = GamePhase.EnemyPhase;
                OnEnemyPhaseStart?.Invoke();
                break;
            case GamePhase.EnemyPhase:
                currentPhase = GamePhase.RestPhase;
                OnRestPhaseStart?.Invoke();
                break;
            case GamePhase.RestPhase:
                currentPhase = GamePhase.PlayerPhase;
                OnPlayerPhaseStart?.Invoke();
                break;
        }

        Debug.Log($"切换到阶段: {currentPhase}, 总节拍: {currentBeat}");
    }

    // 玩家行动时刻：前4拍中只有前3拍是谱线互动
    public bool IsPlayerActionBeat()
    {
        return currentPhase == GamePhase.PlayerPhase && currentPhaseBeat < 3;
    }

    // 敌人行动时刻：中间4拍按照特定顺序行动
    public bool IsEnemyActionBeat(int enemyActionIndex)
    {
        return currentPhase == GamePhase.EnemyPhase && currentPhaseBeat == enemyActionIndex;
    }

    public void SetOnBeat(bool state)
    {
        isOnBeat = state;
        if(state)
        {
            beatTimer = 0f;
            currentBeat = 0;
            currentPhaseBeat = 0;
            currentPhase = GamePhase.RestPhase;
            //OnPlayerPhaseStart?.Invoke();
            OnRestPhaseStart?.Invoke();
            if (OnSheetOpen == null) Debug.Log("OnSheetOpen null");
            OnSheetOpen?.Invoke();
        }
    }
}
