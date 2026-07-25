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
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashFactor = 5f;

    [Header("Glide Mechanics")]
    [SerializeField] private float glideVelocity = -1.5f;

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

    public float ExternalSpeedMultiplier =>
        externalSpeedMultiplier;

    private Rigidbody2D playerRigidbody;

    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float dashCooldownCounter;
    private float baseGravityScale;

    private bool isGrounded;
    private bool jumpReleased;
    private bool isDashing;
    private bool hasAirDash;
    private bool facingRight = true;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        baseGravityScale =
            playerRigidbody.gravityScale;

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

            jumpAction.action.performed +=
                OnJumpPerformed;

            jumpAction.action.canceled +=
                OnJumpCanceled;
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
            jumpAction.action.performed -=
                OnJumpPerformed;

            jumpAction.action.canceled -=
                OnJumpCanceled;

            jumpAction.action.Disable();
        }
    }

    private void Update()
    {
        ReadMovementInput();
        CheckIfGrounded();
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

        Vector2 moveInput =
            moveAction.action.ReadValue<Vector2>();

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
            hasAirDash = true;
        }
    }

    private void UpdateAnimation()
    {
        if (anim != null)
        {
            anim.SetFloat(
                "horizontal",
                Mathf.Abs(
                    playerRigidbody.linearVelocity.x
                )
            );

            anim.SetFloat(
                "vertical",
                playerRigidbody.linearVelocity.y
            );
        }

        bool shouldFaceLeft =
            facingRight && horizontalInput < 0f;

        bool shouldFaceRight =
            !facingRight && horizontalInput > 0f;

        if (shouldFaceLeft || shouldFaceRight)
        {
            FlipFacingDirection();
        }
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
            float targetSpeed =
                horizontalInput *
                maximumMoveSpeed *
                externalSpeedMultiplier;

            float currentSpeed =
                playerRigidbody.linearVelocity.x;

            bool playerIsTryingToMove =
                Mathf.Abs(horizontalInput) > 0.01f;

            float speedChangeRate;

            if (isGrounded)
            {
                speedChangeRate =
                    playerIsTryingToMove
                        ? groundAcceleration
                        : groundDeceleration;
            }
            else
            {
                speedChangeRate =
                    playerIsTryingToMove
                        ? airAcceleration
                        : airDeceleration;
            }

            newHorizontalSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                speedChangeRate *
                Time.fixedDeltaTime
            );
        }
        else
        {
            newHorizontalSpeed =
                maximumMoveSpeed *
                dashFactor *
                horizontalInput *
                externalSpeedMultiplier;
        }

        playerRigidbody.linearVelocity =
            new Vector2(
                newHorizontalSpeed,
                playerRigidbody.linearVelocity.y
            );
    }

    private void TryToJump()
    {
        bool canUseCoyoteTime =
            coyoteTimeCounter > 0f;

        bool hasBufferedJump =
            jumpBufferCounter > 0f;

        if (!canUseCoyoteTime ||
            !hasBufferedJump)
        {
            return;
        }

        playerRigidbody.linearVelocity =
            new Vector2(
                playerRigidbody.linearVelocity.x,
                jumpForce
            );

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpReleased = false;
    }

    private void ApplyVariableJumpHeight()
    {
        if (!jumpReleased)
        {
            return;
        }

        if (playerRigidbody.linearVelocity.y > 0f)
        {
            playerRigidbody.linearVelocity =
                new Vector2(
                    playerRigidbody.linearVelocity.x,
                    playerRigidbody.linearVelocity.y *
                    jumpCutMultiplier
                );
        }

        jumpReleased = false;
    }

    private void UpdateDashCooldownCounter()
    {
        dashCooldownCounter += Time.deltaTime;
    }

    private void OnJumpPerformed(
        InputAction.CallbackContext context)
    {
        jumpBufferCounter = jumpBufferTime;
        jumpReleased = false;
    }

    private void OnJumpCanceled(
        InputAction.CallbackContext context)
    {
        jumpReleased = true;
    }

    public void SetExternalSpeedMultiplier(
        float multiplier)
    {
        externalSpeedMultiplier = Mathf.Max(
            0f,
            multiplier
        );
    }

    public void ResetExternalSpeedMultiplier()
    {
        externalSpeedMultiplier = 1f;
    }

    public void SetDash()
    {
        if (isDashing ||
            dashCooldownCounter <= dashCooldown)
        {
            return;
        }

        if (!isGrounded)
        {
            if (!hasAirDash)
            {
                return;
            }

            hasAirDash = false;
        }

        isDashing = true;

        StartCoroutine(
            DashCounter()
        );
    }

    private IEnumerator DashCounter()
    {
        yield return new WaitForSeconds(
            dashDuration
        );

        isDashing = false;
        dashCooldownCounter = 0f;

        playerRigidbody.linearVelocity =
            new Vector2(
                maximumMoveSpeed *
                horizontalInput *
                externalSpeedMultiplier,
                playerRigidbody.linearVelocity.y
            );
    }

    public void StartGlide()
    {
        if (isGrounded)
        {
            return;
        }

        playerRigidbody.gravityScale = 0f;

        float previousYVelocity =
            playerRigidbody.linearVelocityY;

        playerRigidbody.linearVelocityY =
            glideVelocity;

        StartCoroutine(
            GlideCounter(previousYVelocity)
        );
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMovingDown()
    {
        return playerRigidbody.linearVelocityY < 0f;
    }

    private IEnumerator GlideCounter(
        float previousYVelocity)
    {
        while (
            !isGrounded &&
            IsMovingDown() &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed)
        {
            yield return null;
        }

        playerRigidbody.gravityScale =
            baseGravityScale;

        playerRigidbody.linearVelocityY =
            previousYVelocity;
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
    }
}