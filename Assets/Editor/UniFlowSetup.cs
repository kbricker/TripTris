using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Editor script to automatically set up UniFlow in the project.
/// Can be run via command line: Unity.exe -executeMethod UniFlowSetup.SetupAll
/// </summary>
public static class UniFlowSetup
{
    private const string CONFIG_PATH = "Assets/Resources/UniFlowConfig.asset";
    private const string WORKSPACE_PATH = "D:/kbricker/projects/unity/uniflow/workspace";

    [MenuItem("UniFlow/Setup All (Config + Controller)")]
    public static void SetupAll()
    {
        Debug.Log("=== UniFlow Setup Starting ===");

        CreateConfig();
        AddControllerToScene();

        Debug.Log("=== UniFlow Setup Complete ===");
    }

    [MenuItem("UniFlow/Create Config Asset")]
    public static void CreateConfig()
    {
        // Check if config already exists
        var existingConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(CONFIG_PATH);
        if (existingConfig != null)
        {
            Debug.Log("UniFlowConfig already exists at: " + CONFIG_PATH);
            return;
        }

        // Find the UniFlowConfig type from the UniFlow package
        System.Type configType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            configType = assembly.GetType("UniFlow.UniFlowConfig");
            if (configType != null) break;
        }

        if (configType == null)
        {
            Debug.LogError("Could not find UniFlow.UniFlowConfig type. Is UniFlow package installed?");
            return;
        }

        // Create the config asset
        var config = ScriptableObject.CreateInstance(configType);

        // Set workspace path via reflection
        var workspaceField = configType.GetField("workspacePath");
        if (workspaceField != null)
        {
            workspaceField.SetValue(config, WORKSPACE_PATH);
        }

        // Ensure Resources folder exists
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.Refresh();
        }

        // Save the asset
        AssetDatabase.CreateAsset(config, CONFIG_PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created UniFlowConfig at: " + CONFIG_PATH);
    }

    [MenuItem("UniFlow/Add Controller to Scene")]
    public static void AddControllerToScene()
    {
        // Find the UniFlowController type
        System.Type controllerType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            controllerType = assembly.GetType("UniFlow.UniFlowController");
            if (controllerType != null) break;
        }

        if (controllerType == null)
        {
            Debug.LogError("Could not find UniFlow.UniFlowController type. Is UniFlow package installed?");
            return;
        }

        // Check if controller already exists in scene
        var existingController = Object.FindFirstObjectByType(controllerType);
        if (existingController != null)
        {
            Debug.Log("UniFlowController already exists in scene");
            return;
        }

        // Create the controller GameObject
        GameObject controllerGO = new GameObject("UniFlowController");
        var controller = controllerGO.AddComponent(controllerType) as Component;

        // Load and assign the config
        var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(CONFIG_PATH);
        if (config != null)
        {
            var configField = controllerType.GetField("config");
            if (configField != null)
            {
                configField.SetValue(controller, config);
            }
            else
            {
                // Try property
                var configProp = controllerType.GetProperty("Config");
                if (configProp != null)
                {
                    configProp.SetValue(controller, config);
                }
            }
        }

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("Added UniFlowController to scene and saved");
    }

    [MenuItem("UniFlow/Test Automation")]
    public static void TestAutomation()
    {
        Debug.Log("=== UniFlow Automation Test ===");
        Debug.Log("Entering Play mode...");

        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("Play mode entered - UniFlow should be active");
            Debug.Log("Check workspace/status.json for heartbeat");

            // Schedule screenshot after 2 seconds
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("Exiting play mode...");
                    EditorApplication.ExitPlaymode();
                };
            };
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Debug.Log("=== Automation Test Complete ===");
        }
    }
}
