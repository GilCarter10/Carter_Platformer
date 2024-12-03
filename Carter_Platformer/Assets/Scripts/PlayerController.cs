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
    bool coyoteActive = false;
    bool coyoteJump = false;

    //health
    public int health = 10;

    //charge dash
    public Slider chargeMeter;
    public float chargeNum;
    public float chargeRate;
    public float cooldownRate;
    bool cooldown = false;
    public Image fill;

    //wall jump
    public float wallJumpDistance;

    //gravity flip
    bool upsideDown = false;
    Vector3 scale;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        initialJumpVel = 2 * apexHeight / apexTime;
        acceleration = maxSpeed / timeToReachMaxSpeed;

        scale = transform.localScale;
    }

    // Update is called once per frame
    void Update() 
    {
        chargeMeter.value = chargeNum;

        previousCharacterState = currentCharacterState;

        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

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

        if (IsDead())
        {
            currentCharacterState = CharacterState.die;
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


        //CHARGE DASH
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


        //FLIP
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

        //left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            currentVelocity += acceleration * Time.deltaTime * Vector2.left;
        }

        //right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            currentVelocity += acceleration * Time.deltaTime * Vector2.right;
        }


        //jump
        if (Input.GetKeyDown(KeyCode.Space))
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


        }


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


        //chargeDash release
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (!cooldown)
            {
                if (GetFacingDirection() == FacingDirection.right)
                {
                    currentVelocity.x += chargeNum;
                }
                else if (GetFacingDirection() == FacingDirection.left)
                {
                    currentVelocity.x -= chargeNum;
                }
                cooldown = true;
            }

        }

        rb.velocity = currentVelocity;

    }

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

    public bool IsDead()
    {
        return health <= 0;
    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
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
