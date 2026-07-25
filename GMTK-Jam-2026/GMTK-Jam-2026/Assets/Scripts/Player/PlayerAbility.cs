using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLoadout))]
public class PlayerAbility : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField, Min(0f)]
    private float projectileSpawnDistance = 0.75f;

    [SerializeField, Min(0f)]
    private float projectileTimer = 1f;

    [Header("Diagonal Shooting")]
    [SerializeField, Range(0.02f, 0.3f)]
    private float diagonalInputWindow = 0.12f;

    private PlayerMovement playerMovement;
    private PlayerLoadout playerLoadout;
    private Collider2D[] playerColliders;

    private float projectileTimerCounter;

    private bool projectileInputPending;
    private float projectileInputStartTime;

    private float lastUpPressTime =
        float.NegativeInfinity;

    private float lastDownPressTime =
        float.NegativeInfinity;

    private float lastLeftPressTime =
        float.NegativeInfinity;

    private float lastRightPressTime =
        float.NegativeInfinity;

    private void Awake()
    {
        playerMovement =
            GetComponent<PlayerMovement>();

        playerLoadout =
            GetComponent<PlayerLoadout>();

        playerColliders =
            GetComponentsInChildren<Collider2D>();
    }

    private void OnEnable()
    {
        projectileTimerCounter =
            projectileTimer;

        ClearPendingProjectileInput();
    }

    private void OnDisable()
    {
        ClearPendingProjectileInput();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        projectileTimerCounter +=
            Time.deltaTime;

        if (Keyboard.current.leftShiftKey
                .wasPressedThisFrame &&
            CanUseDash())
        {
            playerMovement.SetDash();
        }

        if (Keyboard.current.spaceKey.isPressed &&
            !playerMovement.IsGrounded() &&
            CanUseGlide() &&
            playerMovement.IsMovingDown() && !playerMovement.IsTooCloseToGroundToGlide && !playerMovement.IsWallSliding && !playerMovement.IsWallJumping)
        {
            playerMovement.StartGlide();
        }

        if (CanUseProjectile())
        {
            UpdateProjectileInput();
        }
    }

    private bool CanUseDash()
    {
        if (playerLoadout == null)
        {
            return false;
        }

        return playerLoadout.HasBatRush;
    }

    private bool CanUseGlide()
    {
        if (playerLoadout == null)
        {
            return false;
        }

        return playerLoadout.HasGlide;
    }

    private bool CanUseProjectile()
    {
        if (playerLoadout == null)
        {
            return false;
        }

        return playerLoadout.HasBloodShot;
    }

    private void UpdateProjectileInput()
    {
        RecordProjectileKeyPresses();

        if (!projectileInputPending)
        {
            if (!WasProjectileInputPressed())
            {
                return;
            }

            if (projectileTimerCounter <
                projectileTimer)
            {
                return;
            }

            if (projectilePrefab == null)
            {
                return;
            }

            projectileInputPending = true;

            projectileInputStartTime =
                Time.unscaledTime;
        }

        Vector2 direction =
            ReadBufferedProjectileDirection();

        bool hasDiagonalDirection =
            Mathf.Abs(direction.x) > 0.01f &&
            Mathf.Abs(direction.y) > 0.01f;

        bool inputWindowExpired =
            Time.unscaledTime -
            projectileInputStartTime >=
            diagonalInputWindow;

        if (hasDiagonalDirection ||
            inputWindowExpired)
        {
            if (direction != Vector2.zero)
            {
                CreateProjectile(direction);
            }

            ClearPendingProjectileInput();
        }
    }

    private void RecordProjectileKeyPresses()
    {
        if (Keyboard.current.upArrowKey
            .wasPressedThisFrame)
        {
            lastUpPressTime =
                Time.unscaledTime;
        }

        if (Keyboard.current.downArrowKey
            .wasPressedThisFrame)
        {
            lastDownPressTime =
                Time.unscaledTime;
        }

        if (Keyboard.current.leftArrowKey
            .wasPressedThisFrame)
        {
            lastLeftPressTime =
                Time.unscaledTime;
        }

        if (Keyboard.current.rightArrowKey
            .wasPressedThisFrame)
        {
            lastRightPressTime =
                Time.unscaledTime;
        }
    }

    private bool WasProjectileInputPressed()
    {
        return
            Keyboard.current.upArrowKey
                .wasPressedThisFrame ||
            Keyboard.current.downArrowKey
                .wasPressedThisFrame ||
            Keyboard.current.leftArrowKey
                .wasPressedThisFrame ||
            Keyboard.current.rightArrowKey
                .wasPressedThisFrame;
    }

    private Vector2 ReadBufferedProjectileDirection()
    {
        bool pressingUp =
            IsKeyActive(
                Keyboard.current.upArrowKey,
                lastUpPressTime
            );

        bool pressingDown =
            IsKeyActive(
                Keyboard.current.downArrowKey,
                lastDownPressTime
            );

        bool pressingLeft =
            IsKeyActive(
                Keyboard.current.leftArrowKey,
                lastLeftPressTime
            );

        bool pressingRight =
            IsKeyActive(
                Keyboard.current.rightArrowKey,
                lastRightPressTime
            );

        float horizontalDirection = 0f;
        float verticalDirection = 0f;

        if (pressingLeft && !pressingRight)
        {
            horizontalDirection = -1f;
        }
        else if (pressingRight && !pressingLeft)
        {
            horizontalDirection = 1f;
        }

        if (pressingDown && !pressingUp)
        {
            verticalDirection = -1f;
        }
        else if (pressingUp && !pressingDown)
        {
            verticalDirection = 1f;
        }

        return new Vector2(
            horizontalDirection,
            verticalDirection
        ).normalized;
    }

    private bool IsKeyActive(
        KeyControl key,
        float lastPressTime)
    {
        bool isHeld =
            key != null &&
            key.isPressed;

        bool wasRecentlyPressed =
            Time.unscaledTime -
            lastPressTime <=
            diagonalInputWindow;

        return isHeld ||
               wasRecentlyPressed;
    }

    private void CreateProjectile(
        Vector2 direction)
    {
        direction.Normalize();

        Vector2 spawnPosition =
            (Vector2)transform.position +
            direction * projectileSpawnDistance;

        GameObject projectileObject =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

        ProjectileController projectile =
            projectileObject
                .GetComponent<ProjectileController>();

        if (projectile == null)
        {
            Destroy(projectileObject);
            return;
        }

        IgnorePlayerCollisions(
            projectileObject
        );

        projectile.SetVelocityDirection(
            direction
        );

        projectileTimerCounter = 0f;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance
                .PlayProjectileSound();
        }
    }

    private void ClearPendingProjectileInput()
    {
        projectileInputPending = false;
        projectileInputStartTime = 0f;

        lastUpPressTime =
            float.NegativeInfinity;

        lastDownPressTime =
            float.NegativeInfinity;

        lastLeftPressTime =
            float.NegativeInfinity;

        lastRightPressTime =
            float.NegativeInfinity;
    }

    private void IgnorePlayerCollisions(
        GameObject projectileObject)
    {
        Collider2D[] projectileColliders =
            projectileObject
                .GetComponentsInChildren<Collider2D>();

        foreach (
            Collider2D playerCollider
            in playerColliders)
        {
            if (playerCollider == null)
            {
                continue;
            }

            foreach (
                Collider2D projectileCollider
                in projectileColliders)
            {
                if (projectileCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    playerCollider,
                    projectileCollider
                );
            }
        }
    }
}