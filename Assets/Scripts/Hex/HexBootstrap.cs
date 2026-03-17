using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HexTris
{
    public static class HexBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // Only run in HexScene
            if (SceneManager.GetActiveScene().name != "HexScene") return;

            Debug.Log("[HexTris] Initializing HexTris...");

            // Clean up any TripTris objects that may have been created
            CleanupTripTris();

            SetupCamera();
            SetupGameManagers();
            SetupUI();
            SetupPostProcessing();
            SetupGridVisualization();

            Debug.Log("[HexTris] Game initialized");
        }

        private static void CleanupTripTris()
        {
            string[] tripTrisObjects = {
                "GridManager", "GameManager", "BlockSpawner", "PlayerInput",
                "UI Canvas", "Global Volume", "Grid Visualization",
                "BlockMaterialManager", "BlockPrefab"
            };
            foreach (var name in tripTrisObjects)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                {
                    Debug.Log($"[HexTris] Cleaning up TripTris object: {name}");
                    Object.Destroy(obj);
                }
            }
        }

        private static void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }

            // Grid visual bounds:
            // Width: 8 hexes * 0.866 = ~6.93, center at ~3.03
            // Height: 16 rows * 0.75 = 12, center at ~5.6
            cam.transform.position = new Vector3(3.0f, 5.5f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.allowHDR = true;
            cam.allowMSAA = true;

            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
        }

        private static void SetupGameManagers()
        {
            // HexGrid
            var gridObj = new GameObject("HexGrid");
            gridObj.AddComponent<HexGrid>();

            // HexGameManager
            var gmObj = new GameObject("HexGameManager");
            gmObj.AddComponent<HexGameManager>();

            // HexSpawner
            var spawnerObj = new GameObject("HexSpawner");
            spawnerObj.AddComponent<HexSpawner>();

            // HexPlayerInput
            var inputObj = new GameObject("HexPlayerInput");
            inputObj.AddComponent<HexPlayerInput>();

            Debug.Log("[HexTris] Managers created");
        }

        private static void SetupUI()
        {
            var canvasObj = new GameObject("HexUI Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var uiObj = new GameObject("HexUI");
            var rect = uiObj.AddComponent<RectTransform>();
            uiObj.transform.SetParent(canvasObj.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            uiObj.AddComponent<HexUI>();
        }

        private static void SetupPostProcessing()
        {
            var volumeObj = new GameObject("HexGlobalVolume");
            var volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            // Subtle bloom (emission-driven, not overwhelming)
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.4f;
            bloom.intensity.overrideState = true;
            bloom.threshold.value = 1.5f;
            bloom.threshold.overrideState = true;
            bloom.scatter.value = 0.3f;
            bloom.scatter.overrideState = true;
            bloom.highQualityFiltering.value = true;
            bloom.highQualityFiltering.overrideState = true;

            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.contrast.value = 10f;
            colorAdj.contrast.overrideState = true;
            colorAdj.saturation.value = 15f;
            colorAdj.saturation.overrideState = true;
        }

        private static void SetupGridVisualization()
        {
            var visObj = new GameObject("HexGridVisualization");

            // Draw bottom edge of hex grid
            DrawBottomEdge(visObj.transform);

            // Draw subtle hex cell outlines for empty grid
            DrawGridCells(visObj.transform);
        }

        private static void DrawBottomEdge(Transform parent)
        {
            float s = HexGrid.HexSize * 0.92f;
            float topY = HexGrid.Rows * HexGrid.RowSpacing + HexGrid.HexSize * 0.3f;
            var points = new System.Collections.Generic.List<Vector3>();

            // Left wall from top down to bottom-left hex
            Vector3 firstHexCenter = HexGrid.GridToWorld(0, 0);
            float leftX = firstHexCenter.x - s * Mathf.Cos(30f * Mathf.Deg2Rad);
            points.Add(new Vector3(leftX, topY, 0));
            points.Add(new Vector3(leftX, firstHexCenter.y + s * Mathf.Sin(150f * Mathf.Deg2Rad), 0));

            // Trace bottom edge following hex contour of row 0 (8 hexes)
            for (int c = 0; c < 8; c++)
            {
                Vector3 center = HexGrid.GridToWorld(c, 0);
                // Bottom-left vertex (210 degrees) and bottom-right vertex (330 degrees)
                // For pointy-top hex: vertices at 90, 30, -30, -90, -150, -210 (or 210, 150)
                float blAngle = 210f * Mathf.Deg2Rad;
                float brAngle = 330f * Mathf.Deg2Rad;
                float botAngle = 270f * Mathf.Deg2Rad;

                if (c == 0)
                {
                    // Start with bottom-left vertex
                    points.Add(center + new Vector3(s * Mathf.Cos(blAngle), s * Mathf.Sin(blAngle), 0));
                }
                // Bottom vertex
                points.Add(center + new Vector3(s * Mathf.Cos(botAngle), s * Mathf.Sin(botAngle), 0));
                // Bottom-right vertex
                points.Add(center + new Vector3(s * Mathf.Cos(brAngle), s * Mathf.Sin(brAngle), 0));
            }

            // Right wall from bottom-right hex up to top
            Vector3 lastHexCenter = HexGrid.GridToWorld(7, 0);
            float rightX = lastHexCenter.x + s * Mathf.Cos(30f * Mathf.Deg2Rad);
            points.Add(new Vector3(rightX, lastHexCenter.y + s * Mathf.Sin(30f * Mathf.Deg2Rad), 0));
            points.Add(new Vector3(rightX, topY, 0));

            // Create line renderer
            var lineObj = new GameObject("Bottom Edge");
            lineObj.transform.SetParent(parent);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.25f, 0.35f, 0.5f, 0.6f);
            lr.endColor = new Color(0.25f, 0.35f, 0.5f, 0.6f);
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.sortingOrder = -1;
            lr.useWorldSpace = true;
        }

        private static void DrawGridCells(Transform parent)
        {
            // Draw very subtle hex outlines for each cell position
            var mat = new Material(Shader.Find("Sprites/Default"));
            Color cellColor = new Color(0.15f, 0.15f, 0.25f, 0.2f);

            for (int r = 0; r < HexGrid.Rows; r++)
            {
                for (int c = 0; c < HexGrid.ColCount(r); c++)
                {
                    Vector3 pos = HexGrid.GridToWorld(c, r);
                    DrawHexOutline(parent, pos, HexGrid.HexSize * 0.88f, cellColor, mat);
                }
            }
        }

        private static void DrawHexOutline(Transform parent, Vector3 center, float size, Color color, Material mat)
        {
            var obj = new GameObject("CellOutline");
            obj.transform.SetParent(parent);
            var lr = obj.AddComponent<LineRenderer>();

            var points = new Vector3[7];
            for (int i = 0; i < 6; i++)
            {
                float angle = (90f - 60f * i) * Mathf.Deg2Rad;
                points[i] = center + new Vector3(size * Mathf.Cos(angle), size * Mathf.Sin(angle), 0);
            }
            points[6] = points[0]; // close loop

            lr.positionCount = 7;
            lr.SetPositions(points);
            lr.startWidth = 0.015f;
            lr.endWidth = 0.015f;
            lr.material = mat;
            lr.startColor = color;
            lr.endColor = color;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.sortingOrder = -2;
            lr.useWorldSpace = true;
            lr.loop = false;
        }
    }
}
