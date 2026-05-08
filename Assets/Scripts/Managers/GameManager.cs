using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;
using System.Collections;
using static Utils;
using static Constants;

/// <summary>Class for managing core gameplay.</summary>
[DefaultExecutionOrder(-100)] // Ensures GameManager's awake initialization runs first
public sealed partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [field: SerializeField] public Player player { get; private set; }
    [SerializeField] SpriteAtlas spriteAtlas;

    // Canvas
    [SerializeField] Image loadingBar;
    [SerializeField] GameObject menu, loadingMenu;
    [SerializeField] TextMeshProUGUI loadingText, coinText, scoreText, deathText, fpsText;
    [SerializeField] Image background;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip uiSelectSound;
    int currentScore, coinsCollectedThisRound;
    enum MenuAction { Play = 0, Menu = 1, Quit = 2 }

    // Objects to pool
    [SerializeField] Movable[] birdsToPool, obstaclesToPool, coinsToPool;

    // DayNightCycle
    [SerializeField] Transform sunMoonIcon;
    [SerializeField] TextMeshProUGUI timerText;
    int minutes = 0, hours = 12;

    // Load and save
    int gameSpeed, highScore, coin, totalDeaths, skill1GreedLevel;
    bool showFpsOption, spawnBirdsOption;

    Coroutine flapRespawnCoroutine;

    public int GetGameSpeed() => gameSpeed;
    public SpriteAtlas GetSpriteAtlas() => spriteAtlas;

    void Awake() => Instance = this;

    void Start()
    {
        LoadStats();
        player.OnCoinTake += TakeCoin;
        player.OnDeath += StopAllObjects;
        player.OnRespawn += Restart;
        background.sprite = spriteAtlas.GetSprite("Background" + PlayerPrefs.GetInt("BackgroundSelected", 0));
        StartGameplayLoops();
        if (showFpsOption) InvokeRepeating(nameof(UpdateFpsHud), 0, FPSHudUpdateInterval);
    }

    #region InvokeRepeatings
    // Start all InvokeRepeating gameplay loops
    void StartGameplayLoops()
    {
        if (spawnBirdsOption) InvokeRepeating(nameof(BirdsPool), BirdSpawnDelay, BirdSpawnDelay);
        float obstaclesAndCoinsSpawnDelay = gameSpeed == 0 ? SlowObstaclesAndCoinsSpawnDelay : gameSpeed == 1 ? MediumObstaclesAndCoinsSpawnDelay : FastObstaclesAndCoinsSpawnDelay;
        InvokeRepeating(nameof(ObstaclesAndCoinsPool), obstaclesAndCoinsSpawnDelay, obstaclesAndCoinsSpawnDelay);
        InvokeRepeating(nameof(DayNightCycle), DayNightCycleInterval, DayNightCycleInterval); // Affects background color, sun/moon icon rotation and timer
        InvokeRepeating(nameof(GainScore), ScoreGainInterval, ScoreGainInterval); // Increase score over time based on game speed
    }

    // Cancel all active InvokeRepeating gameplay loops
    void StopGameplayLoops()
    {
        if (spawnBirdsOption) CancelInvoke(nameof(BirdsPool));
        CancelInvoke(nameof(ObstaclesAndCoinsPool));
        CancelInvoke(nameof(DayNightCycle));
        CancelInvoke(nameof(GainScore));
    }

    void UpdateFpsHud() => fpsText.text = "Fps: " + Mathf.RoundToInt(1 / Time.deltaTime).ToString();

    void BirdsPool() { if (PercentChance(BirdSpawnChance)) PoolComponent(birdsToPool)?.Activate(); }

    void ObstaclesAndCoinsPool()
    {
        PoolComponent(obstaclesToPool)?.Activate();
        PoolComponent(coinsToPool)?.Activate();
    }

    // Update clock, background color, and sun/moon rotation based on time of day
    void DayNightCycle()
    {
        // Timer
        minutes = (minutes + 1) % 60;
        hours = minutes == 0 ? (hours + 1) % 24 : hours;
        timerText.text = $"{hours:00}:{minutes:00}";

        float timeOfDay = (hours + (minutes / 60f)) / 24f; // Calculate time as a value between 0 and 1 (0 is midnight, 0.5 is noon, 1 is midnight)
        float adjustedTimeOfDay = (timeOfDay + 0.5f) % 1f; // Adjust timeOfDay to ensure 12:00 is the peak daylight

        Color dayColor = new Color(1, 1, 1); // White (max light)
        Color nightColor = new Color(0, 0, 0); // Black (no light)

        if (adjustedTimeOfDay < 0.5f) background.color = Color.Lerp(dayColor, nightColor, adjustedTimeOfDay * 2);  // Morning to afternoon (dayColor to nightColor)
        else background.color = Color.Lerp(nightColor, dayColor, (adjustedTimeOfDay - 0.5f) * 2);  // Afternoon to morning (nightColor to dayColor)

        // Rotate the sun & moon icon based on time of day
        float rotationAngle = (timeOfDay * 360f + 180f) % 360f; // Calculate the rotation angle starting from 180 degrees
        sunMoonIcon.localRotation = Quaternion.Euler(0, 0, rotationAngle); // Apply the rotation to the RectTransform
    }

    // Increment score each tick based on gameSpeed
    void GainScore()
    {
        currentScore += gameSpeed == 0 ? SlowScoreIncrement : gameSpeed == 1 ? MediumScoreIncrement : FastScoreIncrement;
        UpdateScore();
    }

    // Update current and high score
    void UpdateScore()
    {
        if (currentScore > highScore) highScore = currentScore;
        scoreText.text = currentScore + " / " + highScore;
    }
    #endregion


    #region Events
    // Increments coin count and updates UI (called when Player collects a coin)
    void TakeCoin()
    {
        int coinsEarned = skill1GreedLevel == 0 ? 1 : PercentChance(skill1GreedLevel == 1 ? Skill1Level1CoinDuplicationChance : skill1GreedLevel == 2 ? Skill1Level2CoinDuplicationChance : Skill1Level3CoinDuplicationChance) ? 2 : 1;
        coin += coinsEarned;
        coinsCollectedThisRound += coinsEarned;
        coinText.text = coin.ToString();
        currentScore += CoinScoreIncrement;
        UpdateScore();
    }

    // Stops coin and obstacle movement, cancels spawns, and updates death stats (called on Player death)
    void StopAllObjects()
    {
        foreach (Movable coin in coinsToPool) coin.Stop();
        foreach (Movable obstacle in obstaclesToPool) obstacle.Stop();
        if (spawnBirdsOption) foreach (Bird bird in birdsToPool) bird.FlyAwayAfterPlayerDeath();

        StopGameplayLoops();

        totalDeaths += 1;
        deathText.text = $"Total Deaths: {totalDeaths}\nHighest Score: {highScore}\nCoins Collected This Round: {coinsCollectedThisRound}";
        coinsCollectedThisRound = 0;
        this.Wait(0.5f, () =>
        {
            menu.SetActive(true);
            flapRespawnCoroutine = StartCoroutine(FlapRespawnRoutine());
        });
    }

    // Waits for flap input after death, then restarts the game
    IEnumerator FlapRespawnRoutine()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(player.flapKey));
        MenuSelection((int)MenuAction.Play);
    }

    // Disables all pooled objects, restarts gameplay loops, resets score and updates UI (called on Player respawn)
    void Restart()
    {
        foreach (Movable coin in coinsToPool) coin.gameObject.SetActive(false);
        foreach (Movable obstacle in obstaclesToPool) obstacle.gameObject.SetActive(false);
        foreach (Movable bird in birdsToPool) bird.gameObject.SetActive(false);

        StartGameplayLoops();

        currentScore = 0;
        scoreText.text = currentScore + " / " + highScore;
    }

    // Handle main menu button presses (Play, Menu, Quit). The menu is enabled on Player death
    public void MenuSelection(int index)
    {
        audioSource.PlayOneShot(uiSelectSound);

        switch ((MenuAction)index)
        {
            case MenuAction.Play:
                if (flapRespawnCoroutine != null) StopCoroutine(flapRespawnCoroutine);
                player.Respawn();
                menu.SetActive(false);
                break;
            case MenuAction.Menu:
                SaveStats();
                foreach (Transform child in menu.transform) child.gameObject.SetActive(false);
                loadingMenu.SetActive(true);
                StartCoroutine(LoadSceneAsync("Menu", loadingBar, loadingText));
                break;
            case MenuAction.Quit:
                QuitApplication();
                break;
        }
    }
    #endregion


    #region Load & Save
    void LoadStats()
    {
        // Load high score and total deaths
        highScore = PlayerPrefs.GetInt("HighestScore", 100);
        scoreText.text = currentScore + " / " + highScore;
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths");

        // Load coins, skill level, background
        coin = PlayerPrefs.GetInt("Coin");
        coinText.text = coin.ToString();
        skill1GreedLevel = PlayerPrefs.GetInt("Skill1GreedLevel");
        background.sprite = spriteAtlas.GetSprite("Background" + PlayerPrefs.GetInt("BackgroundSelected", 0));

        // Load general settings (volume, game speed, fps-showing, bird-spawning)
        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
        gameSpeed = PlayerPrefs.GetInt("Game Speed");
        showFpsOption = PlayerPrefs.GetInt("ShowFps") == 1;
        fpsText.gameObject.SetActive(showFpsOption);
        spawnBirdsOption = PlayerPrefs.GetInt("SpawnBirds") == 1;

        // Load values based on game speed
        Obstacle.SetSpeed(gameSpeed); // Set static speed for all obstacles
        Coin.SetSpeed(gameSpeed); // Set static speed for all coins
    }

    void OnApplicationQuit() => SaveStats();

    void SaveStats()
    {
        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.SetInt("HighestScore", highScore);
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths);
        PlayerPrefs.Save();
    }
    #endregion
}