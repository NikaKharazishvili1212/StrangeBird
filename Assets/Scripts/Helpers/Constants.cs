using UnityEngine;

/// <summary>Contains all configurable values and magic numbers used throughout the game.</summary>
public static class Constants
{
    // ---------- Player & Skills ----------
    public const float PlayerJumpForce = 4.5f;
    public const float PlayerGravity = 12;

    public const int Skill1Level1CoinDuplicationChance = 30, Skill1Level2CoinDuplicationChance = 45, Skill1Level3CoinDuplicationChance = 60;
    public const float Skill2Level0Cooldown = 9, Skill2Level1Cooldown = 8, Skill2Level2Cooldown = 7, Skill2Level3Cooldown = 6;
    public const float Skill2InvulnerabilityDuration = 2;

    // ---------- Buying Costs & Maximum Type Counts ----------
    public const int BirdUnlockCost = 60;
    public const int BackgroundUnlockCost = 60;
    public const int ObstacleUnlockCost = 60;
    public const int SkillUnlockCost = 120;

    public const int MaxBirdTypes = 8;
    public const int MaxBackgroundTypes = 8;
    public const int MaxObstacleTypes = 6;

    // ---------- Talking Birds Chances ----------
    public const int BirdSpawnChance = 40;
    public const int BirdMoveRightChance = 70;
    public const int BirdChatChance = 30;

    // ---------- Spawn Delays & Positions ----------
    public const float EasyObstaclesAndCoinsSpawnDelay = 1.5f, MediumObstaclesAndCoinsSpawnDelay = 1.25f, HardObstaclesAndCoinsSpawnDelay = 1;
    public const float BirdsSpawnDelay = 5;

    public const float BirdSpawnX = 6;
    public const float ObstacleSpawnX = 6;
    public const float CoinSpawnX = 8;
    public const float SpawnY = 1.5f;

    // ---------- Movement Speeds By Difficulty ----------
    public const float EasyBirdSpeed = -5, MediumBirdSpeed = -6, HardBirdSpeed = -7;
    public const float EasyCoinSpeed = -3, MediumCoinSpeed = -4, HardCoinSpeed = -5;
    public const float EasyObstacleSpeed = -3, MediumObstacleSpeed = -4, HardObstacleSpeed = -5;
    public const float EasyObstaclePingPongSpeed = 0.3f, MediumObstaclePingPongSpeed = 0.45f, HardObstaclePingPongSpeed = 0.6f;
    public const float ObstaclePingPongYMax = 1.3f, ObstaclePingPongYMin = -1.3f;

    // ---------- Score gain ----------
    public const float ScoreGainInterval = 1;
    public const int CoinScoreIncrement = 2;
    public const int EasyScoreIncrement = 2, MediumScoreIncrement = 3, HardScoreIncrement = 4;

    // ---------- Other things ----------
    public const float WhooshLifespan = 0.3f;
    public const float WhooshSpeed = 2;
    public const float DayNightCycleInterval = 0.2f;
}