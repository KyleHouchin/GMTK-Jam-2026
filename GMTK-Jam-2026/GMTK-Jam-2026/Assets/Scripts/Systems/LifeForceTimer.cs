using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifeForceTimer : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text lifeForceText;
    [SerializeField] private Image lifeForceBarFill;
    [SerializeField] private TMP_Text lifeForceChangeText;

    [Header("HUD Feedback")]
    [SerializeField] private float changeTextVisibleDuration = 1f;
    [SerializeField] private float changeTextFadeDuration = 0.5f;
    [SerializeField] private Color positiveChangeColor = Color.green;
    [SerializeField] private Color negativeChangeColor = Color.red;

    [Header("Runtime State")]
    [SerializeField] private float startingLifeForce;
    [SerializeField] private float currentLifeForce;
    [SerializeField] private bool countdownIsRunning;

    public event Action LifeForceDepleted;

    public float StartingLifeForce => startingLifeForce;
    public float CurrentLifeForce => currentLifeForce;
    public bool CountdownIsRunning => countdownIsRunning;

    private bool depletionWasTriggered;
    private Coroutine changeTextRoutine;

    private void Awake()
    {
        StopCountdown();
        HideLifeForceChangeText();
        UpdateDisplay();
    }

    private void OnValidate()
    {
        changeTextVisibleDuration = Mathf.Max(
            0f,
            changeTextVisibleDuration
        );

        changeTextFadeDuration = Mathf.Max(
            0f,
            changeTextFadeDuration
        );
    }

    private void Update()
    {
        if (!countdownIsRunning)
        {
            return;
        }

        RemoveLifeForceInternal(
            Time.deltaTime,
            false
        );
    }

    public void BeginCountdown(float startingLifeForce)
    {
        this.startingLifeForce = Mathf.Max(
            0f,
            startingLifeForce
        );

        currentLifeForce =
            this.startingLifeForce;

        depletionWasTriggered = false;
        countdownIsRunning = currentLifeForce > 0f;

        HideLifeForceChangeText();
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
        RemoveLifeForceInternal(
            amount,
            true
        );
    }

    private void RemoveLifeForceInternal(
        float amount,
        bool showChangeText)
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

        float previousLifeForce =
            currentLifeForce;

        currentLifeForce = Mathf.Max(
            0f,
            currentLifeForce - amount
        );

        float removedLifeForce =
            previousLifeForce -
            currentLifeForce;

        UpdateDisplay();

        if (showChangeText &&
            removedLifeForce > 0f)
        {
            ShowLifeForceChange(
                -removedLifeForce
            );
        }

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

        float previousLifeForce =
            currentLifeForce;

        currentLifeForce += amount;

        float restoredLifeForce =
            currentLifeForce -
            previousLifeForce;

        UpdateDisplay();

        if (restoredLifeForce > 0f)
        {
            ShowLifeForceChange(
                restoredLifeForce
            );
        }
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
        if (lifeForceText != null)
        {
            lifeForceText.text =
                $"LIFE FORCE: {currentLifeForce:0.0}";
        }

        if (lifeForceBarFill == null)
        {
            return;
        }

        float normalizedLifeForce = 0f;

        if (startingLifeForce > 0f)
        {
            normalizedLifeForce =
                currentLifeForce /
                startingLifeForce;
        }

        lifeForceBarFill.fillAmount =
            Mathf.Clamp01(
                normalizedLifeForce
            );
    }

    private void ShowLifeForceChange(
        float changeAmount)
    {
        if (lifeForceChangeText == null)
        {
            return;
        }

        if (changeTextRoutine != null)
        {
            StopCoroutine(
                changeTextRoutine
            );
        }

        if (changeAmount > 0f)
        {
            lifeForceChangeText.text =
                $"+{changeAmount:0.#}";

            lifeForceChangeText.color =
                positiveChangeColor;
        }
        else
        {
            lifeForceChangeText.text =
                $"{changeAmount:0.#}";

            lifeForceChangeText.color =
                negativeChangeColor;
        }

        changeTextRoutine = StartCoroutine(
            ShowLifeForceChangeRoutine()
        );
    }

    private IEnumerator ShowLifeForceChangeRoutine()
    {
        Color startingColor =
            lifeForceChangeText.color;

        startingColor.a = 1f;
        lifeForceChangeText.color =
            startingColor;

        yield return new WaitForSecondsRealtime(
            changeTextVisibleDuration
        );

        float elapsedTime = 0f;

        while (elapsedTime <
               changeTextFadeDuration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float fadeProgress;

            if (changeTextFadeDuration > 0f)
            {
                fadeProgress =
                    elapsedTime /
                    changeTextFadeDuration;
            }
            else
            {
                fadeProgress = 1f;
            }

            Color fadingColor =
                startingColor;

            fadingColor.a = Mathf.Lerp(
                1f,
                0f,
                fadeProgress
            );

            lifeForceChangeText.color =
                fadingColor;

            yield return null;
        }

        changeTextRoutine = null;
        HideLifeForceChangeText();
    }

    private void HideLifeForceChangeText()
    {
        if (changeTextRoutine != null)
        {
            StopCoroutine(
                changeTextRoutine
            );

            changeTextRoutine = null;
        }

        if (lifeForceChangeText == null)
        {
            return;
        }

        Color hiddenColor =
            lifeForceChangeText.color;

        hiddenColor.a = 0f;

        lifeForceChangeText.color =
            hiddenColor;

        lifeForceChangeText.text =
            string.Empty;
    }
}