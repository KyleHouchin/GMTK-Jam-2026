using System;
using TMPro;
using UnityEngine;

public class LifeForceTimer : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text lifeForceText;

    [Header("Runtime State")]
    [SerializeField] private float currentLifeForce;
    [SerializeField] private bool countdownIsRunning;

    public event Action LifeForceDepleted;

    public float CurrentLifeForce => currentLifeForce;
    public bool CountdownIsRunning => countdownIsRunning;

    private bool depletionWasTriggered;

    private void Awake()
    {
        StopCountdown();
        UpdateDisplay();
    }

    private void Update()
    {
        if (!countdownIsRunning)
        {
            return;
        }

        RemoveLifeForce(Time.deltaTime);
    }

    public void BeginCountdown(float startingLifeForce)
    {
        currentLifeForce = Mathf.Max(
            0f,
            startingLifeForce
        );

        depletionWasTriggered = false;
        countdownIsRunning = currentLifeForce > 0f;

        UpdateDisplay();

        if (currentLifeForce <= 0f)
        {
            HandleLifeForceDepleted();
        }
    }

    public void StopCountdown()
    {
        countdownIsRunning = false;
    }

    public void RemoveLifeForce(float amount)
    {
        if (!countdownIsRunning)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        if (depletionWasTriggered)
        {
            return;
        }

        currentLifeForce = Mathf.Max(
            0f,
            currentLifeForce - amount
        );

        UpdateDisplay();

        if (currentLifeForce <= 0f)
        {
            HandleLifeForceDepleted();
        }
    }

    public void RestoreLifeForce(float amount)
    {
        if (!countdownIsRunning)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        if (depletionWasTriggered)
        {
            return;
        }

        currentLifeForce += amount;
        UpdateDisplay();
    }

    public void DepleteLifeForce()
    {
        if (!countdownIsRunning)
        {
            return;
        }

        if (depletionWasTriggered)
        {
            return;
        }

        currentLifeForce = 0f;
        UpdateDisplay();
        HandleLifeForceDepleted();
    }

    private void HandleLifeForceDepleted()
    {
        countdownIsRunning = false;

        if (depletionWasTriggered)
        {
            return;
        }

        depletionWasTriggered = true;

        UpdateDisplay();
        LifeForceDepleted?.Invoke();
    }

    private void UpdateDisplay()
    {
        if (lifeForceText == null)
        {
            return;
        }

        lifeForceText.text =
            $"LIFE FORCE: {currentLifeForce:0.0}";
    }
}