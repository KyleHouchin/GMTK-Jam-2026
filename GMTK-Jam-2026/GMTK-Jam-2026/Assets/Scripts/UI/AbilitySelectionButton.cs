using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AbilitySelectionButton : MonoBehaviour
{
    public enum AbilityType
    {
        BatRush,
        Glide,
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
        CacheReferences();
        ConfigureAbility();
        UpdateButtonLabel();
    }

    private void OnEnable()
    {
        CacheReferences();

        // Remove first so this listener can never be added twice.
        button.onClick.RemoveListener(OnButtonClicked);
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

    private void CacheReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (buttonLabel == null)
        {
            buttonLabel = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnButtonClicked()
    {
        if (altarManager == null)
        {
            return;
        }

        altarManager.TryToggleAbility(this);
    }

    private void ConfigureAbility()
    {
        DetectAbilityFromObjectName();
        ApplyAbilityInformation();
    }

    private void DetectAbilityFromObjectName()
    {
        string objectName = gameObject.name.ToLowerInvariant();

        if (objectName.Contains("glide"))
        {
            abilityType = AbilityType.Glide;
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
        }
    }

    private void ApplyAbilityInformation()
    {
        switch (abilityType)
        {
            case AbilityType.BatRush:
                abilityId = "Dash";
                displayName = "BAT RUSH";
                description = "Dash forward quickly, breaks crates\nPress \"Shift\" to use!";
                break;

            case AbilityType.Glide:
                abilityId = "Glide";
                displayName = "DRACULA GLIDE";
                description = "Glide through the air and descend more slowly\nHold \"Space\" while airborne to use!";
                break;

            case AbilityType.BloodShot:
                abilityId = "BloodShot";
                displayName = "BLOOD SHOT";
                description = "Launch a cursed projectile at enemies\nUse arrow keys to use! (Omni-directional)";
                break;
        }
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel == null)
        {
            return;
        }

        string selectedText = string.Empty;

        if (isSelected)
        {
            selectedText =
                "\n\n<color=#8B0000><b>SELECTED</b></color>";
        }

        buttonLabel.text =
            $"<b>{displayName}</b>\n\n" +
            $"{description}\n\n" +
            $"COST: {lifeForceCost} LIFE FORCE" +
            selectedText;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
        ConfigureAbility();
        UpdateButtonLabel();
    }
#endif
}