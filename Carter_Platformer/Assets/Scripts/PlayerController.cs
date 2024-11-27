using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public enum FacingDirection
    {
        left, right
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    public Rigidbody2D rb;
    private float acceleration;
    public float maxSpeed;
    public float timeToReachMaxSpeed;

    public LayerMask layer;

    private float gravity;
    private float initialJumpVel;
    public float apexHeight;
    public float apexTime;
    public float terminalFallSpeed;

    public float coyoteTime;
    float coyoteClock;
    bool coyoteActive = false;
    bool coyoteJump = false;


    public int health = 10;

    public Slider chargeMeter;
    public float chargeNum;
    public float chargeRate;
    public float cooldownRate;
    bool cooldown = false;
    public Image fill;

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
        if (Input.GetKeyUp(KeyCode.X))
        {
            if (!cooldown)
            {
                if (GetFacingDirection() == FacingDirection.right)
                {
                    rb.AddForce(new Vector2(chargeNum, 0), ForceMode2D.Impulse);
                } else if (GetFacingDirection() == FacingDirection.left)
                {
                    rb.AddForce(new Vector2(-chargeNum, 0), ForceMode2D.Impulse);
                }
                cooldown = true;
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


        if (Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || coyoteJump))
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down), 0.75f, layer);

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


}
