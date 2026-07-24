using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelGoal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunManager runManager;

    private bool goalWasReached;

    private void Reset()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();
        goalCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalWasReached)
        {
            return;
        }

        PlayerLoadout playerLoadout =
            other.GetComponent<PlayerLoadout>();

        if (playerLoadout == null)
        {
            return;
        }

        if (runManager == null)
        {
            Debug.LogWarning(
                $"{name} does not have a RunManager assigned.",
                this
            );

            return;
        }

        goalWasReached = true;
        runManager.CompleteRun();
    }
}