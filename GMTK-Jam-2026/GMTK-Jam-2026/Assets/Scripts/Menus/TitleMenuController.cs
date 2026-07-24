using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titleBackground;
    [SerializeField] private GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Options Buttons")]
    [SerializeField] private Button optionsBackButton;

    [Header("Scenes")]
    [SerializeField] private string levelSelectSceneName = "LevelSelect";

    private void Awake()
    {
        ShowMainMenu();
    }

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OpenLevelSelect);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(ShowOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.AddListener(ShowMainMenu);
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OpenLevelSelect);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(ShowOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.RemoveListener(ShowMainMenu);
        }
    }

    public void ShowMainMenu()
    {
        SetPanelActive(titleBackground, true);
        SetPanelActive(optionsPanel, false);
    }

    public void ShowOptions()
    {
        SetPanelActive(titleBackground, false);
        SetPanelActive(optionsPanel, true);
    }

    public void OpenLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetPanelActive(
        GameObject panel,
        bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}