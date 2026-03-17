using UnityEngine;
using TripTris.Core;

namespace HexTris
{
    public class HexMaterialManager : MonoBehaviour
    {
        private static HexMaterialManager instance;
        public static HexMaterialManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("HexMaterialManager");
                    instance = go.AddComponent<HexMaterialManager>();
                    DontDestroyOnLoad(go);
                    instance.Initialize();
                }
                return instance;
            }
        }

        private Material[] materials;

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

        private void Initialize()
        {
            if (materials != null) return;

            materials = new Material[BlockColors.ColorCount];

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[HexTris] URP Lit shader not found!");
                return;
            }

            for (int i = 0; i < BlockColors.ColorCount; i++)
            {
                Material mat = new Material(urpLit);
                Color baseColor = BlockColors.GetColorByIndex(i);

                mat.SetColor("_BaseColor", baseColor);
                mat.SetFloat("_Metallic", 0.2f);
                mat.SetFloat("_Smoothness", 0.8f);
                mat.EnableKeyword("_EMISSION");
                mat.SetFloat("_EmissionEnabled", 1f);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity);

                materials[i] = mat;
            }

            Debug.Log("[HexTris] Materials initialized");
        }

        public Material GetMaterial(int colorType)
        {
            if (materials == null) Initialize();
            if (colorType < 0 || colorType >= materials.Length) colorType = 0;
            return materials[colorType];
        }

        void OnDestroy()
        {
            if (materials != null)
            {
                foreach (var mat in materials)
                {
                    if (mat != null) Destroy(mat);
                }
            }
        }
    }
}
