using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Burst.Intrinsics.X86.Avx;


public class Stage4 : MonoBehaviour
{
    public Transform playerTrans;
    public Transform bossSpawnPos;
    public Transform npcSpawnPos;
    public GameObject bossPrefab;
    public GameObject npcPrefab;
    public CinemachineVirtualCamera vCam;
    //public PlayerInputControl inputActions;
    public Vector3 vCamLockPos;
    public AudioClip BGMClip;

    public TextMeshProUGUI TMP;
    public string[] npcDialogue = {
        "恭喜你打败了BOSS",
        "balabala",
        "............"
    };
    public float dialogueInterval = 2.5f;
    private bool isStage4Started;
    private bool isNPCSpawned;
    private GameObject spawnedBoss;
    private GameObject spawnedNpc;

    public PlayAudioEventSO playAudioEvent;
    public VoidEventSO bossDefeatedEvent;
    public UnityEvent OnGameEnd;

    private void OnEnable()
    {
        bossDefeatedEvent.OnEventRaised += OnBossDefeated;
    }

    private void OnDisable()
    {
        bossDefeatedEvent.OnEventRaised -= OnBossDefeated;
    }

    private void FadeInText(string str, float duration = 2f)
    {
        if (TMP == null) return;

        // 确保文本初始状态为完全透明
        Color originalColor = TMP.color;
        TMP.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        TMP.text = str;
        // 执行渐入动画
        TMP.DOFade(1f, duration)
            .SetEase(Ease.OutQuad);

    }

    // 渐出效果
    private void FadeOutText(float duration = 2f)
    {
        if (TMP == null) return;

            TMP.DOFade(0f, duration)
            .SetEase(Ease.OutQuad);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform == playerTrans && !isStage4Started)
        {
            isStage4Started = true;
            //StartCoroutine(Stage4Init());
            vCam.Follow = null;
            vCam.LookAt = null;
            StartCoroutine(SmoothMoveCameraToLockPos(vCamLockPos, true));
        }
    }

    private IEnumerator SmoothMoveCameraToLockPos(Vector3 vCamLockPos, bool init)
    {
        float moveDuration = 1.5f;
        vCam.transform.DOMove(vCamLockPos, moveDuration).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(moveDuration);
        yield return null;

        if (init)
            StartCoroutine(Stage4Init());
    }

    private IEnumerator Stage4Init()
    {
        spawnedBoss = Instantiate(bossPrefab, bossSpawnPos.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        TMP.text = "";
        TMP.gameObject.SetActive(true);
        FadeInText("主角：看来…只能战斗了", 0.5f);
        yield return new WaitForSeconds(2f);
        FadeOutText(0.5f);
        yield return new WaitForSeconds(1f);

        BeatManager.Instance.SetOnBeat(true);
        playAudioEvent.RaiseEvent(BGMClip);
        spawnedBoss.GetComponent<Vine>().canAttack = true;

    }

    private void OnBossDefeated()
    {
        if(!isNPCSpawned)
        {
            Debug.Log("Boss Defeated");
            isNPCSpawned = true;
            StartCoroutine(NPCSpeech());
        }
    }

    private IEnumerator NPCSpeech()
    {
        yield return new WaitForSeconds(2f);
        //spawnedNpc = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        //Debug.Log("NPC SPAWN");
        //yield return new WaitForSeconds(1f);

        //TMP.text = "";
        //TMP.gameObject.SetActive(true);
        //for (int i = 0; i < npcDialogue.Length; i++)
        //{
        //    TMP.text = $"NPC：{npcDialogue[i]}";
        //    yield return new WaitForSeconds(dialogueInterval);
        //}

        TMP.gameObject.SetActive(false);

        //spawnedNpc.GetComponent<Animator>()?.SetTrigger("fade");
        //yield return new WaitForSeconds(2f);

        vCam.Follow = playerTrans;
        vCam.LookAt = playerTrans;

        OnGameEnd?.Invoke();
        yield return new WaitForSeconds(4f);

        Application.Quit();

        Destroy(this.gameObject);
    }
}
