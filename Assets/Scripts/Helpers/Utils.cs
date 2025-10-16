using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;

// Static utility methods for common game operations
public static class Utils
{
    // Returns true if a random number between 0-99 is less than the given percent
    public static bool PercentChanceSuccess(int percent) => Random.Range(0, 100) < percent;

    // Provides an async awaitable delay method as an extension to MonoBehaviour
    // Usage: this.Wait(2f, () => { Your codes here });
    public static async void Wait(this MonoBehaviour monoBehaviour, float delay, UnityAction action)
    {
        await Task.Delay((int)(delay * 1000));
        if (monoBehaviour) action?.Invoke();
        else Debug.LogWarning("MonoBehaviour destroyed before wait completed, action not invoked");
    }

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