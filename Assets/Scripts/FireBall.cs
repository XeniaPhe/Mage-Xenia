using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    //config variables
    public float relativeSpeed=0;
    public int direction;
    [SerializeField] float fireBallSpeed = 7f;
    [SerializeField] float damage = 35f;

    //cached variables
    Rigidbody2D rigidBody;
    Animator animator;
    void Awake()
    {
        direction = 1;
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    public void Shoot()
    {      
        rigidBody.velocity = new Vector2((fireBallSpeed + Mathf.Abs(relativeSpeed))*direction, 0);               
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        rigidBody.velocity = new Vector2(0, 0);
        animator.SetTrigger("Crash");
    }

    void Explode()
    {
        Destroy(gameObject);
    }
}