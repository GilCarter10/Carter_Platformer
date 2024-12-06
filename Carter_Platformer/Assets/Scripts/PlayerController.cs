using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public enum FacingDirection
    {
        left, right
    }

    public enum WallDirection
    {
        left, right, none
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    //movement bools
    private bool left = false;
    private bool right = false;
    private bool jump = false;
    private bool dashRelease = false;
    
    //horizontal movement
    public Rigidbody2D rb;
    private float acceleration;
    public float maxSpeed;
    public float timeToReachMaxSpeed;

    //is grounded check
    public LayerMask layer;

    //vertical movement
    private float gravity;
    private float initialJumpVel;
    public float apexHeight;
    public float apexTime;
    public float terminalFallSpeed;

    //coyote time
    public float coyoteTime;
    float coyoteClock;
    private bool coyoteActive = false;
    private bool coyoteJump = false;

    //health
    public int health = 10;

    //charge dash
    public Slider chargeMeter;
    private float chargeNum;
    public float chargeRate;
    public float cooldownRate;
    private bool cooldown = false;
    public Image fill;
    public float chargeMultiplier;

    //wall jump
    public float wallJumpDistance;

    //gravity flip
    private bool upsideDown = false;
    private Vector3 scale;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        initialJumpVel = 2 * apexHeight / apexTime;
        acceleration = maxSpeed / timeToReachMaxSpeed;

        scale = transform.localScale;
    }

    private void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);
    }

    // Update is called once per frame
    void Update() 
    {
        //Debug.Log(left);
        //Debug.Log(right);
        Debug.Log(jump);

        //switching character states
        previousCharacterState = currentCharacterState;

        switch (currentCharacterState)
        {
            case CharacterState.die:
                //do nothing you're dead
                break;

            case CharacterState.jump:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        currentCharacterState = CharacterState.walk;
                    } else
                    {
                        currentCharacterState = CharacterState.idle;
                    }
                }
                break;

            case CharacterState.walk:
                //Are we NOT walking?
                if (!IsWalking())
                {
                    currentCharacterState = CharacterState.idle;
                }
                //Are we jumping?
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }
                break;

            case CharacterState.idle:
                //Are we walking?
                if (IsWalking())
                {
                    currentCharacterState = CharacterState.walk;
                }
                //Are we jumping?
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }

                break;


        }

        //die state
        if (IsDead())
        {
            currentCharacterState = CharacterState.die;
        }

        //move left input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            left = true;
        }
        
        //move right input
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            right = true;
        }

        //jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }



        //coyoteTime
        if (coyoteActive)
        {
            coyoteClock += Time.deltaTime;
            if (coyoteClock < coyoteTime)
            {
                coyoteJump = true;
            } else
            {
                coyoteJump = false;
            }
        }


        //charge dash
        chargeMeter.value = chargeNum;

        if (Input.GetKey(KeyCode.X))
        {
            if (chargeNum <= chargeMeter.maxValue && !cooldown)
            {
                chargeNum += Time.deltaTime * chargeRate;
            }

        }

        if (cooldown)
        {
            fill.color = Color.yellow;
            if (chargeNum > chargeMeter.minValue)
            {
                chargeNum -= Time.deltaTime * cooldownRate;
            } else if (chargeNum <= chargeMeter.minValue) {
                chargeNum = chargeMeter.minValue;
                cooldown = false;
            }

        } else {
            fill.color = Color.green;
        }

        //dash release input
        if (Input.GetKeyUp(KeyCode.X))
        {
            dashRelease = true;
        }


        //flip gravity
        if (!upsideDown)
        {

            scale.y = 1;
            transform.localScale = scale;

        } else
        {
            scale.y = -1;
            transform.localScale = scale;
        }

    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 currentVelocity = rb.velocity;

        //Debug.Log(currentVelocity);

        //move left physics
        if (left)
        {
            if (currentVelocity.x < maxSpeed)
            {
                currentVelocity += acceleration * Time.deltaTime * Vector2.left;
            }
            left = false;
        }

        //move right physics
        if (right)
        {
            if (currentVelocity.x < maxSpeed)
            {
                currentVelocity += acceleration * Time.deltaTime * Vector2.right;
            }
            right = false;
        }

        //jump physics
        if (jump)
        {
            if ((IsGrounded() || coyoteJump)){
                //do normal jump
                currentVelocity.y += initialJumpVel * scale.y;
            }
            
            if (GetTouchingWall() == WallDirection.right && !IsGrounded())
            {
                //do a jump to the LEFT
                currentVelocity.y += initialJumpVel * scale.y;
                currentVelocity.x -= wallJumpDistance;
            }

            if (GetTouchingWall() == WallDirection.left && !IsGrounded())
            {
                //do a jump to the RIGHT
                currentVelocity.y += initialJumpVel * scale.y;
                currentVelocity.x += wallJumpDistance;
            }
            jump = false;
        }

        //grounded check
        if (IsGrounded() == false)
        {
            //do gravity
            if (!upsideDown)
            {
                currentVelocity.y += gravity * Time.deltaTime;
            } else
            {
                currentVelocity.y -= gravity * Time.deltaTime;
            } 

            //check terminal fall speed
            if (!upsideDown)
            {
                if (currentVelocity.y < -terminalFallSpeed)
                {
                    currentVelocity.y = -terminalFallSpeed;
                }
            }
            else
            {
                if (currentVelocity.y < -terminalFallSpeed)
                {
                    currentVelocity.y = terminalFallSpeed;
                }
            }

        }

        //chargeDash physics
        if (dashRelease)
        {
            if (!cooldown)
            {
                if (GetFacingDirection() == FacingDirection.right)
                {
                    currentVelocity.x += chargeNum * chargeMultiplier;
                }
                else if (GetFacingDirection() == FacingDirection.left)
                {
                    currentVelocity.x -= chargeNum * chargeMultiplier;
                }
                cooldown = true;
            }
            dashRelease = false;
        }

        rb.velocity = currentVelocity;

    }

    //is walking check
    public bool IsWalking()
    {

        if (rb.velocity.x < 0.2 && rb.velocity.x > -0.2)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    //is grounded check
    public bool IsGrounded()
    {
        RaycastHit2D hit;

        hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down * scale.y), 0.75f, layer);

        if (hit)
        {
            coyoteClock = 0;
            coyoteActive = false;
            return true;
        } else
        {
            coyoteActive = true;
            return false;
        }

    }

    //finds which direction the wall that the player is touching
    public WallDirection GetTouchingWall()
    {
        RaycastHit2D right;
        RaycastHit2D left;

        right = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.right), 0.75f, layer);
        left = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.left), 0.75f, layer);

        if (right)
        {
            return WallDirection.right;

        } else if (left)
        {
            return WallDirection.left;

        } else {
            return WallDirection.none;

        }


    }

    //makes dead
    public bool IsDead()
    {
        return health <= 0;
    }

    //removes game object after dead
    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }


    //gets facing direction
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

    //gravity powerup trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GravityPowerup GravityPowerup = collision.gameObject.GetComponent<GravityPowerup>();
        GravityPowerup.RandomPosition();
        if (!upsideDown)
        {
            upsideDown = true;
        } else
        {
            upsideDown = false;
        }
        
    }

}
