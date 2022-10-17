using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float wallClimpSpeed;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;

    [Header("Exiting")]
    [SerializeField] private float exitWallTime;
    private bool isExitingWall;
    private float exitWallTimer;

    [Header("Input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode downwardsKey = KeyCode.LeftControl;
    private bool isUpwardsRunning;
    private bool isDownwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool isWallLeft;
    private bool isWallRight;

    [Header("Gravity")]
    [SerializeField] bool useGravity = true;
    [SerializeField] float gravityCounterForce;

    [Header("References")]
    [SerializeField] private Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.IsWallRunning)
            WallRun();
    }

    private void CheckForWall() // check if there is a wall
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallLayer);
    }

    private bool IsAboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isUpwardsRunning = Input.GetKey(upwardsRunKey);
        isDownwardsRunning = Input.GetKey(downwardsKey);


        if((isWallLeft || isWallRight) && verticalInput > 0f && IsAboveGround() && !isExitingWall) // if player is ready to start wall running
        {
            if (!pm.IsWallRunning)
                StartWallRun();

            if (wallRunTimer > 0)    // fall from wall timer (to avoid infinite wall running)
                wallRunTimer -= Time.deltaTime;

            if (pm.IsWallRunning && wallRunTimer <= 0)
            {
                isExitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(jumpKey))
                WallJump();
        }
        else if(isExitingWall) // smooth wall running
        {
            if (pm.IsWallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                isExitingWall = false;
        }
        else
        {
            if (pm.IsWallRunning)
                StopWallRun();
        }

    }

    private void StartWallRun()
    {
        pm.IsWallRunning = true;
        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    private void WallRun()
    {
        rb.useGravity = useGravity;
        Vector3 wallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up); // wall running direction vector

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards wall run
        if (isUpwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimpSpeed, rb.velocity.z);

        if (isDownwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimpSpeed, rb.velocity.z);

        // push to wall force
        if (!(isWallLeft && horizontalInput > 0) && !(isWallRight && horizontalInput < 0)) // if player does not want to get off the wall
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.IsWallRunning = false;
        isUpwardsRunning = false;
        isDownwardsRunning = false;
        useGravity = true;
    }

    private void WallJump()
    {
        isExitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 force = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(force, ForceMode.Impulse);
    }
}
