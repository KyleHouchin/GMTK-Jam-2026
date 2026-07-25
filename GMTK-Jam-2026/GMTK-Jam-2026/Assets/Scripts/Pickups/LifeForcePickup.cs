using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LifeForcePickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LifeForceTimer lifeForceTimer;

    [Header("Pickup Settings")]
    [SerializeField] private float lifeForceAmount = 5f;

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
        lifeForceAmount = Mathf.Max(
            0f,
            lifeForceAmount
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasCollected)
        {
            return;
        }

        PlayerLoadout playerLoadout =
            other.GetComponentInParent<PlayerLoadout>();

        if (playerLoadout == null)
        {
            return;
        }

        if (lifeForceTimer == null)
        {
            Debug.LogWarning(
                $"{name} is missing its LifeForceTimer reference.",
                this
            );

            return;
        }

        if (!lifeForceTimer.CountdownIsRunning)
        {
            return;
        }

        wasCollected = true;

        lifeForceTimer.RestoreLifeForce(
            lifeForceAmount
        );

        if (logCollection)
        {
            Debug.Log(
                $"{name} restored {lifeForceAmount} Life Force.",
                this
            );
        }

        Destroy(gameObject);
    }
}