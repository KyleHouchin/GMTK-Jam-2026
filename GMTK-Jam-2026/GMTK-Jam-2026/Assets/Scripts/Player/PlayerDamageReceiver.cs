using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LifeForceTimer lifeForceTimer;
    [SerializeField] private RunManager runManager;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Damage Response")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float stunDuration = 0.2f;
    [SerializeField] private float horizontalKnockback = 8f;
    [SerializeField] private float verticalKnockback = 5f;

    [Header("Runtime State")]
    [SerializeField] private bool isInvincible;
    [SerializeField] private bool isStunned;

    public bool IsInvincible => isInvincible;
    public bool IsStunned => isStunned;

    private Rigidbody2D playerRigidbody;
    private Coroutine damageResponseRoutine;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    public bool TakeDamage(
        float damageAmount,
        Vector2 damageSourcePosition)
    {
        if (damageAmount <= 0f)
        {
            return false;
        }

        if (isInvincible)
        {
            return false;
        }

        if (lifeForceTimer == null)
        {
            Debug.LogWarning(
                $"{name} is missing its LifeForceTimer reference.",
                this
            );

            return false;
        }

        if (!lifeForceTimer.CountdownIsRunning)
        {
            return false;
        }

        lifeForceTimer.RemoveLifeForce(damageAmount);

        /*
         * If the damage reduced Life Force to zero,
         * LifeForceTimer has already notified RunManager.
         * RunManager will handle Game Over and freeze the player.
         */
        if (!lifeForceTimer.CountdownIsRunning)
        {
            return true;
        }

        float knockbackDirection =
            transform.position.x >= damageSourcePosition.x
                ? 1f
                : -1f;

        if (damageResponseRoutine != null)
        {
            StopCoroutine(damageResponseRoutine);
        }

        damageResponseRoutine = StartCoroutine(
            HandleDamageResponse(knockbackDirection)
        );

        return true;
    }

    private IEnumerator HandleDamageResponse(
        float knockbackDirection)
    {
        isInvincible = true;
        isStunned = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        playerRigidbody.linearVelocity = new Vector2(
            knockbackDirection * horizontalKnockback,
            verticalKnockback
        );

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        RestorePlayerMovement();

        float remainingInvincibilityTime =
            Mathf.Max(
                0f,
                invincibilityDuration - stunDuration
            );

        yield return new WaitForSeconds(
            remainingInvincibilityTime
        );

        isInvincible = false;
        damageResponseRoutine = null;
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement == null)
        {
            return;
        }

        if (runManager != null)
        {
            bool gameplayIsActive =
                runManager.RunHasStarted &&
                !runManager.GameIsOver &&
                !runManager.RunWasCompleted;

            if (gameplayIsActive)
            {
                playerMovement.enabled = true;
            }

            return;
        }

        if (lifeForceTimer != null &&
            lifeForceTimer.CountdownIsRunning)
        {
            playerMovement.enabled = true;
        }
    }
}