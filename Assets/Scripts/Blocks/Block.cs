using UnityEngine;
using System.Collections;
using TripTris.Core;

namespace TripTris.Blocks
{
    [RequireComponent(typeof(Renderer))]
    public class Block : MonoBehaviour
    {
        public int gridX;
        public int gridY;
        public int colorType;
        public bool isLocked;

        public Color BlockColor => BlockColors.GetColorByIndex(colorType);

        private Renderer blockRenderer;
        private Material instanceMaterial; // Each block gets its own material instance
        private float pulsePhase;
        private bool isFlashing;

        void Awake()
        {
            blockRenderer = GetComponent<Renderer>();
            pulsePhase = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            if (isFlashing || instanceMaterial == null) return;

            // Breathing pulse: oscillates emission intensity
            float pulse = 1.0f + 0.5f * Mathf.Sin(Time.time * 3f + pulsePhase);
            Color baseColor = BlockColors.GetColorByIndex(colorType);
            instanceMaterial.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity * pulse);
        }

        public void SetColor(int colorIndex)
        {
            colorType = colorIndex;

            // Create a unique instance material for this block (enables per-block pulse)
            Material sharedMat = BlockMaterial.Instance.GetMaterial(colorType);
            if (sharedMat != null && blockRenderer != null)
            {
                instanceMaterial = new Material(sharedMat);
                blockRenderer.material = instanceMaterial;
            }
        }

        public void SetGridPosition(int x, int y)
        {
            gridX = x;
            gridY = y;
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void Unlock()
        {
            isLocked = false;
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

        public void ApplyPulse(float intensity)
        {
            if (instanceMaterial != null)
            {
                Color baseColor = BlockColors.GetColorByIndex(colorType);
                instanceMaterial.SetColor("_EmissionColor", baseColor * BlockColors.EmissionIntensity * intensity);
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
