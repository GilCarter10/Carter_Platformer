using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum FacingDirection
    {
        left, right
    }

    public Rigidbody2D rb;
    private float acceleration;

    //RaycastHit2D[] hitTarget;

    public float maxSpeed;
    public float timeToReachMaxSpeed;

    private float gravity;
    private float initialJumpVel;

    public float apexHeight;
    public float apexTime;

    public float terminalFallSpeed;

    public LayerMask layer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        initialJumpVel = 2 * apexHeight / apexTime;
        acceleration = maxSpeed / timeToReachMaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 currentVelocity = rb.velocity;

        //Debug.Log(currentVelocity);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            currentVelocity += acceleration * Time.deltaTime * Vector2.left;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            currentVelocity += acceleration * Time.deltaTime * Vector2.right;
        }


        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() == true)
        {
            //do jump
            currentVelocity.y += initialJumpVel;
        }

        if (IsGrounded() == false)
        {
            //do gravity
            currentVelocity.y += gravity * Time.deltaTime;
        }


        if (currentVelocity.y < -terminalFallSpeed)
        {
            currentVelocity.y = -terminalFallSpeed;
        }

        rb.velocity = currentVelocity;

        //Debug.Log(Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down), 0.5f, default));

    }

    public bool IsWalking()
    {

        if (rb.velocity.x == 0)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    IsGrounded(true);
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    IsGrounded(false);
    //}

    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down), 0.75f, layer);

        if (hit)
        {
            return true;
        } else
        {
            return false;
        }
    }

    FacingDirection previous = FacingDirection.left;

    public FacingDirection GetFacingDirection()
    {

        if (rb.velocity.x > 0)
        {
            previous = FacingDirection.right;
            return FacingDirection.right;

        }

        if (rb.velocity.x < 0)
        {
            previous = FacingDirection.left;
            return FacingDirection.left;
        } else
        {
            return previous;
        }

    }
}
