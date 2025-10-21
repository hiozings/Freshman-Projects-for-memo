using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Stage3 : MonoBehaviour
{
    [Header("核心组件")]
    //public Stage2VineBattle stage2;
    public CinemachineVirtualCamera vCam; 
    public TextMeshProUGUI TMP;
    public Transform PlayerTrans;
    public Rigidbody2D rb;
    //public Transform nextSectionTrigger;

    [Header("关键资源")]
    public GameObject npcPrefab;
    public Transform npcSpawnPos;
    //public GameObject Cards;
    public Vector3 vCamLockPos;

    [Header("流程参数")]
    public string[] npcRewardDialogue = {      // NPC奖励阶段对话
        "恭喜你通过了战斗！",
        "这是恢复之泉，可以为你提供增益效果",
        "选择一张卡牌来获得特殊能力吧"
    };
    public float dialogueInterval = 2.5f;

    [Header("状态标记")]
    private bool isStage3Start = false;           // 第二段是否开始
    private bool isNpcTeaching = false;           // NPC是否在播放战斗教学
    private GameObject spawnedNPC;

    private void Start()
    {
        TMP.gameObject.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform == PlayerTrans && !isStage3Start)
        {
            Debug.Log("Stage3");
            isStage3Start = true;
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

        if(init)
            StartCoroutine(Stage3Init());
    }

    private IEnumerator Stage3Init()
    {
        TMP.text = "发现恢复之泉！";
        TMP.gameObject.SetActive(true);
        rb.bodyType = RigidbodyType2D.Static;

        yield return new WaitForSeconds(1f);

        spawnedNPC = Instantiate(npcPrefab, npcSpawnPos.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);

        isNpcTeaching = true;
        for (int i = 0; i < npcRewardDialogue.Length; i++)
        {
            TMP.text = $"NPC：{npcRewardDialogue[i]}";
            yield return new WaitForSeconds(dialogueInterval);
        }

        isNpcTeaching = false;
        TMP.gameObject.SetActive(false);

        spawnedNPC.GetComponent<Animator>()?.SetTrigger("fade");
        //StartCoroutine(SmoothMoveCameraToLockPos(PlayerTrans.position, false));
        yield return new WaitForSeconds(1f);
        vCam.Follow = PlayerTrans;
        vCam.LookAt = PlayerTrans;
        rb.bodyType = RigidbodyType2D.Dynamic;

        yield return null;

        Destroy(this.gameObject);
    }
}
