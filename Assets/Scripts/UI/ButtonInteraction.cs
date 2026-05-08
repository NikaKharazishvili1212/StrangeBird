using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Per-button component. Delegates all effects to the scene-wide UIEffectsManager singleton.
/// Attach to every interactive button; set tooltipText in Inspector (leave empty for no tooltip).
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    static UIEffectsManager FX => UIEffectsManager.Instance;
    [SerializeField] string tooltipText; // Leave empty to suppress tooltip for this button

    public void OnPointerEnter(PointerEventData _) => FX.OnButtonEnter(tooltipText);
    public void OnPointerClick(PointerEventData _) => FX.OnButtonClick();
    public void OnPointerExit(PointerEventData _) => FX.OnButtonExit();
    void OnDisable() => FX?.OnButtonExit();
}