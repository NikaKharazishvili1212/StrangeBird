using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nikspector;
using static Utils;

// Partial class for managing the main menu, including navigation and scene loading and game data persistence
public sealed partial class MenuManager : MonoBehaviour
{
    [Tab("Main Menu")]
    [SerializeField] GameObject menu, loadingMenu, shopMenu, optionMenu;
    [SerializeField] Image loadingBar, background;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip cantDoThatSound;
    
    enum MenuType : byte { Play = 0, Shop = 1, Options = 2, Quit = 3, BackToMain = 4, BackToShop = 5 }

    void Awake() => LoadStats();
    void OnApplicationQuit() => SaveStats();

    // Handle main menu button presses (Play, Shop, Options, About, Quit, Back)
    public void MenuSelection(int index)
    {
        switch ((MenuType)index)
        {
            case MenuType.Play: Play(); break;
            case MenuType.Shop: ShowSubMenu(shopMenu); break;
            case MenuType.Options: ShowSubMenu(optionMenu); UpdateGameSpeedTextColors(); break;
            case MenuType.Quit: QuitApplication(); break;
            case MenuType.BackToMain: BackToMain(); break;
            case MenuType.BackToShop: BackToShop(); break;
        }
    }

    // Save stats and load scene asynchronously with loading bar
    void Play()
    {
        SaveStats();
        foreach (Transform child in menu.transform) child.gameObject.SetActive(false);
        loadingMenu.SetActive(true);
        StartCoroutine(LoadSceneAsync("Game", loadingBar, loadingText));
    }

    // Dim background and show the requested submenu
    void ShowSubMenu(GameObject submenu)
    {
        background.color = new Color(0.2f, 0.2f, 0.2f);
        menu.SetActive(false);
        submenu.SetActive(true);
    }

    // Back to main menu from either shop or options menu
    void BackToMain()
    {
        background.color = Color.white;
        menu.SetActive(true);
        shopMenu.SetActive(false);
        optionMenu.SetActive(false);
        // Stop key detection and restore the formatted key name in case player exits without choosing
        StopCoroutine(nameof(DetectFlapKey));
        flapKeyText.text = flapKey.ToString().Replace("Alpha", "A").Replace("Keypad", "K").Replace("Left", "L").Replace("Right", "R").Replace("Back", "B");
    }

    // Back to shop menu from birds/backgrounds/obstacles/skills shop
    void BackToShop()
    {
        shopMenu.SetActive(true);
        shopCosmetics.SetActive(false);
        cosmeticBuyButton.SetActive(false);
        shopSkills.SetActive(false);
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
        for (int i = 1; i < birdsBought.Length; i++) birdsBought[i] = PlayerPrefs.GetInt($"BirdsBought{i}", 0) == 1;
        for (int i = 1; i < backgroundsBought.Length; i++) backgroundsBought[i] = PlayerPrefs.GetInt($"BackgroundsBought{i}", 0) == 1;
        for (int i = 1; i < obstaclesBought.Length; i++) obstaclesBought[i] = PlayerPrefs.GetInt($"ObstaclesBought{i}", 0) == 1;
        skill1GreedLevel = PlayerPrefs.GetInt("Skill1GreedLevel");
        skill2ShieldLevel = PlayerPrefs.GetInt("Skill2ShieldLevel");

        // Load options
        gameSpeed = PlayerPrefs.GetInt("Game Speed");
        gameSpeedTexts[gameSpeed].color = Color.yellow;

        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
        soundsCheckmark.sprite = AudioListener.volume == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        spawnBirds = PlayerPrefs.GetInt("SpawnBirds", 1);
        birdsCheckmark.sprite = spawnBirds == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        showFps = PlayerPrefs.GetInt("ShowFps", 0);
        fpsText.gameObject.SetActive(showFps == 1);
        if (showFps == 1 && !IsInvoking(nameof(ShowFps))) InvokeRepeating(nameof(ShowFps), 0, 1f);  // Prevent duplicate invocations when using ResetGameProgress() button
        fpsCheckmark.sprite = showFps == 1 ? spriteAtlas.GetSprite("Checkmark_Enabled") : spriteAtlas.GetSprite("Checkmark_Disabled");

        flapKey = PlayerPrefs.GetString("FlapKey", "Space");
        // Formats key names for display: Alpha1 -> A1, Keypad1 -> K1, LeftShift -> LShift, RightShift -> RShift, BackQuote -> BQuote, BackSlash -> BSlash, Backspace -> Bspace
        flapKeyText.text = flapKey.ToString().Replace("Alpha", "A").Replace("Keypad", "K").Replace("Left", "L").Replace("Right", "R").Replace("Back", "B");;

        // Load coins
        coin = PlayerPrefs.GetInt("Coin");
        coinText.text = coin.ToString();
    }

    void SaveStats()
    {
        // Save selected cosmetics
        PlayerPrefs.SetInt("BirdSelected", birdSelected);
        PlayerPrefs.SetInt("BackgroundSelected", backgroundSelected);
        PlayerPrefs.SetInt("ObstacleSelected", obstacleSelected);

        // Save bought cosmetics and skills
        for (int i = 1; i < birdsBought.Length; i++) PlayerPrefs.SetInt($"BirdsBought{i}", birdsBought[i] ? 1 : 0);
        for (int i = 1; i < backgroundsBought.Length; i++) PlayerPrefs.SetInt($"BackgroundsBought{i}", backgroundsBought[i] ? 1 : 0);
        for (int i = 1; i < obstaclesBought.Length; i++) PlayerPrefs.SetInt($"ObstaclesBought{i}", obstaclesBought[i] ? 1 : 0);
        PlayerPrefs.SetInt("Skill1GreedLevel", skill1GreedLevel);
        PlayerPrefs.SetInt("Skill2ShieldLevel", skill2ShieldLevel);

        // Save options
        PlayerPrefs.SetInt("Game Speed", gameSpeed);
        PlayerPrefs.SetFloat("GlobalVolume", AudioListener.volume);
        PlayerPrefs.SetInt("SpawnBirds", spawnBirds);
        PlayerPrefs.SetInt("ShowFps", showFps);
        PlayerPrefs.SetString("FlapKey", flapKey);

        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.Save();
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
    void Add500Coin()
    {
        coin += 500;
        coinText.text = coin.ToString();
        Debug.Log($"Added 500 coins! Total: {coin}");
    }
#endif
}