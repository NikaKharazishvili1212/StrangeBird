using UnityEngine;
using UnityEngine.UI;
using TMPro;

using VInspector;
using static Utils;

// Partial class for managing the main menu, including navigation and scene loading and game data persistence
public sealed partial class MenuManager : MonoBehaviour
{
    [Tab("Main Menu")]
    [SerializeField] GameObject menu, loadingMenu, shopMenu, optionMenu, aboutText, back;
    [SerializeField] Image loadingBar, background;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip cantDoThatSound;

    enum MenuType : byte { Play = 0, Shop = 1, Options = 2, About = 3, Quit = 4, Back = 5 }

    public void MenuSelection(int index)
    {
        switch ((MenuType)index)
        {
            case MenuType.Play: Play(); break;
            case MenuType.Shop: ShowSubMenu(shopMenu); break;
            case MenuType.Options: ShowSubMenu(optionMenu); break;
            case MenuType.About: ShowSubMenu(aboutText); break;
            case MenuType.Quit: QuitApplication(); break;
            case MenuType.Back: ResetMainMenu(); break;
        }
    }

    void Play()
    {
        SaveStats();
        foreach (Transform child in menu.transform) child.gameObject.SetActive(false);
        loadingMenu.SetActive(true);
        back.SetActive(false);
        StartCoroutine(LoadSceneAsync("Game", loadingBar, loadingText));
    }

    void ShowSubMenu(GameObject submenu)
    {
        background.color = new Color(0.2f, 0.2f, 0.2f);
        menu.SetActive(false);
        submenu.SetActive(true);
        back.SetActive(true);
    }

    void ResetMainMenu()
    {
        background.color = Color.white;
        back.SetActive(false);
        menu.SetActive(true);
        shopMenu.SetActive(false);
        shopCosmetics.SetActive(false);
        shopSkills.SetActive(false);
        optionMenu.SetActive(false);
        aboutText.SetActive(false);
        ResetKey();
    }

    // Development tools - NOT for production build
#if UNITY_EDITOR
    [Button]
    void ResetGameProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LoadStats();
        Debug.Log("Game progress reset! All settings, coins, and unlocks cleared.");
    }

    [Button]
    void Add100Coin()
    {
        coin += 100;
        coinText.text = coin.ToString();
        Debug.Log($"Added 100 coins! Total: {coin}");
    }
#endif

    #region Save/Load
    void Awake() => LoadStats();
    void OnApplicationQuit() => SaveStats();

    void SaveStats()
    {
        // Save selected cosmetics
        PlayerPrefs.SetInt("BirdSelected", birdSelected);
        PlayerPrefs.SetInt("BackgroundSelected", backgroundSelected);
        PlayerPrefs.SetInt("ObstacleSelected", obstacleSelected);

        // Save bought cosmetics and skills
        SaveBoolArray("BirdsBought", birdsBought);
        SaveBoolArray("BackgroundsBought", backgroundsBought);
        SaveBoolArray("ObstaclesBought", obstaclesBought);
        PlayerPrefs.SetInt("Skill1Level", skill1Level);
        PlayerPrefs.SetInt("Skill2Level", skill2Level);

        // Save options
        PlayerPrefs.SetInt("Difficulty", difficulty);
        PlayerPrefs.SetFloat("GlobalVolume", AudioListener.volume);
        PlayerPrefs.SetInt("SpawnBirds", spawnBirds);
        PlayerPrefs.SetInt("ShowFps", showFps);
        PlayerPrefs.SetString("FlapKey", flapKey);

        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.Save();
    }

    void LoadStats()
    {
        // Load cosmetics. First style is always unlocked(and selected if no other selection) by default
        birdsBought[0] = true;
        backgroundsBought[0] = true;
        obstaclesBought[0] = true;
        birdSelected = PlayerPrefs.GetInt("BirdSelected");
        backgroundSelected = PlayerPrefs.GetInt("BackgroundSelected");
        obstacleSelected = PlayerPrefs.GetInt("ObstacleSelected");

        // Load bought cosmetics and skills
        LoadBoolArray("BirdsBought", birdsBought);
        LoadBoolArray("BackgroundsBought", backgroundsBought);
        LoadBoolArray("ObstaclesBought", obstaclesBought);
        skill1Level = PlayerPrefs.GetInt("Skill1Level");
        skill2Level = PlayerPrefs.GetInt("Skill2Level");

        // Load options
        difficulty = PlayerPrefs.GetInt("Difficulty");
        difficultyTexts[difficulty].color = Color.yellow;

        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
        soundsCheckmark.sprite = AudioListener.volume == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        spawnBirds = PlayerPrefs.GetInt("SpawnBirds", 1);
        birdsCheckmark.sprite = spawnBirds == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        showFps = PlayerPrefs.GetInt("ShowFps", 1);
        fpsText.gameObject.SetActive(showFps == 1);
        if (showFps == 1 && !IsInvoking(nameof(ShowFps))) InvokeRepeating(nameof(ShowFps), 0, 1f);  // Prevent duplicate invocations when using ResetGameProgress() button
        fpsCheckmark.sprite = showFps == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        flapKey = PlayerPrefs.GetString("FlapKey", "Space");
        flapKeyText.text = FormatKeyName(flapKey);

        // Load coins
        coin = PlayerPrefs.GetInt("Coin");
        coinText.text = coin.ToString();
    }

    // Helper to save a bool array as ints. Foreach starts from 1, first is always unlocked
    void SaveBoolArray(string keyPrefix, bool[] array) { for (int i = 1; i < array.Length; i++) PlayerPrefs.SetInt($"{keyPrefix}{i}", array[i] ? 1 : 0); }

    // Helper to load a bool array from PlayerPrefs. Foreach starts from 1, first is always unlocked
    void LoadBoolArray(string keyPrefix, bool[] array) { for (int i = 1; i < array.Length; i++) array[i] = PlayerPrefs.GetInt($"{keyPrefix}{i}", 0) == 1; }
    #endregion
}