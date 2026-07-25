using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public enum ButtonSoundType
    {
        SelectTo,
        SelectBack
    }

    [Header("Button Sound")]
    [SerializeField]
    private ButtonSoundType soundType =
        ButtonSoundType.SelectTo;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.AddListener(
            PlayButtonSound
        );
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                PlayButtonSound
            );
        }
    }

    private void PlayButtonSound()
    {
        if (SoundEffectsManager.Instance == null)
        {
            Debug.LogWarning(
                $"No SoundEffectsManager was found when {name} was clicked.",
                this
            );

            return;
        }

        switch (soundType)
        {
            case ButtonSoundType.SelectTo:
                SoundEffectsManager.Instance
                    .PlaySelectToSound();
                break;

            case ButtonSoundType.SelectBack:
                SoundEffectsManager.Instance
                    .PlaySelectBackSound();
                break;
        }
    }
}