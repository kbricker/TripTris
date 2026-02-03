using UnityEngine;
using UnityEngine.Rendering;
using TripTris.Core;

namespace TripTris.Blocks
{
    /// <summary>
    /// Manages shared URP materials for all blocks.
    /// Creates one material per color type with proper emission settings.
    /// Singleton pattern for efficient material sharing.
    /// </summary>
    public class BlockMaterial : MonoBehaviour
    {
        private static BlockMaterial instance;
        public static BlockMaterial Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("BlockMaterialManager");
                    instance = go.AddComponent<BlockMaterial>();
                    DontDestroyOnLoad(go);
                    instance.Initialize();
                }
                return instance;
            }
        }

        // Shared materials (one per color)
        private Material[] materials;

        // Material settings
        private const float BaseMetallic = 0.2f;
        private const float BaseSmoothness = 0.8f;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes all shared materials with URP Lit shader and emission.
        /// </summary>
        private void Initialize()
        {
            materials = new Material[BlockColors.ColorCount];

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[BlockMaterial] URP Lit shader not found! Is URP installed?");
                return;
            }

            // Create one material per color type
            for (int i = 0; i < BlockColors.ColorCount; i++)
            {
                Material mat = new Material(urpLit);
                Color baseColor = BlockColors.GetColorByIndex(i);

                // Set base color
                mat.SetColor("_BaseColor", baseColor);

                // Surface properties for jewel-like appearance
                mat.SetFloat("_Metallic", BaseMetallic);
                mat.SetFloat("_Smoothness", BaseSmoothness);

                // Enable emission (critical for bloom)
                mat.EnableKeyword("_EMISSION");
                mat.SetFloat("_EmissionEnabled", 1f);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                // Set emission color (HDR values > 1 trigger bloom)
                mat.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity);

                // Store material
                materials[i] = mat;

                Debug.Log($"[BlockMaterial] Created material {i}: {GetColorName(i)}");
            }
        }

        /// <summary>
        /// Gets a shared material for the specified color type.
        /// </summary>
        /// <param name="colorType">Color index (0-3)</param>
        /// <returns>Shared material for the color</returns>
        public Material GetMaterial(int colorType)
        {
            if (materials == null || materials.Length == 0)
            {
                Debug.LogWarning("[BlockMaterial] Materials not initialized");
                Initialize();
            }

            if (colorType < 0 || colorType >= materials.Length)
            {
                Debug.LogWarning($"[BlockMaterial] Invalid color type {colorType}. Using 0.");
                colorType = 0;
            }

            return materials[colorType];
        }

        /// <summary>
        /// Applies a pulse effect to a material by temporarily increasing emission.
        /// </summary>
        /// <param name="mat">Material to pulse</param>
        /// <param name="intensity">Pulse intensity multiplier</param>
        public void ApplyPulse(Material mat, float intensity)
        {
            if (mat == null) return;

            // Get base color from material
            Color baseColor = mat.GetColor("_BaseColor");

            // Apply pulse to emission
            float pulseIntensity = BlockColors.EmissionIntensity * intensity;
            mat.SetColor("_EmissionColor", baseColor * pulseIntensity);
        }

        /// <summary>
        /// Resets emission to default for a material.
        /// </summary>
        /// <param name="mat">Material to reset</param>
        public void ResetEmission(Material mat)
        {
            if (mat == null) return;

            Color baseColor = mat.GetColor("_BaseColor");
            mat.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity);
        }

        /// <summary>
        /// Gets all materials (for debugging/inspection).
        /// </summary>
        public Material[] GetAllMaterials()
        {
            return materials;
        }

        /// <summary>
        /// Gets color name for debugging.
        /// </summary>
        private string GetColorName(int colorType)
        {
            switch (colorType)
            {
                case 0: return "Ruby";
                case 1: return "Emerald";
                case 2: return "Sapphire";
                case 3: return "Amber";
                default: return "Unknown";
            }
        }

        void OnDestroy()
        {
            // Clean up materials to prevent memory leak
            if (materials != null)
            {
                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }
    }
}
