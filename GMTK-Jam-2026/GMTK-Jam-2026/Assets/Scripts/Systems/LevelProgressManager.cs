public static class LevelProgressManager
{
    private const int FirstLevelNumber = 1;
    private const int FinalLevelNumber = 3;

    private static int highestUnlockedLevel =
        FirstLevelNumber;

    public static int GetHighestUnlockedLevel()
    {
        return highestUnlockedLevel;
    }

    public static bool IsLevelUnlocked(
        int levelNumber)
    {
        return levelNumber <=
               highestUnlockedLevel;
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

        if (nextLevelNumber <=
            highestUnlockedLevel)
        {
            return;
        }

        highestUnlockedLevel =
            nextLevelNumber;
    }

    public static void ResetProgress()
    {
        highestUnlockedLevel =
            FirstLevelNumber;
    }
}