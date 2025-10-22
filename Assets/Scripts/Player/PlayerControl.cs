using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class PlayerControl : MonoBehaviour
{
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

    [Header("基本参数")]
    public float speed;
    public float jumpForce;
    public bool isHurt;
    public bool isDead;
    public float hurtForce;
    public float attackRadius;
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

        int faceDir = (int)transform.localScale.x;

        if(inputDirection.x > 0)
        {
            faceDir = 1;
        }
        if(inputDirection.x < 0)
        {
            faceDir = -1;
        }
        transform.localScale = new Vector3(faceDir, 1, 1);
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
            Debug.Log("attack!!!");
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
            attack.damage = damage;
            
            foreach (var hitCollider in hitColliders)
            {
                hitCollider.GetComponent<Character>()?.TakeDamage(attack);
                
                //Character enemyCharacter = hitCollider.GetComponent<Character>();

                //if (enemyCharacter != null)
                //{
                //    // 找到Enemy的Character组件，可在此处添加攻击逻辑（如造成伤害）
                //    Debug.Log($"攻击到敌人: {hitCollider.gameObject.name}");

                //    // 示例：调用敌人的受伤方法（如果需要）
                //    // enemyCharacter.TakeDamage(...)
                //}
            }
        

    }

    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(transform.position.x - attacker.position.x, 0).normalized;
        Debug.Log(hurtForce);
        Debug.Log(dir);
        //rb.AddForce(dir * (hurtForce * 10.0f), ForceMode2D.Impulse);
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        Debug.Log(dir * hurtForce);
        Debug.Log("Hurt");
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
        if(comm == 'J')
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
                    character.AddPower(2); break;
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
