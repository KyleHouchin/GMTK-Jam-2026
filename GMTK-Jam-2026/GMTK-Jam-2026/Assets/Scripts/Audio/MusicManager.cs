using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Menu Music")]
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private AudioClip levelSelectMusic;

    [Header("Gameplay Music")]
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("Scene Names")]
    [SerializeField]
    private string titleSceneName =
        "TitleScreen";

    [SerializeField]
    private string levelSelectSceneName =
        "LevelSelect";

    [SerializeField]
    private string gameplaySceneName =
        "InitialLevel";

    [Header("Playback Settings")]
    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.6f;

    [SerializeField] private float fadeDuration = 0.5f;

    private AudioSource musicSource;
    private Coroutine musicTransitionRoutine;

    private void Awake()
    {
        // Prevent multiple MusicManagers from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this object when changing scenes.
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            PlayMusicForScene(
                SceneManager.GetActiveScene().name
            );
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

    private void OnValidate()
    {
        musicVolume = Mathf.Clamp01(musicVolume);
        fadeDuration = Mathf.Max(0f, fadeDuration);
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip requestedMusic =
            GetMusicForScene(sceneName);

        if (requestedMusic == null)
        {
            Debug.LogWarning(
                $"MusicManager has no music assigned " +
                $"for scene: {sceneName}",
                this
            );

            return;
        }

        ChangeMusic(requestedMusic);
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        if (sceneName == titleSceneName)
        {
            return titleMusic;
        }

        if (sceneName == levelSelectSceneName)
        {
            return levelSelectMusic;
        }

        if (sceneName == gameplaySceneName)
        {
            return gameplayMusic;
        }

        return null;
    }

    public void PlayGameplayMusic()
    {
        ChangeMusic(gameplayMusic);
    }

    public void PlayVictoryMusic()
    {
        ChangeMusic(victoryMusic);
    }

    public void PlayGameOverMusic()
    {
        ChangeMusic(gameOverMusic);
    }

    private void ChangeMusic(AudioClip requestedMusic)
    {
        if (requestedMusic == null)
        {
            Debug.LogWarning(
                "The requested music clip has not been assigned.",
                this
            );

            return;
        }

        // Do not restart a song that is already playing.
        if (musicSource.clip == requestedMusic &&
            musicSource.isPlaying)
        {
            return;
        }

        if (musicTransitionRoutine != null)
        {
            StopCoroutine(musicTransitionRoutine);
        }

        musicTransitionRoutine = StartCoroutine(
            ChangeMusicRoutine(requestedMusic)
        );
    }

    private IEnumerator ChangeMusicRoutine(
        AudioClip newMusic)
    {
        // Fade out the current song.
        if (musicSource.isPlaying &&
            fadeDuration > 0f)
        {
            float startingVolume =
                musicSource.volume;

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float fadeProgress =
                    elapsedTime / fadeDuration;

                musicSource.volume = Mathf.Lerp(
                    startingVolume,
                    0f,
                    fadeProgress
                );

                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = newMusic;
        musicSource.volume = 0f;
        musicSource.Play();

        // Change instantly when fade duration is zero.
        if (fadeDuration <= 0f)
        {
            musicSource.volume = musicVolume;
            musicTransitionRoutine = null;
            yield break;
        }

        // Fade in the new song.
        float fadeInElapsedTime = 0f;

        while (fadeInElapsedTime < fadeDuration)
        {
            fadeInElapsedTime +=
                Time.unscaledDeltaTime;

            float fadeProgress =
                fadeInElapsedTime / fadeDuration;

            musicSource.volume = Mathf.Lerp(
                0f,
                musicVolume,
                fadeProgress
            );

            yield return null;
        }

        musicSource.volume = musicVolume;
        musicTransitionRoutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null &&
            musicTransitionRoutine == null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void StopMusic()
    {
        if (musicTransitionRoutine != null)
        {
            StopCoroutine(musicTransitionRoutine);
            musicTransitionRoutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}