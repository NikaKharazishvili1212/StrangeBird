using UnityEngine;
using TMPro;

/// <summary>Scene-wide singleton. Owns all shared UI effect resources. One instance lives on a dedicated GameObject.</summary>
public sealed class UIEffectsManager : MonoBehaviour
{
    public static UIEffectsManager Instance { get; private set; }
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hoverSound, clickSound;

    [Header("Cursor")]
    [SerializeField] RectTransform cursorParent;
    [SerializeField] GameObject cursorArrow, cursorHand;

    [Header("Tooltip")]
    [SerializeField] Camera cam;
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform tooltipBackground;
    [SerializeField] TextMeshProUGUI tooltipDisplay;

    void Awake()
    {
        Instance = this;
        Cursor.visible = false; // Disables the dafault cursor, because we use our own fake one
    }

    void Update() => cursorParent.localPosition = ScreenToCanvas(Input.mousePosition);
    Vector2 ScreenToCanvas(Vector2 screenPos) { RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, cam, out Vector2 local); return local; }

    public void OnButtonEnter(string tooltipText)
    {
        cursorArrow.SetActive(false);
        cursorHand.SetActive(true);
        audioSource.PlayOneShot(hoverSound);

        if (string.IsNullOrEmpty(tooltipText)) return;

        // Position tooltip
        Vector2 pos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, pos, cam, out Vector2 localPoint);
        localPoint.y += localPoint.y > 0 ? -60 : 60;
        tooltipBackground.localPosition = localPoint;
        tooltipBackground.pivot = new Vector2(pos.x / Screen.width, pos.y / Screen.height);

        // Display text
        tooltipDisplay.text = tooltipText;
        tooltipBackground.gameObject.SetActive(true);
    }

    public void OnButtonExit()
    {
        cursorArrow.SetActive(true);
        cursorHand.SetActive(false);
        tooltipBackground.gameObject.SetActive(false);
    }

    public void OnButtonClick() => audioSource.PlayOneShot(clickSound);
}