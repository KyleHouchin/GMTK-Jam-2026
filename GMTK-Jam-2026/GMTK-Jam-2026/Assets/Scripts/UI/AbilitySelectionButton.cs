using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AbilitySelectionButton : MonoBehaviour
{
    public enum AbilityType
    {
        BatRush,
        WingedLeap,
        BloodShot
    }

    [Header("Ability")]
    [SerializeField] private AbilityType abilityType;
    [SerializeField, Min(0)] private int lifeForceCost;

    [Header("Automatically Generated")]
    [SerializeField] private string abilityId;
    [SerializeField] private string displayName;

    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Header("References")]
    [SerializeField] private LifeForceAltarManager altarManager;
    [SerializeField] private TMP_Text buttonLabel;

    private Button button;
    private bool isSelected;

    public string AbilityId => abilityId;
    public int LifeForceCost => lifeForceCost;
    public bool IsSelected => isSelected;
    public AbilityType Type => abilityType;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>();
        }

        DetectAbilityFromObjectName();
        ApplyAbilityInformation();
        UpdateButtonLabel();
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateButtonLabel();
    }

    private void OnButtonClicked()
    {
        if (altarManager == null)
        {
            Debug.LogWarning(
                $"{name} does not have an altar manager assigned.",
                this
            );

            return;
        }

        altarManager.TryToggleAbility(this);
    }

    private void DetectAbilityFromObjectName()
    {
        string objectName = gameObject.name.ToLowerInvariant();

        if (objectName.Contains("doublejump"))
        {
            abilityType = AbilityType.WingedLeap;
            return;
        }

        if (objectName.Contains("blood"))
        {
            abilityType = AbilityType.BloodShot;
            return;
        }

        if (objectName.Contains("dash"))
        {
            abilityType = AbilityType.BatRush;
            return;
        }

        Debug.LogWarning(
            $"Could not automatically determine the ability for {gameObject.name}. " +
            "Name it DashButton, DoubleJumpButton, or BloodButton.",
            this
        );
    }

    private void ApplyAbilityInformation()
    {
        switch (abilityType)
        {
            case AbilityType.BatRush:
                abilityId = "Dash";
                displayName = "BAT RUSH";
                description = "Dash forward quickly";
                break;

            case AbilityType.WingedLeap:
                abilityId = "DoubleJump";
                displayName = "WINGED LEAP";
                description = "Jump a second time while airborne";
                break;

            case AbilityType.BloodShot:
                abilityId = "BloodShot";
                displayName = "BLOOD SHOT";
                description = "Launch a cursed projectile at enemies";
                break;
        }
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel == null)
        {
            return;
        }

        string selectedText = isSelected
            ? "\n\n<color=#8B0000><b>SELECTED</b></color>"
            : string.Empty;

        buttonLabel.text =
            $"<b>{displayName}</b>\n\n" +
            $"{description}\n\n" +
            $"COST: {lifeForceCost} LIFE FORCE" +
            selectedText;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>();
        }

        DetectAbilityFromObjectName();
        ApplyAbilityInformation();
        UpdateButtonLabel();
    }
#endif
}