using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexTris
{
    public class HexGameManager : MonoBehaviour
    {
        public enum GameState { Playing, Paused, GameOver }

        private static HexGameManager instance;
        public static HexGameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindAnyObjectByType<HexGameManager>();
                return instance;
            }
        }

        [SerializeField] private GameState currentState = GameState.Playing;

        private HexGrid grid;
        private HexSpawner spawner;

        private int score;
        private int level = 1;
        private int blocksPlaced;
        private int linesCleared;
        private bool isClearing; // blocks new placements during clear animation

        private const int PointsPerBlock = 10;
        private const int BlocksPerLevel = 10;

        public GameState CurrentState => isClearing ? GameState.Paused : currentState;
        public int Score => score;
        public int Level => level;
        public int BlocksPlaced => blocksPlaced;
        public int LinesCleared => linesCleared;
        public HexSpawner Spawner => spawner;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            grid = FindAnyObjectByType<HexGrid>();
            spawner = FindAnyObjectByType<HexSpawner>();
        }

        void Update()
        {
            if (grid == null) grid = FindAnyObjectByType<HexGrid>();
            if (spawner == null) spawner = FindAnyObjectByType<HexSpawner>();
        }

        public void OnBlockPlaced()
        {
            if (currentState != GameState.Playing || isClearing) return;

            blocksPlaced++;
            score += PointsPerBlock;
            CheckLevelUp();
            CheckForClears();
        }

        private void CheckForClears()
        {
            if (grid == null) return;

            var clears = grid.CheckAllClears();
            if (clears.Count > 0)
            {
                StartCoroutine(AnimateClear(clears));
            }
        }

        private IEnumerator AnimateClear(List<(List<(int col, int row)> positions, string type, int points)> clears)
        {
            isClearing = true;

            var allPositions = new HashSet<(int, int)>();
            int totalPoints = 0;

            foreach (var clear in clears)
            {
                totalPoints += clear.points;
                linesCleared++;
                Debug.Log($"[HexTris] {clear.type} clear! +{clear.points} pts ({clear.positions.Count} blocks)");
                foreach (var pos in clear.positions)
                    allPositions.Add(pos);
            }

            score += totalPoints;

            // Collect blocks and scale them up briefly as highlight
            var clearBlocks = new List<HexBlock>();
            foreach (var pos in allPositions)
            {
                var block = grid.GetBlockAt(pos.Item1, pos.Item2);
                if (block != null)
                {
                    clearBlocks.Add(block);
                    // Scale up for highlight effect
                    block.transform.localScale = Vector3.one * 1.3f;
                }
            }

            ShowPointsToast(totalPoints, clears[0].type);

            // Hold highlight
            yield return new WaitForSeconds(0.4f);

            // Shrink and fade
            float fadeTime = 0.25f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                float scale = Mathf.Lerp(1.3f, 0f, t);
                foreach (var block in clearBlocks)
                {
                    if (block != null)
                        block.transform.localScale = Vector3.one * scale;
                }
                yield return null;
            }

            // Remove and collapse
            foreach (var pos in allPositions)
            {
                var block = grid.Remove(pos.Item1, pos.Item2);
                if (block != null)
                    Destroy(block.gameObject);
            }

            grid.CollapseAll();
            Debug.Log($"[HexTris] Cleared {clears.Count} line(s). Score: {score}");

            isClearing = false;

            // Spawn new block — LockBlock skipped this because isClearing was true
            if (spawner != null && currentState == GameState.Playing)
            {
                spawner.SpawnBlock();
                Debug.Log("[HexTris] Post-clear spawn");
            }
        }

        private void ShowPointsToast(int points, string type)
        {
            // Find or create canvas
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Create toast text
            var toastObj = new GameObject("PointsToast");
            toastObj.transform.SetParent(canvas.transform, false);

            var rect = toastObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 50f);
            rect.sizeDelta = new Vector2(400f, 100f);

            var text = toastObj.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 60);
            text.font = font;
            text.fontSize = 60;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.fontStyle = FontStyle.Bold;

            string label = type == "horizontal" ? "ROW CLEAR!" : "DIAGONAL!";
            text.text = $"{label}\n+{points}";
            text.color = type == "horizontal"
                ? new Color(1f, 0.9f, 0.2f) // gold
                : new Color(0.3f, 1f, 0.5f); // green

            var outline = toastObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3f, -3f);

            // Animate and destroy
            StartCoroutine(AnimateToast(toastObj));
        }

        private IEnumerator AnimateToast(GameObject toast)
        {
            var rect = toast.GetComponent<RectTransform>();
            var text = toast.GetComponent<Text>();
            float duration = 1.5f;
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Float upward
                rect.anchoredPosition = startPos + new Vector2(0, t * 80f);

                // Fade out in last 40%
                if (t > 0.6f)
                {
                    float fadeT = (t - 0.6f) / 0.4f;
                    Color c = text.color;
                    c.a = 1f - fadeT;
                    text.color = c;
                }

                yield return null;
            }

            Destroy(toast);
        }

        private void CheckLevelUp()
        {
            int newLevel = (blocksPlaced / BlocksPerLevel) + 1;
            if (newLevel > level)
            {
                level = newLevel;
                float newSpeed = 0.8f - (level - 1) * 0.07f;
                if (spawner != null)
                    spawner.SetFallSpeed(newSpeed);
                Debug.Log($"[HexTris] Level {level}, speed: {newSpeed:F2}s");
            }
        }

        public void GameOver()
        {
            if (currentState != GameState.GameOver)
            {
                currentState = GameState.GameOver;
                Debug.Log($"[HexTris] Game Over! Score: {score}");
            }
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
            }
            else if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1f;
            }
        }

        void OnApplicationQuit()
        {
            Time.timeScale = 1f;
        }
    }
}
