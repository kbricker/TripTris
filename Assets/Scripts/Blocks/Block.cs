using UnityEngine;
using System.Collections;
using TripTris.Core;

namespace TripTris.Blocks
{
    /// <summary>
    /// Individual block component that represents a single cube in the game grid.
    /// Handles color, position, locking state, and visual effects.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class Block : MonoBehaviour
    {
        // Grid position
        public int gridX;
        public int gridY;

        // Block state
        public int colorType; // 0-3 (Ruby, Emerald, Sapphire, Amber)
        public bool isLocked; // True when block is placed on grid

        /// <summary>
        /// Gets the block's current color based on colorType index.
        /// </summary>
        public Color BlockColor => BlockColors.GetColorByIndex(colorType);

        // Visual components
        private Renderer blockRenderer;
        private Material blockMaterial;

        // Flash effect settings
        private readonly float flashDuration = 0.4f;
        private readonly float flashIntensity = 8f;

        void Awake()
        {
            blockRenderer = GetComponent<Renderer>();
        }

        /// <summary>
        /// Sets the block's color and applies jewel-like emission.
        /// </summary>
        /// <param name="colorIndex">Color type index (0-3)</param>
        public void SetColor(int colorIndex)
        {
            colorType = colorIndex;

            // Get material from BlockMaterial manager
            blockMaterial = BlockMaterial.Instance.GetMaterial(colorType);
            if (blockMaterial != null && blockRenderer != null)
            {
                blockRenderer.material = blockMaterial;
            }
            else
            {
                Debug.LogWarning($"[Block] Failed to set material for color type {colorType}");
            }
        }

        /// <summary>
        /// Sets grid position for this block.
        /// </summary>
        public void SetGridPosition(int x, int y)
        {
            gridX = x;
            gridY = y;
        }

        /// <summary>
        /// Locks the block in place on the grid.
        /// </summary>
        public void Lock()
        {
            isLocked = true;
        }

        /// <summary>
        /// Unlocks the block (for special cases like undo).
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
        }

        /// <summary>
        /// Flash effect for clearing animation. Bright flash then fade out.
        /// </summary>
        public IEnumerator Flash()
        {
            if (blockMaterial == null)
            {
                Debug.LogWarning("[Block] Cannot flash - material is null");
                yield break;
            }

            // Create instance material to avoid affecting all blocks
            Material instanceMaterial = new Material(blockMaterial);
            blockRenderer.material = instanceMaterial;

            Color baseColor = BlockColors.GetColorByIndex(colorType);
            float elapsed = 0f;

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

                // Also fade alpha
                Color color = instanceMaterial.GetColor("_BaseColor");
                color.a = Mathf.Lerp(1f, 0f, t);
                instanceMaterial.SetColor("_BaseColor", color);

                yield return null;
            }

            // Clean up instance material
            Destroy(instanceMaterial);
        }

        /// <summary>
        /// Apply a pulse effect to this block (used for feedback).
        /// </summary>
        public void ApplyPulse(float intensity)
        {
            if (blockMaterial != null)
            {
                BlockMaterial.Instance.ApplyPulse(blockMaterial, intensity);
            }
        }

        void OnDestroy()
        {
            // Clean up instance material if it was created
            if (blockRenderer != null && blockRenderer.material != blockMaterial)
            {
                Destroy(blockRenderer.material);
            }
        }
    }
}
