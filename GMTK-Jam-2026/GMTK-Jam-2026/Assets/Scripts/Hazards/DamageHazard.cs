using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageHazard : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float lifeForceDamage = 5f;

    [Header("Debugging")]
    [SerializeField] private bool logSuccessfulHits;

    private Collider2D hazardCollider;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
    }

    private void OnValidate()
    {
        lifeForceDamage = Mathf.Max(0f, lifeForceDamage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(
            other,
            other.transform.position
        );
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        Vector2 damageSourcePosition =
            collision.transform.position;

        if (collision.contactCount > 0)
        {
            damageSourcePosition =
                collision.GetContact(0).point;
        }

        TryDamagePlayer(
            collision.collider,
            damageSourcePosition
        );
    }

    private void TryDamagePlayer(
        Collider2D other,
        Vector2 damageSourcePosition)
    {
        PlayerDamageReceiver damageReceiver =
            other.GetComponentInParent<PlayerDamageReceiver>();

        if (damageReceiver == null)
        {
            return;
        }

        if (hazardCollider != null)
        {
            damageSourcePosition =
                hazardCollider.ClosestPoint(
                    other.transform.position
                );
        }

        bool damageWasApplied =
            damageReceiver.TakeDamage(
                lifeForceDamage,
                damageSourcePosition
            );

        if (damageWasApplied)
        {
            if (SoundEffectsManager.Instance != null)
            {
                SoundEffectsManager.Instance
                    .PlaySpikeDamageSound();
            }

            if (logSuccessfulHits)
            {
                Debug.Log(
                    $"{name} dealt {lifeForceDamage} Life Force damage to {other.name}.",
                    this
                );
            }
        }
    }
}