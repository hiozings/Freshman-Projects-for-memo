using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BackManager : MonoBehaviour
{
    public Transform far;
    public Transform middle;
    public Transform front;
    public Transform player;

    Rigidbody2D farRb;
    Rigidbody2D middleRb;
    Rigidbody2D frontRb;
    Rigidbody2D playerRb;

    private void Awake()
    {
        farRb = far.GetComponent<Rigidbody2D>();
        middleRb = middle.GetComponent<Rigidbody2D>();
        frontRb = front.GetComponent<Rigidbody2D>();
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        farRb.velocity = new Vector2(-playerRb.velocity.x / 15f, 0);
        //middleRb.velocity = new Vector2(playerRb.velocity.x / 1.15f, 0);
        //frontRb.velocity = new Vector2(playerRb.velocity.x / 1.15f, 0);
    }

}
