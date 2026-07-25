using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float projectileSpeed = 12f;

    [Header("Lifetime")]
    [SerializeField, Min(0.1f)]
    private float maximumLifetime = 5f;

    private Rigidbody2D projectileRigidbody;

    private void Awake()
    {
        projectileRigidbody =
            GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(
            gameObject,
            maximumLifetime
        );
    }

    public void SetVelocityDirection(
        Vector2 direction)
    {
        if (projectileRigidbody == null)
        {
            return;
        }

        direction.Normalize();

        projectileRigidbody.linearVelocity =
            direction * projectileSpeed;
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (collision.gameObject
            .CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}