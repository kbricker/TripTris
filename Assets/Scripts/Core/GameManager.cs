// TripTris Game Manager - Core game controller
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TripTris.Blocks;
using TripTrisPlayerInput = TripTris.Input.PlayerInput;

namespace TripTris.Core
{
    /// <summary>
    /// Main game controller for TripTris.
    /// Manages game state, scoring, and coordinates between systems.
    /// Singleton pattern with automatic instantiation.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Playing,
            Paused,
            GameOver
        }

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Playing;

        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private BlockSpawner blockSpawner;
        [SerializeField] private TripTrisPlayerInput playerInput;

        [Header("Game Stats")]
        [SerializeField] private int score = 0;
        [SerializeField] private int level = 1;
        [SerializeField] private int blocksPlaced = 0;
        [SerializeField] private int rowsCleared = 0;
        [SerializeField] private int colorMatchRowsCleared = 0;

        [Header("Scoring")]
        [SerializeField] private int pointsPerBlock = 10;
        [SerializeField] private int pointsPerRow = 100;
        [SerializeField] private int pointsPerColorMatchRow = 500;
        [SerializeField] private int blocksPerLevel = 10;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int Level => level;
        public int BlocksPlaced => blocksPlaced;
        public int RowsCleared => rowsCleared;
        public int ColorMatchRowsCleared => colorMatchRowsCleared;

        public GridManager GridManager => gridManager;
        public BlockSpawner BlockSpawner => blockSpawner;
        public TripTrisPlayerInput PlayerInput => playerInput;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            FindReferences();
            ResetStats();
        }

        private void FindReferences()
        {
            if (gridManager == null)
            {
                gridManager = FindAnyObjectByType<GridManager>();
            }

            if (blockSpawner == null)
            {
                blockSpawner = FindAnyObjectByType<BlockSpawner>();
            }

            if (playerInput == null)
            {
                playerInput = FindAnyObjectByType<TripTrisPlayerInput>();
            }
        }

        public void StartGame()
        {
            ResetStats();
            currentState = GameState.Playing;

            if (gridManager != null)
            {
                gridManager.ClearGrid();
            }

            if (blockSpawner != null)
            {
                blockSpawner.SpawnNewBlock();
            }

            Debug.Log("Game Started");
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
                Debug.Log("Game Paused");
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1f;
                Debug.Log("Game Resumed");
            }
        }

        public void GameOver()
        {
            if (currentState != GameState.GameOver)
            {
                currentState = GameState.GameOver;
                Debug.Log($"Game Over! Final Score: {score}");
            }
        }

        public void OnBlockPlaced()
        {
            if (currentState != GameState.Playing)
            {
                return;
            }

            blocksPlaced++;
            score += pointsPerBlock;

            CheckLevelUp();
            CheckForCompletedRows();
            CheckGameOver();
        }

        private void CheckForCompletedRows()
        {
            if (gridManager == null)
            {
                return;
            }

            // Collect all color-matched rows (full row of same color)
            var completedRows = new System.Collections.Generic.List<int>();
            for (int y = 0; y < GridManager.GridHeight; y++)
            {
                if (gridManager.IsRowSameColor(y))
                {
                    completedRows.Add(y);
                }
            }

            if (completedRows.Count > 0)
            {
                StartCoroutine(ClearRowsAnimated(completedRows));
            }
        }

        private System.Collections.IEnumerator ClearRowsAnimated(System.Collections.Generic.List<int> rows)
        {
            // Phase 1: Flash all blocks in completed rows simultaneously
            var flashCoroutines = new System.Collections.Generic.List<Coroutine>();
            var allBlocks = new System.Collections.Generic.List<Block>();

            foreach (int y in rows)
            {
                for (int x = 0; x < GridManager.GridWidth; x++)
                {
                    Block block = gridManager.GetBlockAt(x, y);
                    if (block != null)
                    {
                        allBlocks.Add(block);
                        flashCoroutines.Add(StartCoroutine(block.Flash()));
                    }
                }
            }

            // Wait for all flash animations to complete
            foreach (var coroutine in flashCoroutines)
            {
                yield return coroutine;
            }

            // Phase 2: Clear rows and update score (process from top to bottom to handle index shifting)
            int rowsClearedThisTurn = 0;
            int colorMatchRowsClearedThisTurn = 0;

            // Sort rows descending so collapse doesn't shift unchecked rows
            rows.Sort((a, b) => b.CompareTo(a));

            foreach (int y in rows)
            {
                Block[] clearedBlocks = gridManager.ClearRow(y);
                foreach (Block block in clearedBlocks)
                {
                    if (block != null)
                    {
                        Destroy(block.gameObject);
                    }
                }

                gridManager.CollapseRowsAbove(y);

                rowsClearedThisTurn++;
                rowsCleared++;
                colorMatchRowsCleared++;
                score += pointsPerColorMatchRow;
                Debug.Log($"[TripTris] Color Match Row {y} Cleared! +{pointsPerColorMatchRow} pts");
            }

            Debug.Log($"[TripTris] Cleared {rowsClearedThisTurn} row(s). Score: {score}");
        }

        private void CheckLevelUp()
        {
            int newLevel = (blocksPlaced / blocksPerLevel) + 1;
            if (newLevel > level)
            {
                level = newLevel;
                // Speed increases each level: 0.8s base, -0.07s per level, min 0.1s
                float newSpeed = 0.8f - (level - 1) * 0.07f;
                if (blockSpawner != null)
                {
                    blockSpawner.SetFallSpeed(newSpeed);
                }
                Debug.Log($"Level Up! Now at level {level}, speed: {newSpeed:F2}s");
            }
        }

        private void CheckGameOver()
        {
            if (gridManager != null && gridManager.IsGridFull())
            {
                GameOver();
            }
        }

        private void ResetStats()
        {
            score = 0;
            level = 1;
            blocksPlaced = 0;
            rowsCleared = 0;
            colorMatchRowsCleared = 0;
        }

        public void AddScore(int points)
        {
            score += points;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                PauseGame();
            }
        }

        private void OnApplicationQuit()
        {
            Time.timeScale = 1f;
        }
    }
}
