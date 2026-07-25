using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLoadout))]
public class PlayerAbility : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    private PlayerMovement playerMovement;
    private PlayerLoadout playerLoadout;

    [SerializeField] private float projectileTimer = 1f;    //1 second between projectiles
    private float projectileTimerCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerLoadout = GetComponent<PlayerLoadout>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        projectileTimerCounter += Time.deltaTime;

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame &&
            CanUseDash())
        {
            playerMovement.SetDash();
        }

        if (Keyboard.current.spaceKey.isPressed &&
            !playerMovement.IsGrounded() &&
            CanUseGlide() &&
            playerMovement.IsMovingDown())
        {
            playerMovement.StartGlide();
        }

        if (CanUseProjectile())
        {
            ShootProjectile();
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

    private void ShootProjectile()
    {
        if (projectileTimerCounter < projectileTimer)    //Not enough time since last projectile
        {
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                $"{name} does not have a projectile prefab assigned.",
                this
            );

            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            CreateProjectile(Vector2.up);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            CreateProjectile(Vector2.down);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            CreateProjectile(Vector2.left);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            CreateProjectile(Vector2.right);
        }
    }

    private void CreateProjectile(
        Vector2 direction)
    {
        ProjectileController projectile =
            Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            ).GetComponent<ProjectileController>();

        if (projectile == null)
        {
            Debug.LogWarning(
                "The projectile prefab does not contain a ProjectileController.",
                projectilePrefab
            );

            return;
        }

        projectile.setVelocityDirection(direction);
        projectileTimerCounter = 0f;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance
                .PlayProjectileSound();
        }
    }
}