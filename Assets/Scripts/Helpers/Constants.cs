/// <summary>Contains all configurable values and magic numbers used throughout the game.</summary>
public static class Constants
{
    // ---------- Player & Skills ----------
    public const float PlayerJumpForce = 4.5f;
    public const float PlayerGravity = 12;
    public const float WhooshLifespan = 0.3f;
    public const float WhooshSpeed = 3;

    public const int Skill1Level1CoinDuplicationChance = 30, Skill1Level2CoinDuplicationChance = 45, Skill1Level3CoinDuplicationChance = 60;
    public const float Skill2Level0Cooldown = 12, Skill2Level1Cooldown = 10, Skill2Level2Cooldown = 8, Skill2Level3Cooldown = 6;
    public const float Skill2InvulnerabilityDuration = 2;

    // ---------- Buying Costs & Maximum Type Counts ----------
    public const int BirdUnlockCost = 100;
    public const int BackgroundUnlockCost = 100;
    public const int ObstacleUnlockCost = 100;
    public const int SkillUnlockCost = 150;

    public const int MaxBirdTypes = 8;
    public const int MaxBackgroundTypes = 8;
    public const int MaxObstacleTypes = 6;

    // ---------- Birds, Obstacles and Coins ----------
    public const float BirdSpawnDelay = 5;
    public const float BirdChatDelay = 4;
    public const int BirdSpawnChance = 70;
    public const int BirdMoveRightChance = 65;
    public const int BirdChatChance = 65;

    public const float SlowObstaclesAndCoinsSpawnDelay = 1.5f;
    public const float MediumObstaclesAndCoinsSpawnDelay = 1.25f;
    public const float FastObstaclesAndCoinsSpawnDelay = 1;
    
    public const float BirdSpawnX = 6;
    public const float BirdSpawnY = 1.5f;
    public const float ObstacleSpawnX = 6;
    public const float ObstacleSpawnY = 1;
    public const float CoinSpawnX = 8;
    public const float CoinSpawnY = 2;

    public const float SlowBirdSpeed = -5, MediumBirdSpeed = -6, FastBirdSpeed = -7;
    public const float SlowCoinSpeed = -3, MediumCoinSpeed = -4, FastCoinSpeed = -5;
    public const float SlowObstacleSpeed = -3, MediumObstacleSpeed = -4, FastObstacleSpeed = -5;
    public const float SlowObstaclePingPongSpeed = 0.3f, MediumObstaclePingPongSpeed = 0.45f, FastObstaclePingPongSpeed = 0.6f;

    // ---------- Score gain ----------
    public const float ScoreGainInterval = 1;
    public const int CoinScoreIncrement = 2;
    public const int SlowScoreIncrement = 2, MediumScoreIncrement = 3, FastScoreIncrement = 4;

    // ---------- Other things ----------
    public const float DayNightCycleInterval = 0.2f;
    public const float FPSHudUpdateInterval = 1;
}