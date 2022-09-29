using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float wallRunSpeed;
    private float horizontalInput;
    private float verticalInput;
    private float moveSpeed;

    [HideInInspector]
    public bool isWallRunning;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool isReadyToJump;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    [SerializeField] private GameObject playerObjectToScale; // object with collider and mesh i.e. player avatar
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private float playerHeight;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool isExitingSlope;

    Vector3 moveDirection;
    Rigidbody rb;

    private MovementState state;
    public enum MovementState
    {
        Walking, Sprinting, InAir, Crouching, WallRunning
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        isReadyToJump = true;
        startYScale = playerObjectToScale.transform.localScale.y;
        playerHeight = playerObjectToScale.GetComponent<CapsuleCollider>().height;
    }

    private void Update()
    {
        CheckIsGrounded();
        SetInput();
        SpeedControl();
        UpdateDrag();
        StateMachine();
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
        if(isWallRunning)
        {
            state = MovementState.WallRunning;
            moveSpeed = wallRunSpeed;
        }
        
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
        }
        else if(isGrounded)
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

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !isExitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if(isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if(!isWallRunning) 
            rb.useGravity = !OnSlope();
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
        playerObjectToScale.transform.localScale = new Vector3(playerObjectToScale.transform.localScale.x, crouchYScale, playerObjectToScale.transform.localScale.z); // reduce the size of player mesh and collider
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void UnCrouch()
    {
        playerObjectToScale.transform.localScale = new Vector3(playerObjectToScale.transform.localScale.x, startYScale, playerObjectToScale.transform.localScale.z); // set scale back to original size
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
