using UnityEngine;

namespace TripTris.Core
{
    /// <summary>
    /// Static class providing jewel color definitions for TripTris blocks.
    /// </summary>
    public static class BlockColors
    {
        public const float EmissionIntensity = 2.5f;

        public static readonly Color Ruby = new Color(0.9f, 0.15f, 0.15f);
        public static readonly Color Emerald = new Color(0.15f, 0.85f, 0.35f);
        public static readonly Color Sapphire = new Color(0.15f, 0.35f, 0.95f);
        public static readonly Color Amber = new Color(1.0f, 0.75f, 0.15f);

        private static readonly Color[] AllColors = new Color[]
        {
            Ruby,
            Emerald,
            Sapphire,
            Amber
        };

        /// <summary>
        /// Gets a random jewel color from the available color palette.
        /// </summary>
        /// <returns>A random Color from the jewel palette.</returns>
        public static Color GetRandomColor()
        {
            return AllColors[Random.Range(0, AllColors.Length)];
        }

        /// <summary>
        /// Gets the number of available colors.
        /// </summary>
        public static int ColorCount => AllColors.Length;

        /// <summary>
        /// Gets a color by index (0-3).
        /// </summary>
        public static Color GetColorByIndex(int index)
        {
            if (index < 0 || index >= AllColors.Length)
            {
                Debug.LogWarning($"Invalid color index {index}. Returning Ruby.");
                return Ruby;
            }
            return AllColors[index];
        }

        /// <summary>
        /// Gets the index of a color, or -1 if not found.
        /// </summary>
        public static int GetColorIndex(Color color)
        {
            for (int i = 0; i < AllColors.Length; i++)
            {
                if (Mathf.Approximately(AllColors[i].r, color.r) &&
                    Mathf.Approximately(AllColors[i].g, color.g) &&
                    Mathf.Approximately(AllColors[i].b, color.b))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
