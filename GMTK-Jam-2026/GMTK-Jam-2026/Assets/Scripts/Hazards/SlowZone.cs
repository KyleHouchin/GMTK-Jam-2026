using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlowZone : MonoBehaviour
{
    [Header("Slowing Effect")]
    [SerializeField] private float speedMultiplier = 0.5f;

    [Header("Debugging")]
    [SerializeField] private bool logPlayerEntry;

    private PlayerSpeedModifier affectedPlayer;
    private int playerColliderCount;

    private void Reset()
    {
        Collider2D zoneCollider =
            GetComponent<Collider2D>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        speedMultiplier = Mathf.Clamp(
            speedMultiplier,
            0.05f,
            1f
        );
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        PlayerSpeedModifier speedModifier =
            other.GetComponentInParent<PlayerSpeedModifier>();

        if (speedModifier == null)
        {
            return;
        }

        if (affectedPlayer != null &&
            affectedPlayer != speedModifier)
        {
            return;
        }

        affectedPlayer = speedModifier;
        playerColliderCount++;

        affectedPlayer.SetZoneSpeedMultiplier(
            speedMultiplier
        );

        if (logPlayerEntry)
        {
            Debug.Log(
                $"{other.name} entered {name}. " +
                $"Speed multiplier: {speedMultiplier:0.00}.",
                this
            );
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        PlayerSpeedModifier speedModifier =
            other.GetComponentInParent<PlayerSpeedModifier>();

        if (speedModifier == null ||
            speedModifier != affectedPlayer)
        {
            return;
        }

        playerColliderCount = Mathf.Max(
            0,
            playerColliderCount - 1
        );

        if (playerColliderCount > 0)
        {
            return;
        }

        affectedPlayer.ResetZoneSpeedMultiplier();
        affectedPlayer = null;
    }

    private void OnDisable()
    {
        if (affectedPlayer != null)
        {
            affectedPlayer.ResetZoneSpeedMultiplier();
        }

        affectedPlayer = null;
        playerColliderCount = 0;
    }
}