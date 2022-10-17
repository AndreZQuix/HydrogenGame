using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private AudioSource movementAudio;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip sprintSound;
    private float horizontalInput;
    private float verticalInput;
    private float moveSpeed;

    [HideInInspector]
    public bool IsWallRunning;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool isReadyToJump;
    private float tempJumpForce;

    [Header("Ladder climbing")]
    [SerializeField] private float climbingSpeed;
    [SerializeField] private AudioClip climbingSound;
    [SerializeField] private UnityEvent onTryClimbing;
    [SerializeField] private UnityEvent onStartClimbing;
    [SerializeField] private UnityEvent onStopClimbing;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private AudioClip crouchSound;
    [SerializeField] private float crouchYScale;
    [SerializeField] private GameObject playerObjectToScale; // object with collider and mesh i.e. player avatar
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.C;
    [SerializeField] private KeyCode startClimbKey = KeyCode.E;
    [SerializeField] private KeyCode stopClimbKey = KeyCode.S;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private float playerHeight;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    [SerializeField] private float slopeMovementMultiplier;
    private RaycastHit slopeHit;
    private bool isExitingSlope;
    private bool turnOffSlopeHandling;

    Vector3 moveDirection;
    Rigidbody rb;

    private MovementState state;
    public void SetMovementState(MovementState st) { state = st; }
    public enum MovementState
    {
        Walking, Sprinting, InAir, Crouching, WallRunning, Climbing
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        isReadyToJump = true;
        startYScale = playerObjectToScale.transform.localScale.y;
        playerHeight = playerObjectToScale.GetComponent<CapsuleCollider>().height;
        tempJumpForce = jumpForce; // for ladder climbing
    }

    private void Update()
    {
        CheckIsGrounded();
        SetInput();
        SpeedControl();
        UpdateDrag();
        StateMachine();
        SoundControl();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && isReadyToJump && isGrounded) // try to jump
        {
            isReadyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // delay between jumps
        }
        
        if(Input.GetKeyDown(crouchKey))
            Crouch();

        if (Input.GetKeyUp(crouchKey))
            UnCrouch();

    }

    private void StateMachine()
    {
        if (state == MovementState.Climbing)
        {
            TryToClimb();
        }
        else
        {
            StopClimb(); // if player has just finished climbing

            if (IsWallRunning)
            {
                state = MovementState.WallRunning;
                moveSpeed = wallRunSpeed;
            }
            else if (Input.GetKey(crouchKey))
            {
                state = MovementState.Crouching;
                moveSpeed = crouchSpeed;
            }
            else if (isGrounded)
            {
                if (Input.GetKey(sprintKey))
                {
                    state = MovementState.Sprinting;
                    moveSpeed = sprintSpeed;
                }
                else
                {
                    state = MovementState.Walking;
                    moveSpeed = walkSpeed;
                }
            }
            else
            {
                state = MovementState.InAir;
            }
        }
    }

    private void Move()
    {
        if (!turnOffSlopeHandling)
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (OnSlope() && !isExitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

                if (rb.velocity.y > 0)
                    rb.AddForce(Vector3.down * slopeMovementMultiplier, ForceMode.Force);
            }
            else if (isGrounded)
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            else
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            if (!IsWallRunning)
                rb.useGravity = !OnSlope();
        }
    }

    private void SoundControl()
    {
        if (!movementAudio.isPlaying && IsMoving())
        {
            movementAudio.enabled = true;
            movementAudio.Play();
        }

        if ((!isGrounded || !IsMoving()) && state != MovementState.Climbing && state != MovementState.WallRunning)
            movementAudio.enabled = false;

        SwitchWalkSprintSound();
    }

    private void SwitchWalkSprintSound()
    {
        if (state == MovementState.Walking)
            movementAudio.clip = walkSound;
        else if (state == MovementState.Sprinting || state == MovementState.WallRunning)
            movementAudio.clip = sprintSound;
    }

    private bool IsMoving()
    {
        return rb.velocity.magnitude > 2f;
    }

    private void SpeedControl() // limit velocity if needed
    {
        if (OnSlope() && !isExitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private bool CheckIsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        return isGrounded;
    }

    private void UpdateDrag() // avoid "moving on ice" effect
    {
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0f;
    }

    private void Jump()
    {
        isExitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        isReadyToJump = true;
        isExitingSlope = false;
    }

    private void Crouch()
    {
        movementAudio.clip = crouchSound;
        playerObjectToScale.transform.localScale = new Vector3(playerObjectToScale.transform.localScale.x, crouchYScale, playerObjectToScale.transform.localScale.z); // reduce the size of player mesh and collider
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void UnCrouch()
    {
        playerObjectToScale.transform.localScale = new Vector3(playerObjectToScale.transform.localScale.x, startYScale, playerObjectToScale.transform.localScale.z); // set scale back to original size
    }

    private void TryToClimb()
    {
        onTryClimbing?.Invoke();

        if (Input.GetKey(startClimbKey)) // if player pressed interaction key (e.g. E), then he wants to climb (not just collides with ladder)
            StartClimb();

        if (Input.GetKey(stopClimbKey)) // if player pressed stopClimbKey, then he wants to get off the ladder
        {
            StopClimb(); 
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse); // add force to pull player to the ground
        }

        if(isGrounded && Input.GetKey(KeyCode.S)) // if player is grounded and pressed S, then he wants to leave the ladder without climbing
            state = MovementState.Walking; // this if statement exists to avoid "player sticking" to the ladder
    }

    private void StartClimb()
    {
        movementAudio.clip = climbingSound;
        rb.useGravity = false;
        turnOffSlopeHandling = true;
        jumpForce = climbingSpeed;
        onStartClimbing?.Invoke();
    }

    public void StopClimb()
    {
        rb.useGravity = true;
        turnOffSlopeHandling = false;
        jumpForce = tempJumpForce;
        onStopClimbing?.Invoke();
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
