using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

using VInspector;

// Partial class for managing the options menu, including difficulty, sound, FPS display, and bird spawning
public sealed partial class MenuManager : MonoBehaviour
{
    [Tab("Options Menu")]
    [SerializeField] TextMeshProUGUI[] difficultyTexts;
    [SerializeField] TextMeshProUGUI fpsText, flapKeyText;
    [SerializeField] Image soundsCheckmark, birdsCheckmark, fpsCheckmark;
    [SerializeField] AudioClip keySelectSound;
    string flapKey;
    int difficulty, spawnBirds, showFps;

    enum MenuOption : byte { EasyDifficulty = 0, MediumDifficulty = 1, HardDifficulty = 2, ToggleSound = 3, ToggleBirds = 4, ToggleFps = 5, SelectFlapKey = 6 }

    // Handles option menu selections (difficulty, sound, FPS, birds) using enum-based input
    public void OptionsSelection(int index)
    {
        switch ((MenuOption)index)
        {
            case MenuOption.EasyDifficulty:
            case MenuOption.MediumDifficulty:
            case MenuOption.HardDifficulty:
                SetDifficulty(index);
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
                    InvokeRepeating(nameof(ShowFps), 0, 1f);
                    SetCheckmarkSprite(fpsCheckmark, true);
                }
                break;
            case MenuOption.SelectFlapKey:
                StartCoroutine(DetectFlapKey());
                break;
        }
    }

    // Sets the game difficulty and updates the UI to reflect the current selection
    void SetDifficulty(int index)
    {
        difficulty = index;
        foreach (TextMeshProUGUI text in difficultyTexts) text.color = Color.white;
        difficultyTexts[index].color = Color.yellow;
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
                    flapKeyText.text = FormatKeyName(key.ToString());
                    yield break;
                }
            }
            audioSource.PlayOneShot(cantDoThatSound);
            yield return null;  // Ignore invalid key and continue detection
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

    // Formats key names for display
    string FormatKeyName(string keyName)
    {
        return keyName
        .Replace("Alpha", "A")  // Alpha1 -> A1
        .Replace("Keypad", "K")  // Keypad1 -> K1
        .Replace("Left", "L")  // LeftShift -> LShift
        .Replace("Right", "R")  // RightShift -> RShift
        .Replace("Back", "B");  // BackQuote -> BQuote, BackSlash -> BSlash, Backspace -> Bspace
    }

    // In case player went back without choosing key while it was active, stop detection and restore formatted key
    void ResetKey()
    {
        StopCoroutine(nameof(DetectFlapKey));
        flapKeyText.text = FormatKeyName(flapKey);
    }
}