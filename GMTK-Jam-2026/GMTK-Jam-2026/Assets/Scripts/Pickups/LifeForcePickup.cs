using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class LifeForcePickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LifeForceTimer lifeForceTimer;

    [Header("Pickup Settings")]
    [SerializeField] private float lifeForceAmount = 5f;

    [Header("Debugging")]
    [SerializeField] private bool logCollection;

    private Animator animator;
    private Collider2D pickupCollider;

    private bool wasCollected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pickupCollider = GetComponent<Collider2D>();

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

        StartCoroutine(PlayCollectedAnimation());
    }

    private IEnumerator PlayCollectedAnimation()
    {
        // Prevent the pickup from being collected again.
        pickupCollider.enabled = false;

        // Trigger the transition to the pickup animation.
        animator.SetTrigger("Collected");

        // Wait until the Animator enters the pickup state.
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0)
                .IsName("bloodbag_picked_up")
        );

        // Wait until the animation finishes.
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            return stateInfo.IsName("bloodbag_picked_up") &&
                   stateInfo.normalizedTime >= 1f &&
                   !animator.IsInTransition(0);
        });

        Destroy(gameObject);
    }
}