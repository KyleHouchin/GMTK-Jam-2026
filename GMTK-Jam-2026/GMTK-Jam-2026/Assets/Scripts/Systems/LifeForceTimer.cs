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

        currentLifeForce -= Time.deltaTime;

        if (currentLifeForce <= 0f)
        {
            currentLifeForce = 0f;
            HandleLifeForceDepleted();
        }

        UpdateDisplay();
    }

    public void BeginCountdown(float startingLifeForce)
    {
        currentLifeForce = Mathf.Max(0f, startingLifeForce);
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

    public void RestoreLifeForce(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentLifeForce += amount;
        UpdateDisplay();
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