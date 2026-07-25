using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundEffectsManager : MonoBehaviour
{
    public static SoundEffectsManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource glideLoopSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip selectToSound;
    [SerializeField] private AudioClip selectBackSound;

    [Header("Jump Sounds")]
    [SerializeField] private AudioClip jumpSound1;
    [SerializeField] private AudioClip jumpSound2;
    [SerializeField] private AudioClip jumpSound3;
    [SerializeField] private AudioClip jumpSound4;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip glideSound;
    [SerializeField] private AudioClip projectileSound;

    [Header("Game State Sounds")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float oneShotVolume = 1f;

    [SerializeField, Range(0f, 1f)]
    private float glideVolume = 1f;

    private int nextJumpSoundIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigureAudioSources();
        nextJumpSoundIndex = 0;
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ConfigureAudioSources()
    {
        AudioSource[] audioSources =
            GetComponents<AudioSource>();

        if (oneShotSource == null &&
            audioSources.Length > 0)
        {
            oneShotSource = audioSources[0];
        }

        if (glideLoopSource == null &&
            audioSources.Length > 1)
        {
            glideLoopSource = audioSources[1];
        }

        if (oneShotSource != null)
        {
            oneShotSource.playOnAwake = false;
            oneShotSource.loop = false;
            oneShotSource.spatialBlend = 0f;
        }

        if (glideLoopSource != null)
        {
            glideLoopSource.playOnAwake = false;
            glideLoopSource.loop = true;
            glideLoopSource.spatialBlend = 0f;
            glideLoopSource.volume = glideVolume;
        }
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        StopGlideSound();
    }

    public void PlaySelectToSound()
    {
        PlayOneShot(selectToSound);
    }

    public void PlaySelectBackSound()
    {
        PlayOneShot(selectBackSound);
    }

    public void PlayJumpSound()
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            AudioClip selectedJumpSound =
                GetJumpSound(nextJumpSoundIndex);

            nextJumpSoundIndex =
                (nextJumpSoundIndex + 1) % 4;

            if (selectedJumpSound != null)
            {
                PlayOneShot(selectedJumpSound);
                return;
            }
        }
    }

    private AudioClip GetJumpSound(int jumpSoundIndex)
    {
        switch (jumpSoundIndex)
        {
            case 0:
                return jumpSound1;

            case 1:
                return jumpSound2;

            case 2:
                return jumpSound3;

            case 3:
                return jumpSound4;

            default:
                return jumpSound1;
        }
    }

    public void PlayDashSound()
    {
        PlayOneShot(dashSound);
    }

    public void PlayProjectileSound()
    {
        PlayOneShot(projectileSound);
    }

    public void PlayVictorySound()
    {
        StopGlideSound();
        PlayOneShot(victorySound);
    }

    public void PlayGameOverSound()
    {
        StopGlideSound();
        PlayOneShot(gameOverSound);
    }

    public void StartGlideSound()
    {
        if (glideLoopSource == null ||
            glideSound == null)
        {
            return;
        }

        if (glideLoopSource.isPlaying &&
            glideLoopSource.clip == glideSound)
        {
            return;
        }

        glideLoopSource.Stop();
        glideLoopSource.clip = glideSound;
        glideLoopSource.volume = glideVolume;
        glideLoopSource.Play();
    }

    public void StopGlideSound()
    {
        if (glideLoopSource == null)
        {
            return;
        }

        glideLoopSource.Stop();
    }

    private void PlayOneShot(AudioClip sound)
    {
        if (oneShotSource == null ||
            sound == null)
        {
            return;
        }

        oneShotSource.PlayOneShot(
            sound,
            oneShotVolume
        );
    }
}