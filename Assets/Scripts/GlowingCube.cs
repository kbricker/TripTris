using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates a glowing, pulsing magenta cube with bloom post-processing.
/// Auto-spawns when Play mode starts via RuntimeInitializeOnLoadMethod.
/// </summary>
public class GlowingCube : MonoBehaviour
{
    private Material cubeMaterial;
    private Renderer cubeRenderer;

    // Pulse settings
    private readonly float pulseSpeed = 2f;
    private readonly Color baseColor = new Color(1f, 0.2f, 0.8f); // Magenta/Pink
    private readonly float minIntensity = 1.5f;
    private readonly float maxIntensity = 4f;

    // DISABLED - TripTris now uses TripTrisBootstrap instead
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    // private static void AutoSpawn()
    // {
    //     // Create the cube
    //     GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //     cube.name = "GlowingCube";
    //     cube.transform.position = Vector3.zero;
    //
    //     // Remove collider (not needed for visual effect)
    //     Collider col = cube.GetComponent<Collider>();
    //     if (col != null) Object.Destroy(col);
    //
    //     // Add the glowing cube component
    //     GlowingCube glowScript = cube.AddComponent<GlowingCube>();
    //
    //     // Log for telemetry detection
    //     Debug.Log("[GlowingCube] Spawned glowing cube at origin");
    // }

    void Start()
    {
        SetupMaterial();
        SetupCamera();
        SetupPostProcessing();

        Debug.Log("[GlowingCube] Setup complete - pulsing magenta glow active");
    }

    void SetupMaterial()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer == null)
        {
            Debug.LogError("[GlowingCube] Renderer component not found!");
            return;
        }

        // Create URP Lit material
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("[GlowingCube] URP Lit shader not found! Is URP installed?");
            return;
        }

        cubeMaterial = new Material(urpLit);

        // Set base color
        cubeMaterial.SetColor("_BaseColor", baseColor);

        // Enable emission (critical for bloom)
        cubeMaterial.EnableKeyword("_EMISSION");
        cubeMaterial.SetFloat("_EmissionEnabled", 1f);
        cubeMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // Set initial emission (HDR values > 1 trigger bloom)
        cubeMaterial.SetColor("_EmissionColor", baseColor * minIntensity);

        cubeRenderer.material = cubeMaterial;
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;

        // Create camera if none exists
        if (cam == null)
        {
            Debug.LogWarning("[GlowingCube] No main camera found, creating one");
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        // Position camera to view the cube
        cam.transform.position = new Vector3(0f, 1f, -3f);
        cam.transform.LookAt(transform.position);

        // Black background for contrast
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        // Enable HDR (required for bloom to work)
        cam.allowHDR = true;

        Debug.Log("[GlowingCube] Camera configured with HDR enabled");
    }

    void SetupPostProcessing()
    {
        // Create global volume for bloom
        GameObject volumeObj = new GameObject("GlowingCube_PostProcess");
        volumeObj.transform.SetParent(transform); // Parent to cube for cleanup

        Volume volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100; // High priority to override defaults

        // Create volume profile
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Add and configure bloom
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.Override(2f);      // Strong bloom effect
        bloom.threshold.Override(0.5f);    // Lower threshold so emission triggers bloom
        bloom.scatter.Override(0.7f);      // Nice spread
        bloom.tint.Override(Color.white);  // Neutral tint

        volume.profile = profile;

        Debug.Log("[GlowingCube] Bloom post-processing enabled (intensity: 2, threshold: 0.5)");
    }

    void Update()
    {
        if (cubeMaterial == null) return;

        // Pulse emission intensity using sine wave
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0 to 1
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        // Update emission color (HDR values trigger bloom)
        cubeMaterial.SetColor("_EmissionColor", baseColor * intensity);

        // Slow rotation for visual interest
        transform.Rotate(Vector3.up, 30f * Time.deltaTime);
    }

    void OnDestroy()
    {
        // Clean up material to prevent memory leak
        if (cubeMaterial != null)
        {
            Destroy(cubeMaterial);
        }
    }
}
