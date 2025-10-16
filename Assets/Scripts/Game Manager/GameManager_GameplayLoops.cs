using UnityEngine;
using TMPro;

using VInspector;
using static Utils;
using static Constants;

// Partial class for managing gameplay loops executed via InvokeRepeating
public sealed partial class GameManager : MonoBehaviour
{
    [Tab("ObjectsToPool")]
    [SerializeField] Movable[] birdsToPool, obstaclesToPool, coinsToPool;

    [Tab("DayNightCycle")]
    [SerializeField] Transform sunMoonIcon;
    [SerializeField] TextMeshProUGUI timerText;
    int minutes = 0, hours = 12;

    void UpdateFpsHud() => fpsText.text = "Fps: " + Mathf.RoundToInt(1 / Time.deltaTime).ToString();

    // Spawn a bird from the pool with a 40% chance
    void BirdsPool() { if (PercentChanceSuccess(BirdSpawnChance)) PoolObject(birdsToPool); }

    // Spawn an obstacle from the pool
    void ObstaclesPool() => PoolObject(obstaclesToPool);

    // Spawn a coin from the pool based on skill level and random chance
    void CoinsPool()
    {
        // Spawn coin based on skill level
        if (skill1Level == 1 && !PercentChanceSuccess(Skill1Level1CoinChance)) return;
        if (skill1Level == 2 && !PercentChanceSuccess(Skill1Level2CoinChance)) return;

        PoolObject(coinsToPool);
    }

    // Handle day/night cycle: update timer, background color, and sun/moon rotation
    void DayNightCycle()
    {
        // Timer
        minutes = (minutes + 1) % 60;
        hours = minutes == 0 ? (hours + 1) % 24 : hours;
        timerText.text = $"{hours:00}:{minutes:00}";

        float timeOfDay = (hours + (minutes / 60f)) / 24f;  // Calculate time as a value between 0 and 1 (0 is midnight, 0.5 is noon, 1 is midnight)
        float adjustedTimeOfDay = (timeOfDay + 0.5f) % 1f;  // Adjust timeOfDay to ensure 12:00 is the peak daylight

        Color dayColor = new Color(1f, 1f, 1f);  // White (max light)
        Color nightColor = new Color(0f, 0f, 0f);  // Black (no light)

        if (adjustedTimeOfDay < 0.5f) background.color = Color.Lerp(dayColor, nightColor, adjustedTimeOfDay * 2);  // Morning to afternoon (dayColor to nightColor)
        else background.color = Color.Lerp(nightColor, dayColor, (adjustedTimeOfDay - 0.5f) * 2);  // Afternoon to morning (nightColor to dayColor)

        // Rotate the sun & moon icon based on time of day
        float rotationAngle = (timeOfDay * 360f + 180f) % 360f;  // Calculate the rotation angle starting from 180 degrees
        sunMoonIcon.localRotation = Quaternion.Euler(0, 0, rotationAngle);  // Apply the rotation to the RectTransform
    }

    // Increment score over time based on difficulty
    void GainScore()
    {
        currentScore += difficulty == 0 ? EasyScoreIncrement : difficulty == 1 ? MediumScoreIncrement : HardScoreIncrement;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            if (!newHighScore)
            {
                audioSource.PlayOneShot(newHighScoreSound);
                newHighScore = true;
            }
        }

        scoreText.text = currentScore + " / " + highScore;
    }
}