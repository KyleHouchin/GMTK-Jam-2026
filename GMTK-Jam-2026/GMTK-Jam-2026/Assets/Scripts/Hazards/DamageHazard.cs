using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageHazard : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float lifeForceDamage = 5f;

    [Header("Debugging")]
    [SerializeField] private bool logSuccessfulHits;

    private void Reset()
    {
        Collider2D hazardCollider =
            GetComponent<Collider2D>();

        if (hazardCollider != null)
        {
            hazardCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        lifeForceDamage = Mathf.Max(
            0f,
            lifeForceDamage
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerDamageReceiver damageReceiver =
            other.GetComponentInParent<PlayerDamageReceiver>();

        if (damageReceiver == null)
        {
            return;
        }

        bool damageWasApplied =
            damageReceiver.TakeDamage(
                lifeForceDamage,
                transform.position
            );

        if (damageWasApplied && logSuccessfulHits)
        {
            Debug.Log(
                $"{name} dealt {lifeForceDamage} " +
                $"Life Force damage to {other.name}.",
                this
            );
        }
    }
}