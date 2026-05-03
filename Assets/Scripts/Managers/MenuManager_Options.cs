using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Nikspector;
using static Constants;

// Partial class for managing the options menu, including game speed, sound, FPS display, and bird spawning
public sealed partial class MenuManager : MonoBehaviour
{
    [Tab("Options Menu")]
    [SerializeField] TextMeshProUGUI[] gameSpeedTexts;
    [SerializeField] TextMeshProUGUI fpsText, flapKeyText;
    [SerializeField] Image soundsCheckmark, birdsCheckmark, fpsCheckmark;
    [SerializeField] AudioClip keySelectSound;
    string flapKey;
    int gameSpeed, spawnBirds, showFps;

    enum MenuOption : byte { EasyGameSpeed = 0, MediumGameSpeed = 1, FastGameSpeed = 2, ToggleSound = 3, ToggleBirds = 4, ToggleFps = 5, SelectFlapKey = 6 }

    // Handles option menu selections (game speed, sound, FPS, birds) using enum-based input
    public void OptionsSelection(int index)
    {
        switch ((MenuOption)index)
        {
            case MenuOption.EasyGameSpeed:
            case MenuOption.MediumGameSpeed:
            case MenuOption.FastGameSpeed:
                SetGameSpeed(index);
                break;
            case MenuOption.ToggleSound:
                AudioListener.volume = AudioListener.volume == 1 ? 0 : 1;
                SetCheckmarkSprite(soundsCheckmark, AudioListener.volume == 1);
                break;
            case MenuOption.ToggleBirds:
                spawnBirds = spawnBirds == 1 ? 0 : 1;
                SetCheckmarkSprite(birdsCheckmark, spawnBirds == 1);
                break;
            case MenuOption.ToggleFps:
                if (showFps == 1)
                {
                    showFps = 0;
                    fpsText.gameObject.SetActive(false);
                    CancelInvoke(nameof(ShowFps));
                    SetCheckmarkSprite(fpsCheckmark, false);
                }
                else
                {
                    showFps = 1;
                    fpsText.gameObject.SetActive(true);
                    InvokeRepeating(nameof(ShowFps), 0, FPSHudUpdateInterval);
                    SetCheckmarkSprite(fpsCheckmark, true);
                }
                break;
            case MenuOption.SelectFlapKey:
                StartCoroutine(nameof(DetectFlapKey));
                break;
        }
    }

    // Sets the game speed and updates the UI to reflect the current selection
    void SetGameSpeed(int index)
    {
        gameSpeed = index;
        UpdateGameSpeedTextColors();
    }
    
    // Update game speed button text colors to reflect current selection
    void UpdateGameSpeedTextColors()
    {
        foreach (TextMeshProUGUI text in gameSpeedTexts) text.color = Color.white;
        gameSpeedTexts[gameSpeed].color = Color.yellow;
    }

    // Updates the checkmark sprite based on whether the option is enabled or disabled
    void SetCheckmarkSprite(Image image, bool isEnabled) => image.sprite = spriteAtlas.GetSprite(isEnabled ? "Checkmark_Enabled" : "Checkmark_Disabled");

    // Updates FPS display text every second when enabled
    void ShowFps() => fpsText.text = $"FPS: {Mathf.RoundToInt(1f / Time.deltaTime)}";

    // Detects a single key press and saves it as the Flap key
    System.Collections.IEnumerator DetectFlapKey()
    {
        flapKeyText.text = "...";
        while (true)
        {
            while (!Input.anyKeyDown) yield return null;
            foreach (KeyCode key in ValidFlapKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    audioSource.PlayOneShot(keySelectSound);
                    flapKey = key.ToString();
                    // Formats key names for display: Alpha1 -> A1, Keypad1 -> K1, LeftShift -> LShift, RightShift -> RShift, BackQuote -> BQuote, BackSlash -> BSlash, Backspace -> Bspace
                    flapKeyText.text = key.ToString().Replace("Alpha", "A").Replace("Keypad", "K").Replace("Left", "L").Replace("Right", "R").Replace("Back", "B");
                    yield break;
                }
            }
            yield return null;
        }
    }

    // List of valid keys for flap input (anything besides these)
    static readonly KeyCode[] ValidFlapKeys = System.Enum.GetValues(typeof(KeyCode))
    .Cast<KeyCode>()
    .Where(key => (key < KeyCode.Mouse0 || key > KeyCode.Mouse6) &&
                  key != KeyCode.Escape && key != KeyCode.Pause && key != KeyCode.Print &&
                  key != KeyCode.SysReq && key != KeyCode.Break &&
                  key != KeyCode.Numlock && key != KeyCode.CapsLock && key != KeyCode.ScrollLock)
    .ToArray();
}