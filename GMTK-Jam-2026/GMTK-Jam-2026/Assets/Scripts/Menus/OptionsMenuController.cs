using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer gameAudioMixer;

    [Header("Mixer Parameter Names")]
    [SerializeField]
    private string musicVolumeParameter =
        "MusicVolume";

    [SerializeField]
    private string soundEffectsVolumeParameter =
        "SoundEffectsVolume";

    [Header("Music Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicValueText;

    [Header("Sound Effects Controls")]
    [SerializeField] private Slider soundEffectsSlider;
    [SerializeField] private TMP_Text soundEffectsValueText;

    [Header("Display Controls")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Default Settings")]
    [SerializeField, Range(0f, 1f)]
    private float defaultMusicVolume = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float defaultSoundEffectsVolume = 0.8f;

    private const string MusicVolumeKey =
        "MusicVolume";

    private const string SoundEffectsVolumeKey =
        "SoundEffectsVolume";

    private const string FullscreenKey =
        "Fullscreen";

    private const float MinimumMixerVolume = -80f;

    private void OnEnable()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(
                SetMusicVolume
            );
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.onValueChanged.AddListener(
                SetSoundEffectsVolume
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(
                SetFullscreen
            );
        }
    }

    private void Start()
    {
        LoadSettings();
    }

    private void OnDisable()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                SetMusicVolume
            );
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.onValueChanged.RemoveListener(
                SetSoundEffectsVolume
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(
                SetFullscreen
            );
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        float savedMusicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                defaultMusicVolume
            );

        float savedSoundEffectsVolume =
            PlayerPrefs.GetFloat(
                SoundEffectsVolumeKey,
                defaultSoundEffectsVolume
            );

        bool savedFullscreen =
            PlayerPrefs.GetInt(
                FullscreenKey,
                Screen.fullScreen ? 1 : 0
            ) == 1;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                savedMusicVolume
            );
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.SetValueWithoutNotify(
                savedSoundEffectsVolume
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(
                savedFullscreen
            );
        }

        ApplyMusicVolume(savedMusicVolume);
        ApplySoundEffectsVolume(
            savedSoundEffectsVolume
        );

        Screen.fullScreen = savedFullscreen;
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        ApplyMusicVolume(volume);

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            volume
        );
    }

    public void SetSoundEffectsVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        ApplySoundEffectsVolume(volume);

        PlayerPrefs.SetFloat(
            SoundEffectsVolumeKey,
            volume
        );
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;

        PlayerPrefs.SetInt(
            FullscreenKey,
            fullscreen ? 1 : 0
        );
    }

    private void ApplyMusicVolume(float volume)
    {
        SetMixerVolume(
            musicVolumeParameter,
            volume
        );

        UpdateVolumeText(
            musicValueText,
            volume
        );
    }

    private void ApplySoundEffectsVolume(
        float volume)
    {
        SetMixerVolume(
            soundEffectsVolumeParameter,
            volume
        );

        UpdateVolumeText(
            soundEffectsValueText,
            volume
        );
    }

    private void SetMixerVolume(
        string parameterName,
        float normalizedVolume)
    {
        if (gameAudioMixer == null)
        {
            Debug.LogWarning(
                "OptionsMenuController is missing its GameAudioMixer reference.",
                this
            );

            return;
        }

        float mixerVolume;

        if (normalizedVolume <= 0.0001f)
        {
            mixerVolume = MinimumMixerVolume;
        }
        else
        {
            mixerVolume =
                Mathf.Log10(normalizedVolume) * 20f;
        }

        bool parameterWasFound =
            gameAudioMixer.SetFloat(
                parameterName,
                mixerVolume
            );

        if (!parameterWasFound)
        {
            Debug.LogWarning(
                $"Audio Mixer parameter '{parameterName}' was not found.",
                this
            );
        }
    }

    private static void UpdateVolumeText(
        TMP_Text valueText,
        float volume)
    {
        if (valueText == null)
        {
            return;
        }

        int percentage =
            Mathf.RoundToInt(volume * 100f);

        valueText.text = $"{percentage}%";
    }
}