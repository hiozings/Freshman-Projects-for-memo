using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using static Unity.VisualScripting.Member;

public class PlayerControl : MonoBehaviour
{
    [Header("事件广播")]
    public FadeEventSO fadeEventSO;
    public PlayAudioEventSO playAudioEvent;

    [Header("事件监听")]
    public SheetJudgeEventSO sheetJudgeEvent;

    [Header("组件引用")]
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private PlayerAnimation playerAnimation;
    private CapsuleCollider2D capsuleCollider;
    private Attack attack;
    private Character character;
    private AudioSource characterAudioSource;

    [Header("激光攻击设置")]
    public GameObject laserPrefab; // 激光预制体
    public float laserSpeed = 10f; // 激光飞行速度
    public float laserLifetime = 2f; // 激光存在时间

    public AudioClip ShootFX;

    [Header("基本参数")]
    public float speed;
    public float jumpForce;
    public bool isHurt;
    public bool isDead;
    public float hurtForce;
    public float attackRadius;
    private float originScale;
    public Vector2 offset;
    public Vector2 inputDirection;
    public LayerMask enemyLayer;
    private void Awake()
    {
        inputControl = new PlayerInputControl();
        
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerAnimation = GetComponent<PlayerAnimation>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        attack = GetComponent<Attack>();
        character = GetComponent<Character>();
        characterAudioSource = GetComponent<AudioSource>();

        originScale = transform.localScale.x;

        inputControl.Gameplay.Jump.started += Jump;
        //inputControl.Gameplay.JKey.started += PlayerAttack;
    }

    
    private void OnEnable()
    {
        inputControl.Enable();
        sheetJudgeEvent.OnEventRaised += OnSheetJudge;
    }

    private void OnDisable()
    {
        inputControl.Disable();
        sheetJudgeEvent.OnEventRaised -= OnSheetJudge;
    }

    

    private void Update()
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isHurt) return;
        Move();
    }

    private void Move()
    {
        rb.velocity = new Vector2(inputDirection.x * speed * Time.fixedDeltaTime, rb.velocity.y);

        //int faceDir = (int)transform.localScale.x;
        float faceDir = transform.localScale.x > 0 ? originScale : -originScale;

        if (inputDirection.x > 0)
        {
            faceDir = originScale;
        }
        if(inputDirection.x < 0)
        {
            faceDir = -originScale;
        }
        transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(physicsCheck.isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse); 
        }
    }

    private void PlayerAttack(int damage)
    {
       
            playerAnimation.PlayAttack();
            //Debug.Log("attack!!!");
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
            attack.damage = damage;
            
            foreach (var hitCollider in hitColliders)
            {
            //hitCollider.GetComponent<Character>()?.TakeDamage(attack);
            StartCoroutine(SpawnLaserToEnemy(hitCollider.transform, damage));


        }
        

    }

    private IEnumerator SpawnLaserToEnemy(Transform enemyTarget, int damage)
    {
        if (laserPrefab == null || enemyTarget == null) yield break;

        // 在玩家位置生成激光
        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);

        // 计算朝向敌人的方向
        Vector2 direction = (enemyTarget.position - transform.position).normalized;
        direction.y += 0.1f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 设置激光旋转方向
        laser.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 获取激光的刚体组件
        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        LaserProjectile laserProjectile = laser.GetComponent<LaserProjectile>();

        // 设置激光伤害
        if (laserProjectile != null)
        {
            laserProjectile.damage = damage;
            laserProjectile.target = enemyTarget;
        }

        // 发射激光
        if (laserRb != null)
        {
            laserRb.velocity = direction * laserSpeed;
        }
        //playAudioEvent.RaiseEvent(ShootFX);
        //characterAudioSource.Play();
        PlayAttackFX();
        //else
        //{
        //    // 如果没有刚体，使用Transform移动
        //    StartCoroutine(MoveLaserTransform(laser.transform, enemyTarget, direction));
        //}

        // 自动销毁激光
        Destroy(laser, laserLifetime);
    }

    private void PlayAttackFX()
    {
        characterAudioSource.Play();
        Invoke(nameof(StopFX), 1.6f);
    }

    private void StopFX()
    {
        if (characterAudioSource.isPlaying)
        {
            characterAudioSource.Stop();
        }
    }

    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(transform.position.x - attacker.position.x, 0).normalized;
        //Debug.Log(hurtForce);
        //Debug.Log(dir);
        //rb.AddForce(dir * (hurtForce * 10.0f), ForceMode2D.Impulse);
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        //Debug.Log(dir * hurtForce);
        //Debug.Log("Hurt");
    }

    public void FadeAfterHurt()
    {
        fadeEventSO.RaiseEvent(0f, 1f);
    }

    public void NumbAfterHurt(float numbDuration)
    {
        StartCoroutine(NumbCoroutine(numbDuration));
    }

    private IEnumerator NumbCoroutine(float numbDuration)
    {
        inputControl.Disable();
        yield return new WaitForSeconds(numbDuration);
        inputControl.Enable();
    }

    public void PlayerDead()
    {
        if (isDead) return;
        isDead = true;
        inputControl.Gameplay.Disable();
        rb.AddForce(Vector2.up * hurtForce, ForceMode2D.Impulse);
        //Debug.Log(Vector2.up * hurtForce);
        capsuleCollider.enabled = false;
        //LimitedInBounds limitedInBounds = GetComponent<LimitedInBounds>();
        //limitedInBounds.isLimited = false;
    }

    private void OnSheetJudge(char comm, string precision)
    {

        if (character == null)
        {
            Debug.LogError("Character reference is null in PlayerControl!");
            return;
        }
        if (comm == 'J')
        {
            switch (precision)
            {
                case "just":
                    PlayerAttack(6); break;

                case "good":
                    PlayerAttack(4); break;

                case "normal":
                    PlayerAttack(2); break;
                default:
                    break;
            }

        }
        else if(comm == 'K' && character.permanentShield == 0)
        {
            switch (precision)
            {
                
                case "just":
                    character.AddShield(3); break;
                case "good":
                    character.AddShield(2); break;
                case "normal":
                    character.AddShield(1); break;
                default:
                    break;
                
            }

        }
        else if(comm == 'L')
        {
            switch(precision)
            {
                case "just":
                    character.AddPower(3); break;
                case "good":
                    character.AddPower(2); break;
                case "normal":
                    character.AddPower(1); break;
                default:
                    break;
            }
        }

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + offset, attackRadius);
    }
}
