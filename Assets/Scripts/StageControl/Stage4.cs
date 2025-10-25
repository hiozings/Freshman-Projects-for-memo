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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform == playerTrans && !isStage4Started)
        {
            isStage4Started = true;
            StartCoroutine(Stage4Init());
        }
    }

    private IEnumerator Stage4Init()
    {
        spawnedBoss = Instantiate(bossPrefab, bossSpawnPos.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
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
        yield return new WaitForSeconds(1f);
        spawnedNpc = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        Debug.Log("NPC SPAWN");
        yield return new WaitForSeconds(1f);

        TMP.text = "";
        TMP.gameObject.SetActive(true);
        for (int i = 0; i < npcDialogue.Length; i++)
        {
            TMP.text = $"NPC：{npcDialogue[i]}";
            yield return new WaitForSeconds(dialogueInterval);
        }

        TMP.gameObject.SetActive(false);

        spawnedNpc.GetComponent<Animator>()?.SetTrigger("fade");
        yield return new WaitForSeconds(2f);

        OnGameEnd?.Invoke();
        yield return new WaitForSeconds(4f);

        Application.Quit();

        Destroy(this.gameObject);
    }
}
