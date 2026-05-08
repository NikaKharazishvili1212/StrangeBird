#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Global editor hotkeys:
///   F5         — Align Main Camera with Scene view
///   F6         — Find prefab asset of selected object in Project window
///   F7         — Activate/Deactivate selected object
///   F8         — Lock/unlock inspector
/// 
///   F9         — Step one frame forward (auto-pauses if playing)
///   F10        — Toggle Pause
///   F11        — Toggle Play/Stop (enters fullscreen Game view on play, exits on stop)
///   F12        — Toggle Scene view fullscreen
/// 
///   ` + 1–9    — Load scene by build index (1 = index 0, 2 = index 1, … 9 = index 8)
/// </summary>
[InitializeOnLoad]
public static class EditorHotkeys
{
    static readonly Type gameViewType;
    static readonly PropertyInfo maximizedProp;
    static bool backquoteHeld;

    static EditorHotkeys()
    {
        gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        maximizedProp = typeof(EditorWindow).GetProperty("maximized", BindingFlags.Instance | BindingFlags.Public);

        var globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
        if (globalEventHandler != null)
        {
            var existing = globalEventHandler.GetValue(null) as EditorApplication.CallbackFunction;
            globalEventHandler.SetValue(null, existing + OnGlobalEvent);
        }
        else Debug.LogWarning("[EditorHotkeys] Could not hook globalEventHandler.");
    }

    static void OnGlobalEvent()
    {
        var e = Event.current;
        if (e == null) return;

        // Track backtick held state
        if (e.keyCode == KeyCode.BackQuote)
        {
            if (e.type == EventType.KeyDown) backquoteHeld = true;
            else if (e.type == EventType.KeyUp) backquoteHeld = false;
        }

        if (e.type != EventType.KeyDown) return;

        // Align Main Camera with Scene view (F5)
        if (e.keyCode == KeyCode.F5)
        {
            var sceneCam = SceneView.lastActiveSceneView?.camera;
            var mainCam = Camera.main;
            if (sceneCam != null && mainCam != null)
            {
                Undo.RecordObject(mainCam.transform, "Align Camera With Scene View");
                mainCam.transform.SetPositionAndRotation(sceneCam.transform.position, sceneCam.transform.rotation);
            }
            e.Use(); return;
        }

        // Find prefab asset in Project window (F6)
        if (e.keyCode == KeyCode.F6)
        {
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeGameObject);
            if (prefabAsset != null) EditorGUIUtility.PingObject(prefabAsset);
            e.Use(); return;
        }

        // Activate/Deactivate selected object (F7)
        if (e.keyCode == KeyCode.F7)
        {
            var go = Selection.activeGameObject;
            if (go != null)
            {
                Undo.RecordObject(go, "Toggle Active");
                go.SetActive(!go.activeSelf);
            }
            e.Use(); return;
        }

        // Lock/Unlock inspector (F8)
        if (e.keyCode == KeyCode.F8)
        {
            if (Selection.gameObjects.Length == 1)
            {
                var inspectors = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var win in inspectors)
                {
                    if (win.GetType().Name == "InspectorWindow")
                    {
                        var isLocked = win.GetType().GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
                        if (isLocked != null) isLocked.SetValue(win, !(bool)isLocked.GetValue(win));
                        win.Repaint();
                        break;
                    }
                }
            }
            e.Use(); return;
        }

        // Step one frame forward, auto-pauses if playing (F9)
        if (e.keyCode == KeyCode.F9)
        {
            var wins = Resources.FindObjectsOfTypeAll(gameViewType);
            bool wasMaximized = wins.Length > 0 && (bool)maximizedProp.GetValue(wins[0]);
            EditorApplication.Step();
            EditorApplication.delayCall += () => { if (wasMaximized && wins.Length > 0) maximizedProp.SetValue(wins[0], true); };
            e.Use(); return;
        }

        // Toggle Pause (F10)
        if (e.keyCode == KeyCode.F10)
        {
            var wins = Resources.FindObjectsOfTypeAll(gameViewType);
            bool wasMaximized = wins.Length > 0 && (bool)maximizedProp.GetValue(wins[0]);
            EditorApplication.isPaused = !EditorApplication.isPaused;
            EditorApplication.delayCall += () => { if (wasMaximized && wins.Length > 0) maximizedProp.SetValue(wins[0], true); };
            e.Use(); return;
        }

        // Toggle Play/Stop (F11)
        if (e.keyCode == KeyCode.F11)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
                EditorApplication.delayCall += () =>
                {
                    var wins = Resources.FindObjectsOfTypeAll(gameViewType);
                    if (wins.Length > 0) maximizedProp.SetValue(wins[0], true);
                };
            }
            else
            {
                EditorApplication.ExitPlaymode();
                EditorApplication.delayCall += () =>
                {
                    var wins = Resources.FindObjectsOfTypeAll(gameViewType);
                    if (wins.Length > 0) maximizedProp.SetValue(wins[0], false);
                };
            }
            e.Use(); return;
        }

        // Toggle Scene view fullscreen (F12)
        if (e.keyCode == KeyCode.F12)
        {
            if (maximizedProp == null) { e.Use(); return; }
            var gameWin = Resources.FindObjectsOfTypeAll(gameViewType);
            if (gameWin.Length > 0) maximizedProp.SetValue(gameWin[0], false);
            var sceneWin = Resources.FindObjectsOfTypeAll(typeof(SceneView));
            if (sceneWin.Length > 0) maximizedProp.SetValue(sceneWin[0], !(bool)maximizedProp.GetValue(sceneWin[0]));
            e.Use(); return;
        }

        // Load scene by build index (` + 1–9)
        if (backquoteHeld)
        {
            int sceneIndex = e.keyCode switch
            {
                KeyCode.Alpha1 => 0,
                KeyCode.Alpha2 => 1,
                KeyCode.Alpha3 => 2,
                KeyCode.Alpha4 => 3,
                KeyCode.Alpha5 => 4,
                KeyCode.Alpha6 => 5,
                KeyCode.Alpha7 => 6,
                KeyCode.Alpha8 => 7,
                KeyCode.Alpha9 => 8,
                _ => -1
            };
            if (sceneIndex < 0) return;

            int sceneCount = EditorBuildSettings.scenes.Length;
            if (sceneCount == 0) { Debug.LogWarning("[EditorHotkeys] No scenes in Build Settings."); e.Use(); return; }
            if (sceneIndex >= sceneCount) { Debug.LogWarning($"[EditorHotkeys] Scene index {sceneIndex} doesn't exist (only {sceneCount} scene(s) in Build Settings)."); e.Use(); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) { e.Use(); return; }
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[sceneIndex].path);
            e.Use();
        }
    }
}
#endif