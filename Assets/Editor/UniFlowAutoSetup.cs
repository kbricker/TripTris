using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Auto-setup script that runs when Unity compiles.
/// Sets up UniFlow automatically without manual intervention.
/// </summary>
[InitializeOnLoad]
public static class UniFlowAutoSetup
{
    private const string SETUP_DONE_KEY = "UniFlowAutoSetup_Done_v1";
    private const string WORKSPACE_PATH = "D:/kbricker/projects/unity/uniflow/workspace";

    static UniFlowAutoSetup()
    {
        // Only run once per project
        if (SessionState.GetBool(SETUP_DONE_KEY, false))
        {
            Debug.Log("[UniFlowAutoSetup] Already ran this session, skipping");
            return;
        }

        // Delay to ensure Unity is fully loaded
        EditorApplication.delayCall += RunSetup;
    }

    private static void RunSetup()
    {
        Debug.Log("=== UniFlow Auto-Setup Starting ===");

        try
        {
            // Step 1: Create config if needed
            CreateConfigIfNeeded();

            // Step 2: Add controller to scene if needed
            AddControllerIfNeeded();

            // Mark as done
            SessionState.SetBool(SETUP_DONE_KEY, true);

            Debug.Log("=== UniFlow Auto-Setup Complete ===");
            Debug.Log("UniFlow is now ready. Try: python runner/main.py is-ready");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UniFlowAutoSetup] Error: " + e.Message);
        }
    }

    private static void CreateConfigIfNeeded()
    {
        string configPath = "Assets/Resources/UniFlowConfig.asset";

        // Check if already exists
        if (File.Exists(configPath))
        {
            Debug.Log("[UniFlowAutoSetup] Config already exists");
            return;
        }

        // Find UniFlowConfig type
        System.Type configType = FindType("UniFlow.UniFlowConfig");
        if (configType == null)
        {
            Debug.LogWarning("[UniFlowAutoSetup] UniFlow.UniFlowConfig not found - is package installed?");
            return;
        }

        // Ensure Resources folder exists
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
        }

        // Create config
        var config = ScriptableObject.CreateInstance(configType);

        // Try to set workspace path
        SetFieldOrProperty(config, "workspacePath", WORKSPACE_PATH);
        SetFieldOrProperty(config, "WorkspacePath", WORKSPACE_PATH);

        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[UniFlowAutoSetup] Created config at: " + configPath);
    }

    private static void AddControllerIfNeeded()
    {
        // Find controller type
        System.Type controllerType = FindType("UniFlow.UniFlowController");
        if (controllerType == null)
        {
            Debug.LogWarning("[UniFlowAutoSetup] UniFlow.UniFlowController not found");
            return;
        }

        // Check if already in scene
        var existing = Object.FindFirstObjectByType(controllerType);
        if (existing != null)
        {
            Debug.Log("[UniFlowAutoSetup] Controller already in scene");
            return;
        }

        // Create controller
        GameObject go = new GameObject("UniFlowController");
        go.AddComponent(controllerType);

        // Try to assign config
        var config = Resources.Load("UniFlowConfig");
        if (config != null)
        {
            var component = go.GetComponent(controllerType);
            SetFieldOrProperty(component, "config", config);
            SetFieldOrProperty(component, "Config", config);
        }

        // Save scene
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[UniFlowAutoSetup] Added controller to scene and saved");
    }

    private static System.Type FindType(string fullName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName);
            if (type != null) return type;
        }
        return null;
    }

    private static void SetFieldOrProperty(object obj, string name, object value)
    {
        var type = obj.GetType();

        var field = type.GetField(name, System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
            return;
        }

        var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(obj, value);
        }
    }
}
