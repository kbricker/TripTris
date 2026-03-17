using UnityEngine;
using UnityEngine.UI;

namespace HexTris
{
    public class HexUI : MonoBehaviour
    {
        private Text titleText;
        private Text scoreText;
        private Text levelText;
        private Text blocksText;
        private Text linesText;
        private Text gameOverText;

        private GameObject borderPanel;
        private GameObject uiPanel;

        private const float EdgeOffset = 20f;
        private const float Padding = 20f;
        private const float LineSpacing = 36f;
        private const float BorderWidth = 3f;

        void Start()
        {
            CreateUI();
        }

        void Update()
        {
            UpdateUI();
        }

        private void CreateUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            // Border panel
            borderPanel = new GameObject("HexUI Border");
            borderPanel.transform.SetParent(parent, false);
            var borderRect = borderPanel.AddComponent<RectTransform>();
            var borderImg = borderPanel.AddComponent<Image>();
            borderImg.color = new Color(0.3f, 0.5f, 1f, 1f);

            borderRect.anchorMin = new Vector2(0f, 1f);
            borderRect.anchorMax = new Vector2(0f, 1f);
            borderRect.pivot = new Vector2(0f, 1f);
            borderRect.anchoredPosition = new Vector2(EdgeOffset, -EdgeOffset);
            borderRect.sizeDelta = new Vector2(240f, 230f);

            // Inner panel
            uiPanel = new GameObject("HexUI Panel");
            uiPanel.transform.SetParent(borderPanel.transform, false);
            var panelRect = uiPanel.AddComponent<RectTransform>();
            var panelImg = uiPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(BorderWidth, BorderWidth);
            panelRect.offsetMax = new Vector2(-BorderWidth, -BorderWidth);

            float y = -Padding;

            // Title
            titleText = CreateText("Title", y, 36, new Color(1f, 0.8f, 0.2f), TextAnchor.MiddleCenter);
            titleText.text = "HEXTRIS";
            titleText.fontStyle = FontStyle.Bold;
            y -= LineSpacing + 5f;

            scoreText = CreateText("Score", y, 30, Color.white, TextAnchor.MiddleLeft);
            y -= LineSpacing;

            levelText = CreateText("Level", y, 28, Color.white, TextAnchor.MiddleLeft);
            y -= LineSpacing;

            blocksText = CreateText("Blocks", y, 26, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleLeft);
            y -= LineSpacing - 3f;

            linesText = CreateText("Lines", y, 26, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleLeft);

            // Game Over overlay
            var goObj = new GameObject("Game Over Text");
            goObj.transform.SetParent(parent, false);
            var goRect = goObj.AddComponent<RectTransform>();
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

            var outline = goObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3f, -3f);
            goObj.SetActive(false);
        }

        private Text CreateText(string name, float yOffset, int size, Color color, TextAnchor align)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(uiPanel.transform, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(-Padding * 2f, LineSpacing);

            var text = obj.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", size);
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var outl = obj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            return text;
        }

        private void UpdateUI()
        {
            var gm = HexGameManager.Instance;
            if (gm == null) return;

            if (scoreText != null) scoreText.text = $"Score: {gm.Score}";
            if (levelText != null) levelText.text = $"Level: {gm.Level}";
            if (blocksText != null) blocksText.text = $"Blocks: {gm.BlocksPlaced}";
            if (linesText != null) linesText.text = $"Lines: {gm.LinesCleared}";
            if (gameOverText != null)
                gameOverText.gameObject.SetActive(gm.CurrentState == HexGameManager.GameState.GameOver);
        }
    }
}
