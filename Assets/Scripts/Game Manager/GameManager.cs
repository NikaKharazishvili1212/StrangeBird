using UnityEngine;
using UnityEngine.UI;
using TMPro;

using VInspector;
using static Utils;
using static Constants;

// Partial class for managing core gameplay initialization, menu interactions, state orchestration, and game data persistence
public sealed partial class GameManager : MonoBehaviour
{
    [Tab("Main")]
    [SerializeField] Player player;
    [SerializeField] GameObject menu, loadingMenu;
    [SerializeField] Image loadingBar;
    [SerializeField] TextMeshProUGUI loadingText, coinText, scoreText, deathText, fpsText;
    [SerializeField] SpriteRenderer background;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip newHighScoreSound, uiSelectSound;
    int currentScore = 0, coinsCollectedThisRound = 0;
    bool newHighScore = false;

    [Tab("LoadAndSave")]
    public int difficulty { get; private set; }  // Public getter for Bird class to access difficulty level
    [SerializeField] int highScore, coin, totalDeaths, skill1Level;
    [SerializeField] float obstaclesSpawnDelay;
    [SerializeField] bool showFpsOption, spawnBirdsOption;

    void Start()
    {
        SubscribeToPlayerEvents();  // OnDeath, OnRespawn, OnCoinTake
        StartGameplayLoops();  // Initialize core gameplay loops (spawning Obstacles, Coins, Birds, Day/Night cycle, and score gain)
        if (showFpsOption) InvokeRepeating(nameof(UpdateFpsHud), 0, 1f);  // Update Fps hud if option is enabled
    }

    enum MenuAction { Play = 0, Menu = 1, Quit = 2 }

    // Handles menu actions (Play, Menu, Quit)
    public void MenuSelection(int index)
    {
        audioSource.PlayOneShot(uiSelectSound);

        switch ((MenuAction)index)
        {
            case MenuAction.Play:
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

    void StartGameplayLoops()
    {
        if (spawnBirdsOption) InvokeRepeating(nameof(BirdsPool), 5f, 5f);  // Spawn birds as well if option is enabled
        InvokeRepeating(nameof(ObstaclesPool), obstaclesSpawnDelay, obstaclesSpawnDelay);
        // Even though Coins spawn with the same delay as Obstacles, they are positioned to the right of Obstacles to avoid overlap
        InvokeRepeating(nameof(CoinsPool), obstaclesSpawnDelay, obstaclesSpawnDelay);
        InvokeRepeating(nameof(DayNightCycle), DayNightCycleInterval, DayNightCycleInterval);  // Affects background color, sun/moon icon rotation and timer
        InvokeRepeating(nameof(GainScore), ScoreGainInterval, ScoreGainInterval);  // Increase score over time based on difficulty
    }

    void StopGameplayLoops()
    {
        if (spawnBirdsOption) CancelInvoke(nameof(BirdsPool));
        CancelInvoke(nameof(ObstaclesPool));
        CancelInvoke(nameof(CoinsPool));
        CancelInvoke(nameof(DayNightCycle));
        CancelInvoke(nameof(GainScore));
    }

    #region Save/Load
    void Awake() => LoadStats();
    void OnApplicationQuit() => SaveStats();

    void LoadStats()
    {
        // Load high score and total deaths, update UI
        highScore = PlayerPrefs.GetInt("HighestScore", 100);
        scoreText.text = currentScore + " / " + highScore;
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths");

        // Load coins and skill level, update UI
        coin = PlayerPrefs.GetInt("Coin");
        coinText.text = coin.ToString();
        skill1Level = PlayerPrefs.GetInt("Skill1Level");

        // Load general settings (volume, game difficulty, fps-showing option, bird-spawning option)
        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
        difficulty = PlayerPrefs.GetInt("Difficulty");
        showFpsOption = PlayerPrefs.GetInt("ShowFps") == 1;
        fpsText.gameObject.SetActive(showFpsOption);
        spawnBirdsOption = PlayerPrefs.GetInt("SpawnBirds") == 1;

        // Load values based on difficulty
        obstaclesSpawnDelay = difficulty == 0 ? EasyObstacleSpawnDelay : difficulty == 1 ? MediumObstacleSpawnDelay : HardObstacleSpawnDelay;
        Obstacle.SetSpeed(difficulty);  // Set static speed for all obstacles
        Coin.SetSpeed(difficulty);  // Set static speed for all coins
    }

    void SaveStats()
    {
        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.SetInt("HighestScore", highScore);
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths);
        PlayerPrefs.Save();
    }
    #endregion
}