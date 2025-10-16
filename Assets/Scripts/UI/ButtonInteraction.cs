using UnityEngine;
using UnityEngine.EventSystems;

// Handles UI button interactions including cursor changes, tooltips, and sound effects
public sealed class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    static UIEffectsManager UIEM => UIEffectsManager.Instance;
    [SerializeField] string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIEM.SetCursorHand();
        UIEM.PlayHoverSound();

        if (!string.IsNullOrEmpty(tooltipText))
        {
            UIEM.SetTooltipPosition();
            UIEM.SetTooltipText(tooltipText);
            UIEM.SetTooltipBackgroundActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIEM.SetCursorArrow();
        UIEM.SetTooltipBackgroundActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) => UIEM.PlayClickSound();

    // Clean up if button gets disabled while hovering
    void OnDisable()
    {
        UIEM.SetCursorArrow();
        UIEM.SetTooltipBackgroundActive(false);
    }
}