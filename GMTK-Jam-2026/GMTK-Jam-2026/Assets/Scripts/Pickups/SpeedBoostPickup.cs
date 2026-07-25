using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpeedBoostPickup : MonoBehaviour
{
    [Header("Speed Boost")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float effectDuration = 3f;

    [Header("Debugging")]
    [SerializeField] private bool logCollection;

    private bool wasCollected;

    private void Reset()
    {
        Collider2D pickupCollider =
            GetComponent<Collider2D>();

        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        speedMultiplier = Mathf.Max(
            1f,
            speedMultiplier
        );

        effectDuration = Mathf.Max(
            0f,
            effectDuration
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasCollected)
        {
            return;
        }

        PlayerSpeedModifier speedModifier =
            other.GetComponentInParent<PlayerSpeedModifier>();

        if (speedModifier == null)
        {
            return;
        }

        bool effectWasApplied =
            speedModifier.ApplyTemporarySpeedMultiplier(
                speedMultiplier,
                effectDuration
            );

        if (!effectWasApplied)
        {
            return;
        }

        wasCollected = true;

        if (logCollection)
        {
            Debug.Log(
                $"{name} applied a {speedMultiplier:0.0}x " +
                $"speed boost for {effectDuration:0.0} seconds.",
                this
            );
        }

        Destroy(gameObject);
    }
}