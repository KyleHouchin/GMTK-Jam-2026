using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    [SerializeField] private float projectileSpeed = 8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void setVelocityDirection(Vector2 direction)
    {
        direction.Normalize();
        rigidbody.linearVelocity = direction * projectileSpeed;
    }

    // destroy the projectile when it hits the ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string ground_tag = "Ground";

        if (collision.gameObject.CompareTag(ground_tag))
        {
            Destroy(gameObject);
        }
    }
}
