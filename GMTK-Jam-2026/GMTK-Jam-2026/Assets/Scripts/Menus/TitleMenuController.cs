using TMPro;
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
    [SerializeField] private Button levelTwoButton;
    [SerializeField] private Button levelThreeButton;
    [SerializeField] private Button levelSelectBackButton;

    [Header("Level Select Button Text")]
    [SerializeField] private TMP_Text levelOneButtonText;
    [SerializeField] private TMP_Text levelTwoButtonText;
    [SerializeField] private TMP_Text levelThreeButtonText;

    [Header("Scenes")]
    [SerializeField] private string levelOneSceneName = "Level-1";
    [SerializeField] private string levelTwoSceneName = "Level-2";
    [SerializeField] private string levelThreeSceneName = "Level-3";

    private static bool openLevelSelectOnLoad;

    private void Awake()
    {
        UpdateLevelButtons();

        if (openLevelSelectOnLoad)
        {
            openLevelSelectOnLoad = false;
            ShowLevelSelect();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void OnEnable()
    {
        AddButtonListeners();
        UpdateLevelButtons();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void AddButtonListeners()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(ShowLevelSelect);
            playButton.onClick.AddListener(ShowLevelSelect);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(ShowOptions);
            optionsButton.onClick.AddListener(ShowOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.RemoveListener(ShowMainMenu);
            optionsBackButton.onClick.AddListener(ShowMainMenu);
        }

        if (levelOneButton != null)
        {
            levelOneButton.onClick.RemoveListener(LoadLevelOne);
            levelOneButton.onClick.AddListener(LoadLevelOne);
        }

        if (levelTwoButton != null)
        {
            levelTwoButton.onClick.RemoveListener(LoadLevelTwo);
            levelTwoButton.onClick.AddListener(LoadLevelTwo);
        }

        if (levelThreeButton != null)
        {
            levelThreeButton.onClick.RemoveListener(LoadLevelThree);
            levelThreeButton.onClick.AddListener(LoadLevelThree);
        }

        if (levelSelectBackButton != null)
        {
            levelSelectBackButton.onClick.RemoveListener(ShowMainMenu);
            levelSelectBackButton.onClick.AddListener(ShowMainMenu);
        }
    }

    private void RemoveButtonListeners()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(ShowLevelSelect);
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

        if (levelOneButton != null)
        {
            levelOneButton.onClick.RemoveListener(LoadLevelOne);
        }

        if (levelTwoButton != null)
        {
            levelTwoButton.onClick.RemoveListener(LoadLevelTwo);
        }

        if (levelThreeButton != null)
        {
            levelThreeButton.onClick.RemoveListener(LoadLevelThree);
        }

        if (levelSelectBackButton != null)
        {
            levelSelectBackButton.onClick.RemoveListener(ShowMainMenu);
        }
    }

    public static void RequestLevelSelectOnLoad()
    {
        openLevelSelectOnLoad = true;
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
        UpdateLevelButtons();

        SetPanelActive(titleBackground, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(levelSelectPanel, true);
    }

    private void UpdateLevelButtons()
    {
        bool levelOneUnlocked = LevelProgressManager.IsLevelUnlocked(1);
        bool levelTwoUnlocked = LevelProgressManager.IsLevelUnlocked(2);
        bool levelThreeUnlocked = LevelProgressManager.IsLevelUnlocked(3);

        SetLevelButtonState(
            levelOneButton,
            levelOneButtonText,
            levelOneUnlocked,
            "LEVEL 1"
        );

        SetLevelButtonState(
            levelTwoButton,
            levelTwoButtonText,
            levelTwoUnlocked,
            "LEVEL 2"
        );

        SetLevelButtonState(
            levelThreeButton,
            levelThreeButtonText,
            levelThreeUnlocked,
            "LEVEL 3"
        );
    }

    private static void SetLevelButtonState(
        Button levelButton,
        TMP_Text buttonText,
        bool isUnlocked,
        string levelName)
    {
        if (levelButton != null)
        {
            levelButton.interactable = isUnlocked;
        }

        if (buttonText == null)
        {
            return;
        }

        if (isUnlocked)
        {
            buttonText.text = levelName;
        }
        else
        {
            buttonText.text = $"{levelName}\n(LOCKED)";
        }
    }

    public void LoadLevelOne()
    {
        LoadLevel(levelOneSceneName, 1);
    }

    public void LoadLevelTwo()
    {
        LoadLevel(levelTwoSceneName, 2);
    }

    public void LoadLevelThree()
    {
        LoadLevel(levelThreeSceneName, 3);
    }

    private static void LoadLevel(
        string sceneName,
        int levelNumber)
    {
        if (!LevelProgressManager.IsLevelUnlocked(levelNumber))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
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