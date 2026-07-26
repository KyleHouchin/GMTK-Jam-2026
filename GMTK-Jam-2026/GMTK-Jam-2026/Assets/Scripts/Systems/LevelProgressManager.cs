public static class LevelProgressManager
{
    private const int FirstLevelNumber = 1;

    public static int GetHighestUnlockedLevel()
    {
        return FirstLevelNumber;
    }

    public static bool IsLevelUnlocked(
        int levelNumber)
    {
        return levelNumber ==
               FirstLevelNumber;
    }

    public static void CompleteLevel(
        int completedLevelNumber)
    {
        // Level progression is currently disabled.
        // Completing Level 1 does not unlock Level 2.
    }

    public static void ResetProgress()
    {
        // Progress always remains at Level 1.
    }
}