using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

// Static utility methods for common game operations
public static class Utils
{
    // Caches WaitForSeconds by duration to avoid repeated allocations
    static readonly Dictionary<float, WaitForSeconds> Cache = new();
    // Waits for the given seconds then invokes the action; safe if the object is destroyed. Usage: this.Wait(2f, () => { Your codes here });
    public static void Wait(this MonoBehaviour mb, float seconds, UnityAction action) => mb.StartCoroutine(WaitRoutine(mb, seconds, action));
    static IEnumerator WaitRoutine(MonoBehaviour mb, float seconds, UnityAction action)
    {
        if (!Cache.TryGetValue(seconds, out var wait)) Cache[seconds] = wait = new WaitForSeconds(seconds);
        yield return wait;
        if (mb) action?.Invoke();
    }

    // Returns true if a random roll succeeds for the given percent (0-100)
    public static bool PercentChance(int percent) => UnityEngine.Random.Range(0, 100) < percent;

    // Returns random element from arrays
    public static T GetArrayRandomElement<T>(T[] array) => array[UnityEngine.Random.Range(0, array.Length)];

    // Ensures Application.Quit() is called first (even for UnityEditor) to trigger OnApplicationQuit() method in case we are using it
    public static void QuitApplication()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // Activates the first inactive GameObject in a pool of GameObjects or UnityEngine components
    public static void PoolObject<T>(T[] pool) where T : Object
    {
        foreach (var obj in pool)
        {
            GameObject gameObject = obj is Component c ? c.gameObject : obj as GameObject;

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                return;
            }
        }
    }

    // Loads a scene asynchronously with optional loading bar and text updates and brief delay. Usage: StartCoroutine(LoadSceneAsync("SceneName", loadingBar, loadingText, true));
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
}