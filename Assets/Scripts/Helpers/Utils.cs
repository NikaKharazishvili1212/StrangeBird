using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

// Static utility methods for common game operations
public static class Utils
{
    #region Coroutine Timers
    static readonly Dictionary<float, WaitForSeconds> Cache = new(); // Caches WaitForSeconds by duration to avoid repeated allocations

    // Waits for the given seconds then invokes the action; safe if the object is destroyed
    // Usage: this.Wait(2f, () => { your code here });
    public static void Wait(this MonoBehaviour mb, float seconds, UnityAction action) => mb.StartCoroutine(WaitRoutine(mb, seconds, action));
    static IEnumerator WaitRoutine(MonoBehaviour mb, float seconds, UnityAction action)
    {
        if (!Cache.TryGetValue(seconds, out var wait)) Cache[seconds] = wait = new WaitForSeconds(seconds);
        yield return wait;
        if (mb) action?.Invoke();
    }

    // Repeats action every interval seconds until the returned coroutine is stopped
    // Usage: loop = StartCoroutine(RepeatLoop(1f, () => { your code here }));  StopCoroutine(loop);
    public static IEnumerator RepeatLoop(float interval, UnityAction action)
    {
        if (!Cache.TryGetValue(interval, out var wait)) Cache[interval] = wait = new WaitForSeconds(interval);
        while (true)
        {
            yield return wait;
            action?.Invoke();
        }
    }

    // Repeats action every interval seconds, N times, optionally invoking immediately, then calls onComplete
    // Usage: StartCoroutine(RepeatTimes(0.5f, 5, () => { your code here }, true, () => { your code here }));
    public static IEnumerator RepeatTimes(float interval, int times, UnityAction action, bool invokeImmediately = true, UnityAction onComplete = null)
    {
        if (!Cache.TryGetValue(interval, out var wait)) Cache[interval] = wait = new WaitForSeconds(interval);
        if (invokeImmediately) action?.Invoke();
        for (int i = invokeImmediately ? 1 : 0; i < times; i++)
        {
            yield return wait;
            action?.Invoke();
        }
        onComplete?.Invoke();
    }
    #endregion


    #region Probability
    // Returns true if a random roll succeeds for the given percent (0-100)
    // Usage: if (PercentChance(30)) print("it succeed");
    public static bool PercentChance(int percent) => Random.Range(0, 100) < percent;

    // Returns a random element from any IList
    // Usage: audioSource.PlayOneShot(GetRandomElement(sounds));
    public static T GetRandomElement<T>(IList<T> collection) => collection[Random.Range(0, collection.Count)];

    // Randomly invokes one action based on its chance
    // Usage: PlayRandomAction((() => Method1(), 70), (() => Method2(), 25), (() => Method3(), 5));
    public static void PlayRandomAction(params (UnityAction action, int chance)[] options)
    {
        int total = 0;
        foreach (var option in options) total += option.chance;
        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var o in options)
        {
            cumulative += o.chance;
            if (roll < cumulative) { o.action?.Invoke(); return; }
        }
    }
    #endregion


    #region GameObject & Component
    // Safely gets a component, adding it if missing
    // Usage: rb = GetOrAddComponent<Rigidbody2D>(gameObject);
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component => obj.TryGetComponent(out T comp) ? comp : obj.AddComponent<T>();

    // Activates the first inactive GameObject in the pool
    // Usage: PoolGameObject(gameObjects);
    public static void PoolGameObject(GameObject[] pool) { foreach (var go in pool) if (!go.activeInHierarchy) { go.SetActive(true); return; } }

    // Returns the first inactive component from the pool
    // Usage: PoolComponent(bullets)?.Shoot(target);
    public static T PoolComponent<T>(T[] pool) where T : Component
    {
        foreach (var comp in pool) if (!comp.gameObject.activeInHierarchy) return comp;
        return null;
    }
    #endregion


    #region Math & Geometry
    // Checks if given collider is within self range using ClosestPoint for accuracy (usually used to check if character is close enough its target to attack)
    public static bool IsInRange(Transform self, Collider target, float attackRange) => Vector3.Distance(self.position, target.ClosestPoint(self.position)) <= attackRange;

    // Smoothly rotates towards given position (usually used for rotating character towards target when attacking)
    public static void RotateTowards(Transform self, Vector3 givenPos, float rotationSpeed = 10)
    {
        Vector3 direction = (givenPos - self.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        self.rotation = Quaternion.Slerp(self.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // Makes a transform always face the camera and maintain a constant screen size regardless of distance (usually used for character's 3D head UI)
    public static void SetScreenSizeBillboard(Transform target, Transform camera, float screenSize)
    {
        target.rotation = camera.rotation;
        float scaleValue = Vector3.Dot(target.position - camera.position, camera.forward) * screenSize;
        target.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
    #endregion



    #region Strings & Formattings
    // Returns colored rich text string
    public static string ColoredText(string text, Color32 color) => $"<color=#{color.r:X2}{color.g:X2}{color.b:X2}>{text}</color>";
    public static string ColoredText(string text, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

    // Formats health values as "Current (Percentage%)" for health displays
    public static string FormatHealthPercent(float current, float max) => $"{current:N0} ({Mathf.Round(100 * current / max)}%)";

    // Formats number with thousand separators. Usage: 1000000.WithSeparators(); → "1,000,000"
    public static string WithSeparators(this int n) => $"{n:N0}";

    // Pads number with leading zeros to given digits. Usage: ZeroPadded(2, 3) → "002"
    public static string ZeroPadded(int n, int digits) => n.ToString($"D{digits}");
    #endregion


    #region Scene & UI
    // Checks if UI is blocking mouse input; ignores elements with disabled Raycast Target
    public static bool IsUIBlockingInput() => EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);

    // Loads a scene asynchronously with optional loading bar and text updates and brief delay
    // Usage: StartCoroutine(LoadSceneAsync("SceneName", loadingBar, loadingText, 0.5f));
    public static IEnumerator LoadSceneAsync(string sceneName, Image loadingBar = null, TextMeshProUGUI loadingText = null, float briefDelay = 0)
    {
        // Start loading the scene asynchronously and prevent the scene from switching automatically
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Wait until the scene is loaded to 90% (Unity loads 0.9 max before activation)
        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f; // Normalize progress to 0-1 range
            if (loadingText) loadingText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            if (loadingBar) loadingBar.fillAmount = progress;
            yield return null;
        }

        // Scene is ready - show 100% briefly then activate
        if (loadingText) loadingText.text = "Loading 100%";
        if (loadingBar) loadingBar.fillAmount = 1;
        yield return new WaitForSeconds(briefDelay);
        operation.allowSceneActivation = true;
    }

    // Fades a CanvasGroup in or out over duration. Usage: StartCoroutine(Fade(canvasGroup, 0f, 1f, 0.5f));
    public static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    // Punches a transform's local scale for a pop/bounce effect, then restores it. Usage: StartCoroutine(PunchScale(transform, 1f, 0.15f));
    public static IEnumerator PunchScale(Transform t, float peakScale, float duration)
    {
        Vector3 original = t.localScale;
        float half = duration * 0.5f;
        for (float e = 0; e < half; e += Time.deltaTime)
        {
            t.localScale = Vector3.LerpUnclamped(original, original * peakScale, e / half);
            yield return null;
        }
        for (float e = 0; e < half; e += Time.deltaTime)
        {
            t.localScale = Vector3.LerpUnclamped(original * peakScale, original, e / half);
            yield return null;
        }
        t.localScale = original;
    }
    #endregion


    #region Application
    // Application.Quit() is a no-op in Editor but still triggers OnApplicationQuit()
    public static void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public enum Platform { Editor, PC, Mobile, iPhone, Android, Console }
    // Returns true if the game is currently running on the given platform
    public static bool IsCurrentPlatform(Platform platform)
    {
        switch (platform)
        {
            case Platform.Editor: return Application.isEditor;
            case Platform.PC: return Application.platform is RuntimePlatform.WindowsPlayer or RuntimePlatform.OSXPlayer or RuntimePlatform.LinuxPlayer;
            case Platform.Mobile: return Application.platform is RuntimePlatform.IPhonePlayer or RuntimePlatform.Android;
            case Platform.Console: return Application.platform is RuntimePlatform.PS4 or RuntimePlatform.PS5 or RuntimePlatform.XboxOne or RuntimePlatform.Switch;
            default: return false;
        }
    }
    #endregion
}