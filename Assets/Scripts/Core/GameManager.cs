// TripTris Game Manager - Core game controller
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
                if (gridManager == null)
                {
                    GameObject gridGO = new GameObject("GridManager");
                    gridManager = gridGO.AddComponent<GridManager>();
                }
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

            int rowsClearedThisTurn = 0;
            int colorMatchRowsClearedThisTurn = 0;

            for (int y = 0; y < GridManager.GridHeight; y++)
            {
                if (gridManager.IsRowComplete(y))
                {
                    bool isColorMatch = gridManager.IsRowSameColor(y);

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

                    if (isColorMatch)
                    {
                        colorMatchRowsClearedThisTurn++;
                        colorMatchRowsCleared++;
                        score += pointsPerColorMatchRow;
                        Debug.Log($"Color Match Row Cleared! Bonus: {pointsPerColorMatchRow} points");
                    }
                    else
                    {
                        score += pointsPerRow;
                    }

                    y--;
                }
            }

            if (rowsClearedThisTurn > 0)
            {
                Debug.Log($"Cleared {rowsClearedThisTurn} row(s). Color matches: {colorMatchRowsClearedThisTurn}");
            }
        }

        private void CheckLevelUp()
        {
            int newLevel = (blocksPlaced / blocksPerLevel) + 1;
            if (newLevel > level)
            {
                level = newLevel;
                Debug.Log($"Level Up! Now at level {level}");
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
