using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float maximumMoveSpeed = 7f;
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float groundDeceleration = 70f;
    [SerializeField] private float airAcceleration = 35f;
    [SerializeField] private float airDeceleration = 20f;

    [Header("Dash Mechanics")]
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashFactor = 2.25f;

    [Header("Glide Mechanics")]
    [SerializeField] private float glideVelocity = -1.5f;
    [SerializeField, Min(0.01f)] private float glideGroundStopDistance = 0.4f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    [Header("Runtime Speed Modifier")]
    [SerializeField] private float externalSpeedMultiplier = 1f;

    public float ExternalSpeedMultiplier => externalSpeedMultiplier;
    public bool IsTooCloseToGroundToGlide => isTooCloseToGroundToGlide;
    public float DistanceToGround => distanceToGround;

    private Rigidbody2D playerRigidbody;

    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float dashCooldownCounter;
    private float baseGravityScale;
    private float dashDirection;
    private float distanceToGround = -1f;

    private bool isGrounded;
    private bool jumpReleased;
    private bool isDashing;
    private bool isGliding;
    private bool wasGlidingLastFrame;
    private bool isTooCloseToGroundToGlide;
    private bool hasAirDash;
    private bool facingRight = true;

    // These store references to the currently running Dash and Glide coroutines.
    // Keeping the references lets us safely stop them when gameplay is disabled
    // or when another action interrupts the ability.
    private Coroutine dashRoutine;
    private Coroutine glideRoutine;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        baseGravityScale = playerRigidbody.gravityScale;

        ResetExternalSpeedMultiplier();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.canceled += OnJumpCanceled;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.canceled -= OnJumpCanceled;
            jumpAction.action.Disable();
        }

        EndDash();
        EndGlide();

        horizontalInput = 0f;
    }

    private void Update()
    {
        ReadMovementInput();
        CheckIfGrounded();
        UpdateGroundDistance();
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        UpdateDashCooldownCounter();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        TryToJump();
        ApplyVariableJumpHeight();
    }

    private void ReadMovementInput()
    {
        if (moveAction == null)
        {
            horizontalInput = 0f;
            return;
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
    }

    private void CheckIfGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            // Landing restores the player's one allowed air Dash.
            hasAirDash = true;

            if (isGliding)
            {
                EndGlide();
            }
        }
    }

    private void UpdateGroundDistance()
    {
        if (groundCheck == null)
        {
            distanceToGround = -1f;
            isTooCloseToGroundToGlide = false;
            return;
        }

        RaycastHit2D groundHit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            glideGroundStopDistance,
            groundLayer
        );

        if (groundHit.collider != null)
        {
            distanceToGround = groundHit.distance;
            isTooCloseToGroundToGlide = true;

            Debug.DrawLine(
                groundCheck.position,
                groundHit.point,
                Color.green
            );
        }
        else
        {
            distanceToGround = -1f;
            isTooCloseToGroundToGlide = false;

            Debug.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * glideGroundStopDistance,
                Color.red
            );
        }
    }

    private void UpdateAnimation()
    {
        if (anim != null)
        {
            anim.SetFloat(
                "horizontal",
                Mathf.Abs(playerRigidbody.linearVelocity.x)
            );

            anim.SetFloat(
                "vertical",
                playerRigidbody.linearVelocity.y
            );

            anim.SetBool(
                "isGliding",
                isGliding
            );

            // Transform from bat back into a vampire.
            // The same transformation animation is played backward.
            bool isBecomingVampire = !isGliding && wasGlidingLastFrame;

            // Transform from vampire into a bat.
            // The transformation animation is played forward.
            bool isBecomingBat = isGliding && !wasGlidingLastFrame;

            if (isBecomingBat)
            {
                anim.SetFloat("transform_anim_speed", 1f);
                anim.Play("vampire_to_bat", 0, 0f);
            }
            else if (isBecomingVampire)
            {
                anim.SetFloat("transform_anim_speed", -1f);
                anim.Play("vampire_to_bat", 0, 1f);
            }
        }

        bool shouldFaceLeft = facingRight && horizontalInput < 0f;
        bool shouldFaceRight = !facingRight && horizontalInput > 0f;

        if (shouldFaceLeft || shouldFaceRight)
        {
            FlipFacingDirection();
        }

        wasGlidingLastFrame = isGliding;
    }

    private void FlipFacingDirection()
    {
        transform.Rotate(0f, 180f, 0f);
        facingRight = !facingRight;
    }

    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void ApplyHorizontalMovement()
    {
        float newHorizontalSpeed;

        if (!isDashing)
        {
            float targetSpeed = horizontalInput * maximumMoveSpeed * externalSpeedMultiplier;
            float currentSpeed = playerRigidbody.linearVelocity.x;
            bool playerIsTryingToMove = Mathf.Abs(horizontalInput) > 0.01f;

            float speedChangeRate;

            if (isGrounded)
            {
                if (playerIsTryingToMove)
                {
                    speedChangeRate = groundAcceleration;
                }
                else
                {
                    speedChangeRate = groundDeceleration;
                }
            }
            else
            {
                if (playerIsTryingToMove)
                {
                    speedChangeRate = airAcceleration;
                }
                else
                {
                    speedChangeRate = airDeceleration;
                }
            }

            newHorizontalSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                speedChangeRate * Time.fixedDeltaTime
            );
        }
        else
        {
            newHorizontalSpeed =
                maximumMoveSpeed *
                dashFactor *
                dashDirection *
                externalSpeedMultiplier;
        }

        playerRigidbody.linearVelocity = new Vector2(
            newHorizontalSpeed,
            playerRigidbody.linearVelocity.y
        );
    }

    private void TryToJump()
    {
        bool canUseCoyoteTime = coyoteTimeCounter > 0f;
        bool hasBufferedJump = jumpBufferCounter > 0f;

        if (!canUseCoyoteTime || !hasBufferedJump)
        {
            return;
        }

        EndGlide();

        playerRigidbody.linearVelocity = new Vector2(
            playerRigidbody.linearVelocity.x,
            jumpForce
        );

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpReleased = false;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayJumpSound();
        }
    }

    private void ApplyVariableJumpHeight()
    {
        if (!jumpReleased)
        {
            return;
        }

        if (playerRigidbody.linearVelocity.y > 0f)
        {
            playerRigidbody.linearVelocity = new Vector2(
                playerRigidbody.linearVelocity.x,
                playerRigidbody.linearVelocity.y * jumpCutMultiplier
            );
        }

        jumpReleased = false;
    }

    private void UpdateDashCooldownCounter()
    {
        if (!isDashing)
        {
            dashCooldownCounter += Time.deltaTime;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpBufferCounter = jumpBufferTime;
        jumpReleased = false;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpReleased = true;
    }

    public void SetExternalSpeedMultiplier(float multiplier)
    {
        externalSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetExternalSpeedMultiplier()
    {
        externalSpeedMultiplier = 1f;
    }

    public void SetDash()
    {
        // Do not begin another Dash while one is active or while Dash is cooling down.
        if (isDashing || dashCooldownCounter < dashCooldown)
        {
            return;
        }

        // The player gets one Dash while airborne.
        // Landing restores hasAirDash inside CheckIfGrounded().
        if (!isGrounded)
        {
            if (!hasAirDash)
            {
                return;
            }

            hasAirDash = false;
        }

        // Dashing interrupts Glide and restores normal gravity first.
        EndGlide();

        // Save the Dash direction when the Dash begins.
        // This prevents changing or canceling the Dash by changing input midway through it.
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            dashDirection = Mathf.Sign(horizontalInput);
        }
        else if (facingRight)
        {
            dashDirection = 1f;
        }
        else
        {
            dashDirection = -1f;
        }

        isDashing = true;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayDashSound();
        }

        // This should normally be null because another Dash cannot start while isDashing is true.
        // The check still prevents an old coroutine from remaining active unexpectedly.
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
        }

        dashRoutine = StartCoroutine(DashCounter());
    }

    private IEnumerator DashCounter()
    {
        // Keep Dash active for the configured Dash duration.
        yield return new WaitForSeconds(dashDuration);

        // Clear the coroutine reference before calling EndDash().
        // EndDash() will finish the ability without attempting to stop this completed coroutine.
        dashRoutine = null;
        EndDash();
    }

    private void EndDash()
    {
        // EndDash() can also be called early when PlayerMovement is disabled.
        // If the Dash coroutine is still running, stop and clear it.
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
            dashRoutine = null;
        }

        if (!isDashing)
        {
            return;
        }

        isDashing = false;
        dashCooldownCounter = 0f;

        if (playerRigidbody != null)
        {
            // Return to normal movement speed after the Dash ends.
            playerRigidbody.linearVelocity = new Vector2(
                maximumMoveSpeed * horizontalInput * externalSpeedMultiplier,
                playerRigidbody.linearVelocity.y
            );
        }
    }

    public void StartGlide()
    {
        // Glide can only begin while falling and far enough above the ground.
        if (isGrounded ||
            isTooCloseToGroundToGlide ||
            isGliding ||
            !IsMovingDown())
        {
            return;
        }

        isGliding = true;
        playerRigidbody.gravityScale = 0f;
        playerRigidbody.linearVelocityY = glideVelocity;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.StartGlideSound();
        }

        // Prevent multiple Glide coroutines from controlling gravity and velocity at once.
        if (glideRoutine != null)
        {
            StopCoroutine(glideRoutine);
        }

        glideRoutine = StartCoroutine(GlideCounter());
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMovingDown()
    {
        return playerRigidbody.linearVelocityY < 0f;
    }

    private IEnumerator GlideCounter()
    {
        // Keep Glide active while the player is falling, holding Space,
        // and has not reached the configured ground cutoff distance.
        while (!isGrounded &&
               !isTooCloseToGroundToGlide &&
               IsMovingDown() &&
               Keyboard.current != null &&
               Keyboard.current.spaceKey.isPressed)
        {
            playerRigidbody.linearVelocityY = glideVelocity;
            yield return null;
        }

        // Clear the reference first because this coroutine has finished naturally.
        glideRoutine = null;
        EndGlide();
    }

    private void EndGlide()
    {
        // EndGlide() may also be called by landing, jumping, dashing,
        // reaching the ground cutoff, or disabling PlayerMovement.
        if (glideRoutine != null)
        {
            StopCoroutine(glideRoutine);
            glideRoutine = null;
        }

        if (!isGliding)
        {
            return;
        }

        isGliding = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = baseGravityScale;
        }

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.StopGlideSound();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );

        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * glideGroundStopDistance
        );
    }
}