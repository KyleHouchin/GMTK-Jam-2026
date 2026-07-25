using UnityEngine;

public static class LevelProgressManager
{
    private const string HighestUnlockedLevelKey =
        "HighestUnlockedLevel";

    private const int FirstLevelNumber = 1;
    private const int FinalLevelNumber = 3;

    public static int GetHighestUnlockedLevel()
    {
        return PlayerPrefs.GetInt(
            HighestUnlockedLevelKey,
            FirstLevelNumber
        );
    }

    public static bool IsLevelUnlocked(
        int levelNumber)
    {
        return levelNumber <=
               GetHighestUnlockedLevel();
    }

    public static void CompleteLevel(
        int completedLevelNumber)
    {
        int nextLevelNumber =
            completedLevelNumber + 1;

        if (nextLevelNumber >
            FinalLevelNumber)
        {
            nextLevelNumber =
                FinalLevelNumber;
        }

        int currentHighestUnlockedLevel =
            GetHighestUnlockedLevel();

        if (nextLevelNumber <=
            currentHighestUnlockedLevel)
        {
            return;
        }

        PlayerPrefs.SetInt(
            HighestUnlockedLevelKey,
            nextLevelNumber
        );

        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(
            HighestUnlockedLevelKey,
            FirstLevelNumber
        );

        PlayerPrefs.Save();
    }
}