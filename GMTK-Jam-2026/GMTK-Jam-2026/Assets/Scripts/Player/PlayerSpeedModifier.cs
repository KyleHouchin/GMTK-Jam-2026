using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerSpeedModifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Runtime State")]
    [SerializeField] private bool temporaryEffectIsActive;
    [SerializeField] private float temporaryMultiplier = 1f;
    [SerializeField] private float zoneMultiplier = 1f;
    [SerializeField] private float combinedMultiplier = 1f;

    public bool TemporaryEffectIsActive =>
        temporaryEffectIsActive;

    public float TemporaryMultiplier =>
        temporaryMultiplier;

    public float ZoneMultiplier =>
        zoneMultiplier;

    public float CombinedMultiplier =>
        combinedMultiplier;

    private Coroutine temporarySpeedRoutine;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        ResetAllSpeedModifiers();
    }

    public bool ApplyTemporarySpeedMultiplier(
        float multiplier,
        float duration)
    {
        if (playerMovement == null)
        {
            Debug.LogWarning(
                $"{name} is missing its PlayerMovement reference.",
                this
            );

            return false;
        }

        if (multiplier <= 0f || duration <= 0f)
        {
            return false;
        }

        if (temporarySpeedRoutine != null)
        {
            StopCoroutine(temporarySpeedRoutine);
        }

        temporarySpeedRoutine = StartCoroutine(
            TemporarySpeedRoutine(
                multiplier,
                duration
            )
        );

        return true;
    }

    public void SetZoneSpeedMultiplier(
        float multiplier)
    {
        if (multiplier <= 0f)
        {
            return;
        }

        zoneMultiplier = multiplier;
        ApplyCombinedMultiplier();
    }

    public void ResetZoneSpeedMultiplier()
    {
        zoneMultiplier = 1f;
        ApplyCombinedMultiplier();
    }

    public void ClearTemporarySpeedModifier()
    {
        if (temporarySpeedRoutine != null)
        {
            StopCoroutine(temporarySpeedRoutine);
            temporarySpeedRoutine = null;
        }

        temporaryEffectIsActive = false;
        temporaryMultiplier = 1f;

        ApplyCombinedMultiplier();
    }

    public void ResetAllSpeedModifiers()
    {
        if (temporarySpeedRoutine != null)
        {
            StopCoroutine(temporarySpeedRoutine);
            temporarySpeedRoutine = null;
        }

        temporaryEffectIsActive = false;
        temporaryMultiplier = 1f;
        zoneMultiplier = 1f;

        ApplyCombinedMultiplier();
    }

    private IEnumerator TemporarySpeedRoutine(
        float multiplier,
        float duration)
    {
        temporaryEffectIsActive = true;
        temporaryMultiplier = multiplier;

        ApplyCombinedMultiplier();

        yield return new WaitForSeconds(duration);

        temporaryEffectIsActive = false;
        temporaryMultiplier = 1f;
        temporarySpeedRoutine = null;

        ApplyCombinedMultiplier();
    }

    private void ApplyCombinedMultiplier()
    {
        combinedMultiplier =
            temporaryMultiplier * zoneMultiplier;

        if (playerMovement != null)
        {
            playerMovement.SetExternalSpeedMultiplier(
                combinedMultiplier
            );
        }
    }
}