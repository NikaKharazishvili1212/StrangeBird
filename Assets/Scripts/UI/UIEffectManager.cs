using UnityEngine;
using TMPro;

// Centralized manager for UI visual and audio effects, providing cursor textures, tooltip display, and button sounds to all interactive UI elements through a singleton pattern
// This architecture eliminates duplicate resource references across multiple UI components, optimizing memory usage
public class UIEffectsManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] Texture2D cursorHand, cursorArrow;
    [SerializeField] RectTransform tooltipBackground;
    [SerializeField] TextMeshProUGUI tooltipDisplay;
    [SerializeField] AudioClip hoverSound, clickSound;

    public static UIEffectsManager Instance { get; private set; }

    void Awake() => Instance = this;

    public void PlayHoverSound() => audioSource.PlayOneShot(hoverSound);
    public void PlayClickSound() => audioSource.PlayOneShot(clickSound);

    public void SetCursorHand() => Cursor.SetCursor(cursorHand, Vector2.zero, CursorMode.Auto);
    public void SetCursorArrow() => Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.Auto);

    public void SetTooltipText(string given) => tooltipDisplay.text = given;
    public void SetTooltipBackgroundActive(bool given) => tooltipBackground.gameObject.SetActive(given);

    public void SetTooltipPosition()
    {
        Vector2 mousePos = Input.mousePosition;
        // Offset tooltip vertically based on cursor position: push up if near bottom, push down if near top
        mousePos.y = mousePos.y > Screen.height * 0.5f ? mousePos.y - 100 : mousePos.y + 100;
        tooltipBackground.position = mousePos;  // Set tooltip position near mouse
        tooltipBackground.pivot = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);  // Adjust pivot so tooltip stays inside screen
    }
}