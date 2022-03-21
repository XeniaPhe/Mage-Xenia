using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float health = 500;
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void TakeDamage(float damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");
        if(health<=0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    
}
