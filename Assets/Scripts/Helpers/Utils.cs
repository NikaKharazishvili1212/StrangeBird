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
    public static bool PercentChance(int percent) => Random.Range(0, 100) < percent;

    // Exit the application in build, or stop play mode in Unity Editor
    // Ensure Application.Quit() is called first (even in Editor) to trigger OnApplicationQuit() and save data correctly
    public static void QuitApplication()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // Activates the first inactive GameObject from a pool of GameObjects
    public static void PoolObject(GameObject[] pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return;
            }
        }
    }

    // Activates the first inactive GameObject from a pool of Components
    public static void PoolObject<T>(T[] pool) where T : Component
    {
        foreach (T obj in pool)
        {
            if (!obj.gameObject.activeInHierarchy)
            {
                obj.gameObject.SetActive(true);
                return;
            }
        }
    }

    // Loads a scene asynchronously with optional loading bar and text updates
    // Usage: StartCoroutine(LoadSceneAsync("SceneName", loadingBar, loadingText, true));
    public static IEnumerator LoadSceneAsync(string sceneName, Image loadingBar = null, TextMeshProUGUI loadingText = null, float briefDelay = 0)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);  // Start loading the scene asynchronously
        operation.allowSceneActivation = false;  // Prevent the scene from switching automatically

        while (operation.progress < 0.9f)  // Wait until the scene is loaded to 90% (Unity loads 0.9 max before activation)
        {
            float progress = operation.progress / 0.9f;  // Normalize progress to 0-1 range
            // Update optional UI elements if provided
            if (loadingText) loadingText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            if (loadingBar) loadingBar.fillAmount = progress;
            yield return null;
        }

        // Scene is ready - show 100% briefly then activate
        if (loadingText) loadingText.text = "Loading 100%";
        if (loadingBar) loadingBar.fillAmount = 1f;

        yield return new WaitForSeconds(briefDelay);  // Optional brief delay before activation
        operation.allowSceneActivation = true;
    }
}