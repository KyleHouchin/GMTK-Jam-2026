using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titleBackground;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Options Buttons")]
    [SerializeField] private Button optionsBackButton;

    [Header("Level Select Buttons")]
    [SerializeField] private Button levelOneButton;
    [SerializeField] private Button levelSelectBackButton;

    [Header("Scenes")]
    [SerializeField]
    private string levelOneSceneName =
        "Level-1";

    private void Awake()
    {
        ShowMainMenu();
    }

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(
                ShowLevelSelect
            );
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(
                ShowOptions
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(
                QuitGame
            );
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.AddListener(
                ShowMainMenu
            );
        }

        if (levelOneButton != null)
        {
            levelOneButton.onClick.AddListener(
                LoadLevelOne
            );
        }

        if (levelSelectBackButton != null)
        {
            levelSelectBackButton.onClick.AddListener(
                ShowMainMenu
            );
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                ShowLevelSelect
            );
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(
                ShowOptions
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                QuitGame
            );
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.RemoveListener(
                ShowMainMenu
            );
        }

        if (levelOneButton != null)
        {
            levelOneButton.onClick.RemoveListener(
                LoadLevelOne
            );
        }

        if (levelSelectBackButton != null)
        {
            levelSelectBackButton.onClick.RemoveListener(
                ShowMainMenu
            );
        }
    }

    public void ShowMainMenu()
    {
        SetPanelActive(titleBackground, true);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(levelSelectPanel, false);
    }

    public void ShowOptions()
    {
        SetPanelActive(titleBackground, false);
        SetPanelActive(optionsPanel, true);
        SetPanelActive(levelSelectPanel, false);
    }

    public void ShowLevelSelect()
    {
        SetPanelActive(titleBackground, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(levelSelectPanel, true);
    }

    public void LoadLevelOne()
    {
        SceneManager.LoadScene(
            levelOneSceneName
        );
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