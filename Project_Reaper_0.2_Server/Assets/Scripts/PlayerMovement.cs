using Riptide;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // when changing Inputs array size don't forget to change Input function in Player.cs (Server)

    [SerializeField] private Player player;
    [SerializeField] private Transform camProxy;

    public Transform CamProxy
    { get { return camProxy; } set { camProxy = value; } }

    [SerializeField] private Rigidbody rb;

    public Rigidbody RB
    { get { return rb; } private set { } }

    [Header("Movement")]
    
    [SerializeField] private float walkSpeed;
    [SerializeField] private float defaultSprintSpeed;
    [SerializeField] private float boostedSprintSpeed;
    private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float defaultAccelerationMultiplier = 10f;
    [SerializeField] private float defaultAccelerationMultiplierSlope = 20f;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;

    [SerializeField] private float crouchYScale;
    private float startYScale;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;

    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMulitplier;

    private MovementState state;
    private float moveSpeed;

    [Header("Ground Check")]

    [SerializeField] private LayerMask whatIsGround;
    private Vector3 raycastOriginCharacter;
    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    [SerializeField] private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Dashing")]
    [SerializeField] private float dashCooldownTime = 5f;

    [SerializeField] private float dashForce;
    [SerializeField] private float dashDuration;

    [Header("Stair Climb")]
    public bool debugStairs = true;

    [SerializeField] private float rayDistanceUpper = 0.56f;
    [SerializeField] private float rayDistanceLower = 0.61f;
    [SerializeField] private GameObject stepRayUpper;
    [SerializeField] private GameObject stepRayLower;
    [SerializeField] private float stepHeight = 0.7f;
    [SerializeField] private float stepSmooth = 3f;

    // bool to lmit abuse of the stair climbing mechanism while jumping
    private bool validStairGrounded;

    [Header("Power Ups")]
    [SerializeField] private bool enableDoubleJump = true;
    public bool EnableDoubleJump { get { return enableDoubleJump; } set { enableDoubleJump = value; } }
    [SerializeField] private bool enableDash = true;
    public bool EnableDash { get { return enableDash; } set { enableDash = value; } }
    private bool[] inputs;

    private Vector2 move;
    private Vector3 moveDirection;
    private bool shouldJump, shouldDash, isSprinting, canDash, canDoubleJump, isDashing, isCrouching;
    private bool didTeleport;

    // for lever interaction
    [SerializeField] private bool isInteracting;

    public bool IsInteracting
    { get { return isInteracting; } private set { } }

    // reseting player position
    private bool resetPlayer;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }

    private void OnValidate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (player == null)
            player = GetComponent<Player>();
        rb.freezeRotation = true;
        Initialize();
    }

    private void Awake()
    {
        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
        sprintSpeed = defaultSprintSpeed;

    }

    private void Start()
    {
        Initialize();
        inputs = new bool[10];
        raycastOriginCharacter = transform.position + new Vector3(0f, 0.05f, 0f);
    }

    private void Update()
    {
        raycastOriginCharacter = transform.position + new Vector3(0f, 0.05f, 0f);
        CheckGrounded();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        // convert bool input vector to a Vector 2
        move = ConverBoolInputsToMovementVector(inputs);
        shouldJump = inputs[4];
        isSprinting = inputs[5];
        shouldDash = inputs[6];
        isCrouching = inputs[7];
        isInteracting = inputs[8];
        resetPlayer = inputs[9];
        if (shouldJump)
        {
            exitingSlope = true;
        }
        Move(move, shouldJump, shouldDash);
        if (resetPlayer)
        {
            Teleport(new Vector3(-18f, 3.56496644f, 235.699997f));
        }
    }

    public Vector2 ConverBoolInputsToMovementVector(bool[] inputs)
    {
        move = Vector2.zero;
        // Determine y component
        if (inputs[0])
            move.y += 1f;// Forward
        if (inputs[1])
            move.y -= 1f;// Backwards

        // Determine x component
        if (inputs[2]) // Right
            move.x += 1f;
        if (inputs[3]) // Left
            move.x -= 1f;
        return move.normalized; // Normalize to ensure it's a unit vector
    }

    private void Initialize()
    {
       
        RB.isKinematic = true;
        canDoubleJump = true;
        canDash = true;
        isDashing = false;
        isCrouching = false;
        isInteracting = false;
        startYScale = transform.localScale.y;
    }

    private void Move(Vector2 inputDirection, bool jump, bool dash)
    {
        // General Movement WSAD
        moveDirection = camProxy.forward * inputDirection.y + camProxy.right * inputDirection.x;
        // remove y velocity for standard movement
        moveDirection.y = 0;
        if (debugStairs && validStairGrounded)
        {
            StepClimb(inputDirection);
        }

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            Debug.Log("Player is on Slope");
            // apply movement to slope plain
            rb.AddForce(defaultAccelerationMultiplierSlope * moveSpeed * GetSlopeMoveDirection(), ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                // prevents bumping on slope
                rb.AddForce(Vector3.down * 20f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            // normal movement
            rb.AddForce(defaultAccelerationMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
        else if (!grounded)
        {
            // air movement
            rb.AddForce(airMulitplier * defaultAccelerationMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }

        // Dash movement
        if (dash && enableDash && canDash && !isDashing)
        {
            Dash();
        }

        //Jumping Movement
        if (jump)
            Jump();
        Invoke(nameof(ResetJump), jumpCooldown);
        // Crouching (for now without animations and therefore using some weird temporary scaling) (probably not functional atm)
        if (isCrouching)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        rb.useGravity = !OnSlope();
        SendMovement();
    }

    private void SpeedControl()
    {
        // this function limits the max speed a player can reach

        // limit Speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity on ground or air if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        if (grounded)
        {
            // ensure that jump height is consistent
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            exitingSlope = true;
        }
        else if (enableDoubleJump && canDoubleJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            canDoubleJump = false;
        }
    }

    private void ResetJump()
    {
        exitingSlope = false;
    }

    private void StateHandler()
    {
        // Dashing
        if (isDashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }
        // Crouch Speed
        else if (isCrouching)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        // Mode - Sprinting
        else if (grounded && isSprinting)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void CheckGrounded()
    {
        grounded = Physics.Raycast(raycastOriginCharacter, Vector3.down, 0.2f, whatIsGround);
        Debug.DrawRay(raycastOriginCharacter, Vector3.down * 0.2f, Color.red);
        validStairGrounded = Physics.Raycast(raycastOriginCharacter, Vector3.down, 0.15f + stepHeight, whatIsGround);
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
        {
            rb.drag = groundDrag;
            canDoubleJump = true;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(raycastOriginCharacter, Vector3.down, out slopeHit, 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        // used to get normalized force to project force on slope
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void StepClimb(Vector2 inputDirection)
    {
        Vector3 lookDirection = new Vector3(camProxy.forward.x, 0f, camProxy.forward.z).normalized;
        Vector3 moveDirection = new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;

        float rotationAngle = 0f;
        Vector3 raycastDirection;

        if (moveDirection.sqrMagnitude == 0f)
        {
            // no movement means no raycasts to prevent teleporting on stairs
            return;
        }

        if (moveDirection.x > 0f)
        {
            // move to the right
            rotationAngle += 90f;
        }
        else if (moveDirection.x < 0f)
        {
            // move to the left
            rotationAngle -= 90f;
        }
        if (moveDirection.z != 0f)
        {
            if (moveDirection.x == 0f)
            {
                // moving backwards or forwards
                rotationAngle += moveDirection.z < 0f ? +180f : +0f;
            }
            // handle diagonal movement
            else if (moveDirection.x < 0f)
            {
                rotationAngle += moveDirection.z < 0f ? -45f : +45f;
            }
            else if (moveDirection.x > 0f)
            {
                rotationAngle += moveDirection.z < 0f ? +45f : -45;
            }
        }

        Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);
        raycastDirection = rotation * lookDirection;

        Vector3 raycastStartLower = new Vector3(transform.position.x, stepRayLower.transform.position.y, transform.position.z);
        Vector3 raycastStartUpper = new Vector3(transform.position.x, stepRayUpper.transform.position.y, transform.position.z);
        Debug.DrawRay(raycastStartLower, raycastDirection * rayDistanceLower, Color.red);
        Debug.DrawRay(raycastStartUpper, raycastDirection * rayDistanceUpper, Color.blue);
        if (Physics.Raycast(raycastStartLower, raycastDirection, out _, rayDistanceLower))
        {
            if (!Physics.Raycast(raycastStartUpper, raycastDirection, out _, rayDistanceUpper))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
                rb.AddForce(raycastDirection, ForceMode.Force);
                return;
            }
        }

        // for spiral stairs
        Vector3 raycastDirection45 = Quaternion.AngleAxis(45f, Vector3.up) * raycastDirection;
        Debug.DrawRay(raycastStartLower, raycastDirection45 * rayDistanceLower, Color.green);
        Debug.DrawRay(raycastStartUpper, raycastDirection45 * rayDistanceUpper, Color.black);
        if (Physics.Raycast(raycastStartLower, raycastDirection45, out _, rayDistanceLower))
        {
            if (!Physics.Raycast(raycastStartUpper, raycastDirection45, out _, rayDistanceUpper))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
                rb.AddForce(raycastDirection45, ForceMode.Force);
                return;
            }
        }
        Vector3 raycastDirectionMinus45 = Quaternion.AngleAxis(-45f, Vector3.up) * raycastDirection;
        Debug.DrawRay(raycastStartLower, raycastDirectionMinus45 * rayDistanceLower, Color.cyan);
        Debug.DrawRay(raycastStartUpper, raycastDirectionMinus45 * rayDistanceUpper, Color.magenta);
        if (Physics.Raycast(raycastStartLower, raycastDirectionMinus45 * rayDistanceLower, out _, rayDistanceLower))
        {
            if (!Physics.Raycast(raycastStartUpper, raycastDirectionMinus45 * rayDistanceUpper, out _, rayDistanceUpper))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
                rb.AddForce(raycastDirectionMinus45, ForceMode.Force);
                return;
            }
        }
    }

    private void Dash()
    {
        isDashing = true;
        canDash = false;
        Vector3 forceToApply = camProxy.forward * dashForce;
        //disabling upward dash force
        forceToApply.y = 0;
        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
        StartCoroutine(DashCooldown());
    }

    private void ResetDash()
    {
        isDashing = false;
    }

    private Vector3 delayedForceToApply;

    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldownTime);
        canDash = true;
        Debug.Log("Dashing now available again");
    }

    public void ActivateBoostedSprintSpeed()
    {
        sprintSpeed = boostedSprintSpeed;
    }

    public void DeactivateBoostedSprintSpeed()
    {
        sprintSpeed = defaultSprintSpeed;
    }
    public void SetInput(bool[] inputs, Vector3 forward)
    {
        this.inputs = inputs;
        camProxy.forward = forward;
    }

    // useful for later when implementing respawn or move player back onto the ground after falling
    public void Teleport(Vector3 toPosition)
    {
        bool isEnabled = rb.isKinematic;
        rb.isKinematic = true;
        transform.position = toPosition;
        rb.isKinematic = isEnabled;
        didTeleport = true;
    }

    private void SendMovement()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
            return;

        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddBool(didTeleport);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
        didTeleport = false;
    }

    public void SendNewCameraRotation()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.newCameraOrientation);
        message.AddUShort(player.Id);
        message.AddVector3(camProxy.rotation.eulerAngles);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}