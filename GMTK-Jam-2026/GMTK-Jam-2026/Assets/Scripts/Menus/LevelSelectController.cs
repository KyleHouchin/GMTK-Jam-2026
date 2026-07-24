using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button initialLevelButton;
    [SerializeField] private Button backButton;

    [Header("Scenes")]
    [SerializeField] private string initialLevelSceneName = "InitialLevel";
    [SerializeField] private string titleSceneName = "TitleScreen";

    private void OnEnable()
    {
        if (initialLevelButton != null)
        {
            initialLevelButton.onClick.AddListener(
                LoadInitialLevel
            );
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(
                ReturnToTitle
            );
        }
    }

    private void OnDisable()
    {
        if (initialLevelButton != null)
        {
            initialLevelButton.onClick.RemoveListener(
                LoadInitialLevel
            );
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                ReturnToTitle
            );
        }
    }

    public void LoadInitialLevel()
    {
        SceneManager.LoadScene(initialLevelSceneName);
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}