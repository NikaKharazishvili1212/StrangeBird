// Contains all configurable values and magic numbers used throughout the game
public static class Constants
{
    // ---------- Maximum counts ----------
    public const int MaxBirdTypes = 8, MaxBackgroundTypes = 8, MaxObstacleTypes = 6;

    // ---------- Buying Costs ----------
    public const int BirdUnlockCost = 50, BackgroundUnlockCost = 50, ObstacleUnlockCost = 50, SkillUnlockCost = 100;

    // ---------- Probabilities (percent) ----------
    public const int BirdSpawnChance = 40, BirdMoveRightChance = 70, BirdChatChance = 30;
    public const int Skill1Level1CoinChance = 50, Skill1Level2CoinChance = 70;

    // ---------- Spawn delays ----------
    public const float EasyObstacleSpawnDelay = 1.5f, MediumObstacleSpawnDelay = 1.25f, HardObstacleSpawnDelay = 1f;

    // ---------- Score gain per tick by difficulty ----------
    public const float ScoreGainInterval = 1f;
    public const int EasyScoreIncrement = 2, MediumScoreIncrement = 3, HardScoreIncrement = 4;

    // ---------- Movement speeds by difficulty ----------
    public const float EasyBirdSpeed = -5f, MediumBirdSpeed = -6f, HardBirdSpeed = -7f;
    public const float EasyObstacleSpeed = -3f, MediumObstacleSpeed = -4f, HardObstacleSpeed = -5f;
    public const float EasyCoinSpeed = -3f, MediumCoinSpeed = -4f, HardCoinSpeed = -5f;


    public const float DayNightCycleInterval = 0.2f;
}
