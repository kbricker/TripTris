// Force recompile v2
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using TripTris.Blocks;
using TripTris.UI;
using TripTrisPlayerInput = TripTris.Input.PlayerInput;

namespace TripTris.Core
{
    /// <summary>
    /// Bootstrapper script that sets up the entire TripTris game scene at runtime.
    /// Automatically creates all necessary game objects, camera, UI, and visual effects.
    /// </summary>
    public static class TripTrisBootstrap
    {
        private const int GRID_WIDTH = 8;
        private const int GRID_HEIGHT = 16;
        private const float CELL_SIZE = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // Only run in SampleScene (skip HexScene and other scenes)
            if (SceneManager.GetActiveScene().name != "SampleScene") return;

            Debug.Log("[TripTris] Starting game initialization...");

            SetupCamera();
            SetupGameManagers();
            SetupUI();
            SetupPostProcessing();
            SetupGridVisualization();

            Debug.Log("[TripTris] Game initialized");
        }

        /// <summary>
        /// Sets up the main camera with proper positioning and settings for the 8x16 grid.
        /// Uses orthographic projection for clean 2D puzzle game view.
        /// </summary>
        private static void SetupCamera()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            // Grid is 8 wide (X: 0-7) and 16 tall (Y: 0-15)
            // Center X at 3.5 (middle of 0-7), center Y at 7.5 (middle of 0-15)
            // Add small offset to Y to show full bottom row with padding
            mainCamera.transform.position = new Vector3(3.5f, 7.5f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;

            // Camera settings - orthographic for clean 2D view
            mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.1f); // Dark blue-black
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 9.5f; // Half of view height (shows ~19 units, grid is 16)
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 100f;

            // Enable HDR for bloom effect
            mainCamera.allowHDR = true;
            mainCamera.allowMSAA = true;

            // Add Universal Additional Camera Data for URP
            var cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;

            Debug.Log("[TripTris] Camera configured");
        }

        /// <summary>
        /// Creates and initializes all core game manager objects.
        /// </summary>
        private static void SetupGameManagers()
        {
            // Create GridManager FIRST so GameManager.FindReferences() finds it
            GameObject gridManagerObj = new GameObject("GridManager");
            var gridManager = gridManagerObj.AddComponent<GridManager>();

            // Create GameManager (Awake->FindReferences will find GridManager above)
            GameObject gameManagerObj = new GameObject("GameManager");
            var gameManager = gameManagerObj.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(gameManagerObj);

            // Create BlockSpawner
            GameObject spawnerObj = new GameObject("BlockSpawner");
            var spawner = spawnerObj.AddComponent<BlockSpawner>();
            spawner.transform.position = new Vector3(GRID_WIDTH / 2f - 0.5f, GRID_HEIGHT, 0f);

            // Create PlayerInput
            GameObject inputObj = new GameObject("PlayerInput");
            var playerInput = inputObj.AddComponent<TripTrisPlayerInput>();

            Debug.Log("[TripTris] Game managers created");
        }

        /// <summary>
        /// Sets up the UI canvas and game UI elements.
        /// </summary>
        private static void SetupUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasScaler.scaleFactor = 1f;

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create GameUI with explicit RectTransform
            GameObject gameUIObj = new GameObject("GameUI");
            RectTransform rectTransform = gameUIObj.AddComponent<RectTransform>();
            gameUIObj.transform.SetParent(canvasObj.transform, false);

            // Set up RectTransform to fill canvas
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add GameUI component after RectTransform is set up
            gameUIObj.AddComponent<GameUI>();

            Debug.Log("[TripTris] UI canvas created");
        }

        /// <summary>
        /// Sets up post-processing volume with bloom effect for neon glow.
        /// </summary>
        private static void SetupPostProcessing()
        {
            GameObject volumeObj = new GameObject("Global Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            // Create volume profile
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            // Add Bloom effect (subtle glow, not washed out)
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.6f;
            bloom.intensity.overrideState = true;
            bloom.threshold.value = 1.2f;
            bloom.threshold.overrideState = true;
            bloom.scatter.value = 0.4f;
            bloom.scatter.overrideState = true;
            bloom.tint.value = Color.white;
            bloom.tint.overrideState = true;
            bloom.highQualityFiltering.value = true;
            bloom.highQualityFiltering.overrideState = true;

            // Add Color Adjustments for contrast
            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.contrast.value = 10f;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.saturation.value = 10f;
            colorAdjustments.saturation.overrideState = true;

            Debug.Log("[TripTris] Post-processing volume created with bloom effect");
        }

        /// <summary>
        /// Creates subtle grid visualization lines to show column boundaries.
        /// </summary>
        private static void SetupGridVisualization()
        {
            GameObject gridVisObj = new GameObject("Grid Visualization");

            // Create vertical lines for each column boundary
            for (int x = 0; x <= GRID_WIDTH; x++)
            {
                CreateGridLine(
                    gridVisObj.transform,
                    $"Column Line {x}",
                    new Vector3(x, 0, 0),
                    new Vector3(x, GRID_HEIGHT, 0)
                );
            }

            // Create horizontal lines for visual reference (every 4 rows)
            for (int y = 0; y <= GRID_HEIGHT; y += 4)
            {
                CreateGridLine(
                    gridVisObj.transform,
                    $"Row Line {y}",
                    new Vector3(0, y, 0),
                    new Vector3(GRID_WIDTH, y, 0)
                );
            }

            Debug.Log("[TripTris] Grid visualization created");
        }

        /// <summary>
        /// Creates a single grid line using LineRenderer.
        /// </summary>
        private static void CreateGridLine(Transform parent, string name, Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(parent);
            lineObj.transform.position = Vector3.zero;

            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            // Configure line renderer
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // Set material and color
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.2f, 0.2f, 0.3f, 0.3f); // Subtle dark blue-gray
            lineRenderer.endColor = new Color(0.2f, 0.2f, 0.3f, 0.3f);

            // Disable shadows
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            // Set sorting
            lineRenderer.sortingOrder = -1; // Behind game blocks
        }
    }
}
