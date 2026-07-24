using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunManager : MonoBehaviour
{
    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private LifeForceAltarManager altarManager;
    [SerializeField] private Button startRunButton;

    [Header("Gameplay HUD")]
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private LifeForceTimer lifeForceTimer;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryLifeForceText;
    [SerializeField] private Button victoryRetryButton;
    [SerializeField] private Button levelSelectButton;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLoadout playerLoadout;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "TitleScreen";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";

    [Header("Run State")]
    [SerializeField] private bool runHasStarted;
    [SerializeField] private bool gameIsOver;
    [SerializeField] private bool runWasCompleted;

    public bool RunHasStarted => runHasStarted;
    public bool GameIsOver => gameIsOver;
    public bool RunWasCompleted => runWasCompleted;

    private void Awake()
    {
        PrepareShop();
    }

    private void OnEnable()
    {
        if (startRunButton != null)
        {
            startRunButton.onClick.AddListener(StartRun);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryLevel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(ReturnToTitle);
        }

        if (victoryRetryButton != null)
        {
            victoryRetryButton.onClick.AddListener(RetryLevel);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.AddListener(
                ReturnToLevelSelect
            );
        }

        if (lifeForceTimer != null)
        {
            lifeForceTimer.LifeForceDepleted +=
                HandleLifeForceDepleted;
        }
    }

    private void OnDisable()
    {
        if (startRunButton != null)
        {
            startRunButton.onClick.RemoveListener(StartRun);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryLevel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(ReturnToTitle);
        }

        if (victoryRetryButton != null)
        {
            victoryRetryButton.onClick.RemoveListener(RetryLevel);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(
                ReturnToLevelSelect
            );
        }

        if (lifeForceTimer != null)
        {
            lifeForceTimer.LifeForceDepleted -=
                HandleLifeForceDepleted;
        }
    }

    public void StartRun()
    {
        if (runHasStarted || gameIsOver || runWasCompleted)
        {
            return;
        }

        if (altarManager == null ||
            playerLoadout == null ||
            lifeForceTimer == null)
        {
            Debug.LogError(
                "RunManager is missing a required reference.",
                this
            );

            return;
        }

        SaveSelectedLoadout();

        runHasStarted = true;
        gameIsOver = false;
        runWasCompleted = false;

        SetPanelActive(shopPanel, false);
        SetPanelActive(gameplayHUD, true);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(victoryPanel, false);

        SetPlayerGameplayEnabled(true);

        lifeForceTimer.BeginCountdown(
            playerLoadout.StartingLifeForce
        );
    }

    public void CompleteRun()
    {
        if (!runHasStarted || gameIsOver || runWasCompleted)
        {
            return;
        }

        runHasStarted = false;
        runWasCompleted = true;

        if (lifeForceTimer != null)
        {
            lifeForceTimer.StopCountdown();
        }

        SetPlayerGameplayEnabled(false);

        SetPanelActive(shopPanel, false);
        SetPanelActive(gameplayHUD, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(victoryPanel, true);

        if (victoryLifeForceText != null &&
            lifeForceTimer != null)
        {
            victoryLifeForceText.text =
                $"LIFE FORCE REMAINING: " +
                $"{lifeForceTimer.CurrentLifeForce:0.0}";
        }
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void ReturnToLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }

    private void PrepareShop()
    {
        runHasStarted = false;
        gameIsOver = false;
        runWasCompleted = false;

        SetPanelActive(shopPanel, true);
        SetPanelActive(gameplayHUD, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(victoryPanel, false);

        SetPlayerGameplayEnabled(false);

        if (playerLoadout != null)
        {
            playerLoadout.ClearLoadout();
        }

        if (lifeForceTimer != null)
        {
            lifeForceTimer.StopCountdown();
        }
    }

    private void SaveSelectedLoadout()
    {
        bool batRushSelected =
            altarManager.IsAbilitySelected("Dash");

        bool wingedLeapSelected =
            altarManager.IsAbilitySelected("DoubleJump");

        bool bloodShotSelected =
            altarManager.IsAbilitySelected("BloodShot");

        playerLoadout.ConfigureLoadout(
            altarManager.RemainingLifeForce,
            batRushSelected,
            wingedLeapSelected,
            bloodShotSelected
        );
    }

    private void HandleLifeForceDepleted()
    {
        if (!runHasStarted || gameIsOver || runWasCompleted)
        {
            return;
        }

        runHasStarted = false;
        gameIsOver = true;

        if (lifeForceTimer != null)
        {
            lifeForceTimer.StopCountdown();
        }

        SetPlayerGameplayEnabled(false);

        SetPanelActive(shopPanel, false);
        SetPanelActive(gameplayHUD, false);
        SetPanelActive(victoryPanel, false);
        SetPanelActive(gameOverPanel, true);
    }

    private void SetPlayerGameplayEnabled(bool enabled)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        if (playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.linearVelocity = Vector2.zero;
        playerRigidbody.simulated = enabled;
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