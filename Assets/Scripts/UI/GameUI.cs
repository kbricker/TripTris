// Force recompile v4 - TOP LEFT POSITIONING
using UnityEngine;
using UnityEngine.UI;
using TripTris.Core;

namespace TripTris.UI
{
    /// <summary>
    /// Handles the in-game UI display for TripTris.
    /// Shows score, level, blocks placed, and rows cleared.
    /// Creates UI elements at runtime with a framed panel design.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        // Version tracking - this MUST change for recompilation
        private const string UI_VERSION = "v5-larger-panel";

        [Header("UI Elements")]
        private Text titleText;
        private Text scoreText;
        private Text levelText;
        private Text blocksText;
        private Text rowsText;
        private Text gameOverText;

        [Header("UI Settings")]
        [SerializeField] private int titleFontSize = 36;
        [SerializeField] private int fontSize = 28;
        [SerializeField] private Color titleColor = new Color(1f, 0.8f, 0.2f); // Gold
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        [SerializeField] private Color borderColor = new Color(0.3f, 0.5f, 1f, 1f); // Blue border

        [Header("Layout")]
        [SerializeField] private float edgeOffset = 20f;  // Offset from screen edge
        [SerializeField] private float padding = 20f;     // Internal padding
        [SerializeField] private float lineSpacing = 36f;
        [SerializeField] private float borderWidth = 3f;

        private GameObject uiPanel;
        private GameObject borderPanel;

        void Start()
        {
            CreateUI();
            UpdateUI();
        }

        void Update()
        {
            UpdateUI();
        }

        /// <summary>
        /// Creates the UI elements at runtime with a bordered panel.
        /// </summary>
        private void CreateUI()
        {
            // Find the Canvas to parent directly to it
            Canvas canvas = GetComponentInParent<Canvas>();
            Transform parentTransform = canvas != null ? canvas.transform : transform;

            // Create outer border panel
            borderPanel = new GameObject("UI Border");
            borderPanel.transform.SetParent(parentTransform, false);

            RectTransform borderRect = borderPanel.AddComponent<RectTransform>();
            Image borderImage = borderPanel.AddComponent<Image>();
            borderImage.color = borderColor;

            // Position border panel in upper-left corner of the canvas
            borderRect.anchorMin = new Vector2(0f, 1f);
            borderRect.anchorMax = new Vector2(0f, 1f);
            borderRect.pivot = new Vector2(0f, 1f);
            borderRect.anchoredPosition = new Vector2(edgeOffset, -edgeOffset);
            borderRect.sizeDelta = new Vector2(240f, 220f);  // Enlarged panel

            // Create inner background panel
            uiPanel = new GameObject("UI Panel");
            uiPanel.transform.SetParent(borderPanel.transform, false);

            RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
            Image panelImage = uiPanel.AddComponent<Image>();
            panelImage.color = backgroundColor;

            // Fill border with small inset for border effect
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(borderWidth, borderWidth);
            panelRect.offsetMax = new Vector2(-borderWidth, -borderWidth);

            // Create text elements
            float yOffset = -padding;

            // Title
            titleText = CreateTextElement("Title Text", yOffset, titleFontSize, titleColor, TextAnchor.MiddleCenter);
            titleText.text = "TRIPTRIS";
            titleText.fontStyle = FontStyle.Bold;
            yOffset -= lineSpacing + 5f;

            // Score (prominent)
            scoreText = CreateTextElement("Score Text", yOffset, fontSize + 2, textColor, TextAnchor.MiddleLeft);
            yOffset -= lineSpacing;

            // Level
            levelText = CreateTextElement("Level Text", yOffset, fontSize, textColor, TextAnchor.MiddleLeft);
            yOffset -= lineSpacing;

            // Blocks
            blocksText = CreateTextElement("Blocks Text", yOffset, fontSize - 2, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleLeft);
            yOffset -= lineSpacing - 3f;

            // Rows
            rowsText = CreateTextElement("Rows Text", yOffset, fontSize - 2, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleLeft);

            // Game Over overlay - large centered text, hidden by default
            GameObject goObj = new GameObject("Game Over Text");
            goObj.transform.SetParent(parentTransform, false);
            RectTransform goRect = goObj.AddComponent<RectTransform>();
            goRect.anchorMin = new Vector2(0.5f, 0.5f);
            goRect.anchorMax = new Vector2(0.5f, 0.5f);
            goRect.pivot = new Vector2(0.5f, 0.5f);
            goRect.anchoredPosition = Vector2.zero;
            goRect.sizeDelta = new Vector2(600f, 200f);

            gameOverText = goObj.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 80);
            gameOverText.font = font;
            gameOverText.fontSize = 80;
            gameOverText.color = new Color(1f, 0.2f, 0.2f);
            gameOverText.alignment = TextAnchor.MiddleCenter;
            gameOverText.fontStyle = FontStyle.Bold;
            gameOverText.text = "GAME OVER";
            gameOverText.horizontalOverflow = HorizontalWrapMode.Overflow;
            gameOverText.verticalOverflow = VerticalWrapMode.Overflow;

            Outline goOutline = goObj.AddComponent<Outline>();
            goOutline.effectColor = Color.black;
            goOutline.effectDistance = new Vector2(3f, -3f);

            goObj.SetActive(false); // hidden until game over

            Debug.Log("[GameUI] UI created successfully");
        }

        /// <summary>
        /// Creates a single text element with customizable styling.
        /// </summary>
        private Text CreateTextElement(string name, float yOffset, int size, Color color, TextAnchor alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(uiPanel.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            Text textComponent = textObj.AddComponent<Text>();

            // Configure RectTransform
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, yOffset);
            textRect.sizeDelta = new Vector2(-padding * 2f, lineSpacing);

            // Configure Text component - try multiple font loading methods
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", size);

            textComponent.font = font;
            textComponent.fontSize = size;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;

            // Add outline for better readability
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return textComponent;
        }

        /// <summary>
        /// Updates all UI text elements with current game stats.
        /// </summary>
        public void UpdateUI()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {GameManager.Instance.Score}";
            }

            if (levelText != null)
            {
                levelText.text = $"Level: {GameManager.Instance.Level}";
            }

            if (blocksText != null)
            {
                blocksText.text = $"Blocks: {GameManager.Instance.BlocksPlaced}";
            }

            if (rowsText != null)
            {
                rowsText.text = $"Rows: {GameManager.Instance.RowsCleared}";
            }

            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(
                    GameManager.Instance.CurrentState == GameManager.GameState.GameOver);
            }
        }

        /// <summary>
        /// Shows the UI panel.
        /// </summary>
        public void Show()
        {
            if (borderPanel != null)
            {
                borderPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the UI panel.
        /// </summary>
        public void Hide()
        {
            if (borderPanel != null)
            {
                borderPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the text color.
        /// </summary>
        public void SetTextColor(Color color)
        {
            textColor = color;

            if (scoreText != null) scoreText.color = color;
            if (levelText != null) levelText.color = color;
            if (blocksText != null) blocksText.color = color;
            if (rowsText != null) rowsText.color = color;
        }

        /// <summary>
        /// Sets the background color.
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;

            if (uiPanel != null)
            {
                Image panelImage = uiPanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = color;
                }
            }
        }
    }
}
