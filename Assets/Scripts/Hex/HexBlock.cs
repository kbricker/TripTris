using UnityEngine;
using System.Collections;
using TripTris.Core;

namespace HexTris
{
    public class HexBlock : MonoBehaviour
    {
        public int col;
        public int row;
        public int colorType;
        public bool isLocked;

        private MeshRenderer fillRenderer;
        private Material instanceMaterial;
        private float pulsePhase;
        private bool isFlashing;

        void Awake()
        {
            pulsePhase = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            if (isFlashing || instanceMaterial == null) return;

            float pulse = 1.0f + 0.5f * Mathf.Sin(Time.time * 3f + pulsePhase);
            Color baseColor = BlockColors.GetColorByIndex(colorType);
            instanceMaterial.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity * pulse);
        }

        public void Initialize(int colorIndex)
        {
            colorType = colorIndex;
            Color baseColor = BlockColors.GetColorByIndex(colorType);
            Color outlineColor = new Color(0.08f, 0.08f, 0.12f);

            // Create shared material for this color
            Material sharedMat = HexMaterialManager.Instance.GetMaterial(colorType);

            // Create hex visuals
            GameObject hexVisual = HexMesh.CreateHex(HexGrid.HexSize, sharedMat, outlineColor);
            hexVisual.transform.SetParent(transform, false);
            hexVisual.transform.localPosition = Vector3.zero;

            // Get fill renderer and create instance material
            Transform fillTransform = hexVisual.transform.Find("Fill");
            if (fillTransform != null)
            {
                fillRenderer = fillTransform.GetComponent<MeshRenderer>();
                if (fillRenderer != null)
                {
                    instanceMaterial = new Material(sharedMat);
                    fillRenderer.material = instanceMaterial;
                }
            }
        }

        public void SetGridPosition(int c, int r)
        {
            col = c;
            row = r;
        }

        public void Lock()
        {
            isLocked = true;
        }

        public IEnumerator Flash()
        {
            if (instanceMaterial == null) yield break;

            isFlashing = true;
            Color baseColor = BlockColors.GetColorByIndex(colorType);
            float elapsed = 0f;
            float flashDuration = 0.4f;
            float flashIntensity = 8f;

            // Flash bright
            while (elapsed < flashDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (flashDuration * 0.5f);
                float intensity = Mathf.Lerp(BlockColors.EmissionIntensity, flashIntensity, t);
                instanceMaterial.SetColor("_EmissionColor", baseColor * intensity);
                yield return null;
            }

            // Fade out
            elapsed = 0f;
            while (elapsed < flashDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (flashDuration * 0.5f);
                float intensity = Mathf.Lerp(flashIntensity, 0f, t);
                instanceMaterial.SetColor("_EmissionColor", baseColor * intensity);
                Color color = instanceMaterial.GetColor("_BaseColor");
                color.a = Mathf.Lerp(1f, 0f, t);
                instanceMaterial.SetColor("_BaseColor", color);
                yield return null;
            }
        }

        void OnDestroy()
        {
            if (instanceMaterial != null)
            {
                Destroy(instanceMaterial);
            }
        }
    }
}
