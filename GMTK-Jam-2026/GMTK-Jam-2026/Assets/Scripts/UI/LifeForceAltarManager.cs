using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeForceAltarManager : MonoBehaviour
{
    [Header("Life Force")]
    [SerializeField, Min(1)] private int maximumLifeForce = 20;

    [Header("Text")]
    [SerializeField] private TMP_Text maximumLifeForceText;
    [SerializeField] private TMP_Text remainingLifeForceText;

    [Header("Ability Options")]
    [SerializeField] private List<AbilitySelectionButton> abilityOptions = new();

    private int remainingLifeForce;

    public int MaximumLifeForce => maximumLifeForce;
    public int RemainingLifeForce => remainingLifeForce;

    private void Awake()
    {
        ResetSelections();
    }

    public void TryToggleAbility(AbilitySelectionButton ability)
    {
        if (ability == null)
        {
            return;
        }

        if (ability.IsSelected)
        {
            DeselectAbility(ability);
            return;
        }

        SelectAbility(ability);
    }

    public bool IsAbilitySelected(string abilityId)
    {
        foreach (AbilitySelectionButton option in abilityOptions)
        {
            if (option != null &&
                option.AbilityId == abilityId &&
                option.IsSelected)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetSelections()
    {
        remainingLifeForce = maximumLifeForce;

        foreach (AbilitySelectionButton option in abilityOptions)
        {
            if (option != null)
            {
                option.SetSelected(false);
            }
        }

        UpdateLifeForceDisplay();
    }

    private void SelectAbility(AbilitySelectionButton ability)
    {
        if (ability.LifeForceCost > remainingLifeForce)
        {
            Debug.Log(
                $"Not enough Life Force to select {ability.AbilityId}.",
                ability
            );

            return;
        }

        remainingLifeForce -= ability.LifeForceCost;
        ability.SetSelected(true);

        UpdateLifeForceDisplay();
    }

    private void DeselectAbility(AbilitySelectionButton ability)
    {
        remainingLifeForce += ability.LifeForceCost;

        remainingLifeForce = Mathf.Min(
            remainingLifeForce,
            maximumLifeForce
        );

        ability.SetSelected(false);

        UpdateLifeForceDisplay();
    }

    private void UpdateLifeForceDisplay()
    {
        if (maximumLifeForceText != null)
        {
            maximumLifeForceText.text =
                $"MAXIMUM LIFE FORCE: {maximumLifeForce}";
        }

        if (remainingLifeForceText != null)
        {
            remainingLifeForceText.text =
                $"ENTER WITH: {remainingLifeForce} LIFE FORCE";
        }
    }
}