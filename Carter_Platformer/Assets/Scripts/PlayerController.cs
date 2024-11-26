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

    public LayerMask layer;
    public RaycastHit hitTarget;

    public float maxSpeed;
    public float timeToReachMaxSpeed;

    private float gravity;
    private float initialJumpVel;

    public float apexHeight;
    public float apexTime;

    public float terminalFallSpeed;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        initialJumpVel = 2 * apexHeight / apexTime;
        acceleration = maxSpeed / timeToReachMaxSpeed;
    }

    private void FixedUpdate()
    {

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


        if (Input.GetKeyDown(KeyCode.Space) /*&& IsGrounded() == true*/)
        {

            currentVelocity.y += initialJumpVel;
        }

        if (IsGrounded() == false)
        {
            //do gravity
            //currentVelocity.y += gravity * Time.deltaTime;
        }

        currentVelocity.y += gravity * Time.deltaTime;
        //currentVelocity += new Vector2(0, gravity);

        rb.velocity = currentVelocity;
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
    public bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hitTarget, 1000f, layer))
        {

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hitTarget.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
        }
        
        
        return true;

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
