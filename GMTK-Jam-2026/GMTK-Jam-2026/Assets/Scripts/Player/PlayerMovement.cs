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
    [SerializeField, Min(0.01f)] private float glideGroundStopDistance = 0.5f;
    private float maxRaycastLength = 50f;

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

    // main
    public float ExternalSpeedMultiplier => externalSpeedMultiplier;
    public bool IsTooCloseToGroundToGlide => isTooCloseToGroundToGlide;
    public float DistanceToGround => distanceToGround;

    // player-wallmechanics
    [Header("Wall Mechanics")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private float timeBeforeSlipping = 0.5f;
    private float timeBeforeSlippingCounter;
    private bool isWallSliding;
    public bool IsWallSliding => isWallSliding;
    private bool isWallJumping;
    public bool IsWallJumping => isWallJumping;
    [SerializeField] private Vector2 wallJumpMagnitude = new Vector2(8, 16);
    private float wallJumpDirection;
    [SerializeField] private float wallJumpTime = 0.2f;
    private float wallSlidingBufferCounter;
    [SerializeField] private float wallSlidingBuffer = 0.12f;

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

    /*
     * A Coroutine lets Unity run a method over multiple frames.
     *
     * Normal methods run from beginning to end immediately.
     * Coroutine methods can pause at a "yield return" statement
     * and continue later.
     *
     * These variables store references to the currently running
     * Dash and Glide coroutines.
     *
     * Keeping these references allows us to stop the correct
     * coroutine if the ability is interrupted.
     */
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

        /*
         * The script can be disabled when the player wins,
         * loses, or returns to the shop.
         *
         * EndDash() and EndGlide() stop their active coroutines
         * and restore the player to a normal state.
         */
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
        IsWalled();
        TryToWallJump();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        TryToJump();
        ApplyVariableJumpHeight();
        WallSlide();
    }

    private void TryToWallJump()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            wallSlidingBufferCounter = 0;
            Debug.Log("This is the case");
        }
        else
        {
            wallSlidingBufferCounter += Time.deltaTime;
        }
        Debug.Log(isWallSliding + " " + (wallSlidingBufferCounter < wallSlidingBuffer));
        //You can wall jump if you are currently sliding down a wall (isWallSliding = true)
        if (isWallSliding && wallSlidingBufferCounter < wallSlidingBuffer)
        {
            Debug.Log("This is the case too");
            wallSlidingBufferCounter = 0;
            //The jump itself
            //Flip the character if they are facing the wrong way
            wallJumpDirection = facingRight ? -1 : 1;
            FlipFacingDirection();
            isWallJumping = true;
            isWallSliding = false;
            playerRigidbody.linearVelocity = new Vector2(wallJumpDirection * wallJumpMagnitude.x, wallJumpMagnitude.y);
            StartCoroutine(WallJumpDuration());
        }

    }

    private IEnumerator WallJumpDuration()
    {
        yield return new WaitForSeconds(wallJumpTime);
        isWallJumping = false;

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
                /*
                 * Landing interrupts Glide.
                 *
                 * EndGlide() stops GlideCounter(),
                 * restores gravity, and stops the Glide sound.
                 */
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
            transform.position,
            Vector2.down,
            maxRaycastLength,
            groundLayer
        );

        if (groundHit.collider != null)
        {
            distanceToGround = groundHit.distance;
            isTooCloseToGroundToGlide = (distanceToGround < glideGroundStopDistance);

            Debug.DrawLine(
                transform.position,
                groundHit.point,
                Color.green
            );
        }
        else
        {
            distanceToGround = -1f;
            isTooCloseToGroundToGlide = false;
        }
    }

    private void UpdateAnimation()
    {
        if(isWallJumping)
        {
            Debug.Log("Wall jump happening, move on");
            return;
        }
        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(playerRigidbody.linearVelocity.x));

            anim.SetFloat("vertical", playerRigidbody.linearVelocity.y);

            anim.SetBool("isGliding", isGliding);

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
        if (isWallJumping)
        {
            return;
        }
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
            /*
             * While isDashing is true, normal movement is replaced
             * with the saved Dash direction and increased speed.
             *
             * DashCounter() controls how long isDashing stays true.
             */
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

        // Jumping interrupts Glide before applying the jump.
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
        if (isDashing ||
            dashCooldownCounter < dashCooldown
            || isWallJumping)
        {
            return;
        }

        // player is in air
        if (!isGrounded)
        {
            // no air dash available, don't dash
            if (!hasAirDash)
            {
                return;
            }
            // use up air dash
            hasAirDash = false;
        }

        // begin player dash!!!!!
        isDashing = true;
        EndGlide(); // interrupt glide

        if (Mathf.Abs(horizontalInput) > 0.01f) // set dash direction of horizontal input
        {
            dashDirection = Mathf.Sign(horizontalInput);
        }
        else // no player input, dash direction player is facing
        {
            dashDirection = facingRight ? 1f : -1f;
        }

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayDashSound();
        }

        // stop any already running dash coroutines (only one at a time)
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
        }

        dashRoutine = StartCoroutine(DashCounter());
    }

    private IEnumerator DashCounter()
    {
        // pause just this method (coroutine)
        yield return new WaitForSeconds(dashDuration);

        // Reaching this line means the wait completed normally.
        dashRoutine = null;

        // Finish the Dash and return to normal movement.
        EndDash();

        // coroutine ends, do not need to call stop coroutine
    }

    private void EndDash()
    {
        /*
         * EndDash can be called in two ways:
         *
         * 1. Normally, after DashCounter finishes waiting.
         * 2. Early, when the script is disabled or Dash is interrupted.
         *
         * If the coroutine is still active, stop it so it cannot
         * continue later and call EndDash a second time.
         */
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
            dashRoutine = null;
        }

        /*
         * If Dash already ended, there is nothing else to reset.
         *
         * This protects the method from being called more than once.
         */
        if (!isDashing)
        {
            return;
        }

        isDashing = false;

        // reset cooldown timer
        dashCooldownCounter = 0f;

        if (playerRigidbody != null)
        {
            /*
             * Dash uses a much higher horizontal speed.
             *
             * When it ends, return the player to normal movement
             * speed based on their current horizontal input.
             */
            playerRigidbody.linearVelocity = new Vector2(
                maximumMoveSpeed * horizontalInput * externalSpeedMultiplier,
                playerRigidbody.linearVelocity.y
            );
        }
    }

    public void StartGlide()
    {
        /*
         * Glide can only begin when:
         *
         * - The player is not grounded.
         * - The player is not near the ground cutoff.
         * - Glide is not already active.
         * - The player is moving downward.
         */
        if (isGrounded ||
            isTooCloseToGroundToGlide ||
            isGliding ||
            !IsMovingDown())
        {
            return;
        }

        /*
         * Mark Glide as active.
         *
         * Gravity is temporarily disabled so the Rigidbody does
         * not continue accelerating downward.
         *
         * The player's vertical speed is then held at the slower
         * configured glideVelocity.
         */
        isGliding = true;
        playerRigidbody.gravityScale = 0f;
        playerRigidbody.linearVelocityY = glideVelocity;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.StartGlideSound();
        }

        /*
         * Only one GlideCounter coroutine should ever control
         * the player's gravity and vertical velocity.
         *
         * Stop an older one before starting a new one.
         */
        if (glideRoutine != null)
        {
            StopCoroutine(glideRoutine);
        }

        /*
         * Start GlideCounter and save its reference.
         *
         * Unlike DashCounter, which waits for a fixed duration,
         * GlideCounter keeps checking its conditions every frame.
         */
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
        /*
         * This while loop represents the conditions required
         * for Glide to remain active.
         *
         * Glide continues while:
         *
         * - The player has not landed.
         * - The player is not too close to the ground.
         * - The player is still moving downward.
         * - A keyboard exists.
         * - The player is still holding Space.
         */
        while (!isGrounded &&
               !isTooCloseToGroundToGlide &&
               Keyboard.current != null &&
               Keyboard.current.spaceKey.isPressed)
        {
            /*
             * Keep the downward speed at the configured Glide speed.
             *
             * Without this line, another physics interaction could
             * change the player's vertical velocity during Glide.
             */
            playerRigidbody.linearVelocityY = glideVelocity;

            /*
             * "yield return null" pauses this coroutine until the
             * next frame.
             *
             * On the next frame, Unity returns here and checks the
             * while-loop conditions again.
             *
             * This is what allows Glide to remain active over time
             * without freezing the game.
             */
            yield return null;
        }

        /*
         * Reaching this point means at least one Glide condition
         * became false.
         *
         * Examples:
         * - Space was released.
         * - The player landed.
         * - The player reached the ground cutoff.
         * - The player stopped moving downward.
         */

        /*
         * This coroutine has finished naturally, so clear its
         * reference before calling EndGlide().
         */
        glideRoutine = null;

        // Restore normal player physics and stop the Glide sound.
        EndGlide();
    }

    private void EndGlide()
    {
        /*
         * EndGlide can be called normally by GlideCounter, or early
         * because the player landed, jumped, dashed, won, or lost.
         *
         * Stop the active coroutine if it has not finished naturally.
         */
        if (glideRoutine != null)
        {
            StopCoroutine(glideRoutine);
            glideRoutine = null;
        }

        /*
         * If Glide already ended, there is nothing else to reset.
         *
         * This prevents gravity and audio from being reset repeatedly.
         */
        if (!isGliding)
        {
            return;
        }

        isGliding = false;

        /*
         * Restore the Rigidbody's normal gravity value that was
         * saved in Awake().
         */
        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = baseGravityScale;
        }

        /*
         * Stop the looping Glide sound when the ability ends.
         */
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

    private bool IsWalled()
    {
        bool isWalled = Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && IsMovingDown() && ((facingRight && horizontalInput > 0.01f) || (!facingRight && horizontalInput < -0.01f)) && !isGliding)
        {
            timeBeforeSlippingCounter += Time.deltaTime;
            isWallSliding = true;
            if(isWallJumping)
            {
                isWallJumping = false;
            }
            if(timeBeforeSlippingCounter < timeBeforeSlipping)
            {
                playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocityX, 0);
            }
            else
            {
                playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocityX, Mathf.Clamp(playerRigidbody.linearVelocityY, -wallSlidingSpeed, float.MaxValue));
            }
        }
        else
        {
            isWallSliding = false;
            timeBeforeSlippingCounter = 0;
        }
    }
}