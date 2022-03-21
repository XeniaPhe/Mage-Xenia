using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : MonoBehaviour
{
    //config params

    [Header("Movement", order = 0)]
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float walkTime = 1.5f;

    [Header("Jump", order = 1)]
    [SerializeField] float firstJumpXVelocity = 10f;
    [SerializeField] float secondJumpXVelocity = 5f;
    [SerializeField] float firstJumpYVelocity = 9.81f;
    [SerializeField] float secondJumpYVelocity = 4.405f;

    [Header("Shooting", order = 2)]
    [SerializeField] FireBall fireBall;
    [SerializeField] float shootXOffset = 1f;
    [SerializeField] float shootYOffset = 0.4f;
    [SerializeField] float shootCoolDown = 1f;
    [SerializeField] float secondaryShootCoolDown = 3f;


    //cached variables

    Rigidbody2D body;
    Animator animator;

    Vector2 jumpSpeed;
    Vector2 secondJumpSpeed;

    bool isWalking;
    bool isRunning;
    bool isOnJump;
    bool isOnSecondJump;
    bool canMove;
    bool grounded = false;
    float timeElapsedAfterWalk = 0;
    float timeElapsedAfterShoot = 0;
    int latestDirection = 1;
    float latestYVelocity = 0;
    float latestYPosition = 0;
    

    string animWalk = "isWalking";
    string animRun = "isRunning";
    string animJump = "firstJump";
    string animSecondJump = "secondJump";
    string animGrounded = "isGrounded";


    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        jumpSpeed = new Vector2(firstJumpXVelocity, firstJumpYVelocity);
        secondJumpSpeed = new Vector2(secondJumpXVelocity, secondJumpYVelocity);
        canMove = true;
    }

    private void Update()
    {
        SynchronizeParams();
        float horizontal = Input.GetAxis("Horizontal");
        timeElapsedAfterShoot += Time.deltaTime;

        FixDirection();
        if (isRunning)
            Run(horizontal);
        else
            Walk(horizontal);

        grounded = CheckIfGrounded();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnJump)
                SecondJump(grounded);
            else
                Jump(grounded);
        }

        PrimaryAttack();
    }

    private void SynchronizeParams()
    {
        isWalking = animator.GetBool(animWalk);
        isRunning = animator.GetBool(animRun);
        isOnJump = animator.GetBool(animJump);
        isOnSecondJump = animator.GetBool(animSecondJump);
        grounded = animator.GetBool(animGrounded);
    }

    void Walk(float horizontalActivity)
    {
        if (Mathf.Abs(horizontalActivity) > Mathf.Epsilon && !isRunning && canMove)
        {
            int direction = (int)Mathf.Sign(horizontalActivity);
            timeElapsedAfterWalk += (direction == latestDirection) ? Time.deltaTime : -timeElapsedAfterWalk;
            latestDirection = direction;

            isWalking = true;
            animator.SetBool(animWalk, true);

            Vector2 walk = new Vector2(walkSpeed * direction, body.velocity.y);

            if (isOnJump)
                body.velocity = new Vector2(direction * jumpSpeed.x, body.velocity.y);
            else if (isOnSecondJump)
                body.velocity = new Vector2(direction * secondJumpSpeed.x, body.velocity.y);
            else
                body.velocity = walk;

            if (timeElapsedAfterWalk >= walkTime)
            {
                timeElapsedAfterWalk = 0;
                isWalking = false;
                animator.SetBool(animWalk, false);
                Run(horizontalActivity);
            }

        }
        else
        {
            Reset();
        }

    }

    void Run(float horizontalActivity)
    {
        if (Mathf.Abs(horizontalActivity) > Mathf.Epsilon && canMove)
        {
            int direction = (int)Mathf.Sign(horizontalActivity);

            if (direction == latestDirection)
            {
                isRunning = true;
                animator.SetBool(animRun, true);
            }
            else
            {
                isRunning = false;
                animator.SetBool(animRun, false);
                direction = latestDirection;
                return;
            }

            Vector2 run = new Vector2(runSpeed * direction, body.velocity.y);

            if (isOnJump)
                body.velocity = new Vector2(direction * jumpSpeed.x, body.velocity.y);
            else if (isOnSecondJump)
                body.velocity = new Vector2(direction * secondJumpSpeed.x, body.velocity.y);
            else if (isRunning)
                body.velocity = run;
        }
        else
        {
            Reset();
        }
    }

    private void Reset()
    {
        isRunning = false;
        isWalking = false;
        animator.SetBool(animWalk, false);
        animator.SetBool(animRun, false);
        timeElapsedAfterWalk = 0;
    }

    void FixDirection()
    {
        if (Mathf.Abs(body.velocity.x) < Mathf.Epsilon)
            return;

        int direction = (int)Mathf.Sign(body.velocity.x);
        transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
    }

    bool CheckIfGrounded()
    {
        bool isTouching = body.IsTouchingLayers(1 << 10);
        
        float yVelocity = body.velocity.y;
        bool hasYVelocity = Mathf.Abs(yVelocity) > Mathf.Epsilon;
        bool returnValue = false;


         
        if (!isTouching)
        {
            //This means the character is simply on a jump
            setFalse();
        }
        else if (latestYVelocity > 0 && yVelocity <EstimateNewVelocity() &&body.position.y<EstimateNewPosition() && grounded == false)
        {
            //This situation means the character just bumped his head into a wall above him so that he's on a jump
            setFalse();
        }
        else if (hasYVelocity == false && (latestYVelocity == 0 || latestYVelocity < 0))
        {
            //This situation simply means the character has no velocity in y axis thus is on ground 
            setTrue();
        }
        else if (Mathf.Abs(yVelocity) > 0  && Mathf.Abs(body.velocity.x) > 0)
        {
            //This situation means the character has a little y velocity due to inclination but is on ground
            setTrue();
        }
        

        void setFalse()
        {
            returnValue = false;
        }
        void setTrue()
        {
            returnValue = true;
            animator.SetBool(animJump, false);
            animator.SetBool(animSecondJump, false);
            isOnJump = false;
            isOnSecondJump = false;
        }
        float EstimateNewVelocity()
        {
            float velocity = latestYVelocity - (Physics.gravity.y * Time.deltaTime);

            return velocity;
        }
        float EstimateNewPosition()
        {
            float timeElapsed = Time.deltaTime;
            float newPos = latestYVelocity * timeElapsed - 0.5f * Physics2D.gravity.y * timeElapsed * timeElapsed;

            return newPos;
        }
        
        latestYVelocity = yVelocity;
        latestYPosition = body.position.y;
        animator.SetBool(animGrounded, returnValue);


        return returnValue;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        collision.GetContacts(contacts);
    }

    void Jump(bool isgrounded)
    {
        if (isgrounded && canMove)
        {
            isOnJump = true;
            animator.SetBool(animJump, true);
            grounded = false;
            animator.SetBool(animGrounded, false);

            float isThereMovement = (Mathf.Abs(body.velocity.x) > Mathf.Epsilon) ? 1 : 0;
            float movementDirection = (isThereMovement == 1) ? Mathf.Sign(body.velocity.x) : latestDirection;

            Vector2 jumpVelocity = new Vector2(firstJumpXVelocity * isThereMovement * movementDirection, firstJumpYVelocity);

            body.velocity = jumpVelocity;
        }
    }

    void SecondJump(bool isgrounded)
    {
        if (isOnJump && isgrounded == false && canMove)
        {
            isOnJump = false;
            isOnSecondJump = true;
            animator.SetBool(animJump, false);
            animator.SetBool(animSecondJump, true);

            float isThereMovement = (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon) ? 1 : 0;
            float movementDirection = (isThereMovement == 1) ? Mathf.Sign(body.velocity.x) : latestDirection;

            Vector2 jumpVelocity = new Vector2(isThereMovement * movementDirection * secondJumpXVelocity, secondJumpYVelocity);

            body.velocity = jumpVelocity;
        }

    }

    void PrimaryAttack()
    {
        if (Input.GetMouseButtonDown(0) && timeElapsedAfterShoot >= shootCoolDown && canMove)
        {
            timeElapsedAfterShoot = 0;
            animator.SetTrigger("Primary Attack");
        }
    }

    void Fire()
    {
        Vector3 pos = new Vector3(transform.position.x + shootXOffset, transform.position.y - shootYOffset, transform.position.z);
        fireBall = Instantiate(fireBall, pos, Quaternion.identity) as FireBall;

        fireBall.direction = (body.velocity.x == 0) ? latestDirection : (int)Mathf.Sign(body.velocity.x);
        fireBall.relativeSpeed = body.velocity.x;

        fireBall.Shoot();
    }

    void SetCanMove()
    {
        canMove = !canMove;
        if (!canMove)
            body.velocity = new Vector2(0, 0);
    }
}