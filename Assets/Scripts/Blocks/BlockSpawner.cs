using UnityEngine;
using TripTris.Core;

namespace TripTris.Blocks
{
    /// <summary>
    /// Spawns blocks at the top of the play area and handles falling logic.
    /// Manages the active falling block and coordinates with GridManager.
    /// </summary>
    public class BlockSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private int spawnX = 4;  // Center of 8-wide grid (columns 0-7)
        [SerializeField] private int spawnY = 15; // Top of 16-high grid (rows 0-15)
        [SerializeField] private float fallSpeed = 0.8f; // Time in seconds between fall steps (slower for testing)

        [Header("Block Prefab")]
        [SerializeField] private GameObject blockPrefab;

        // Active falling block
        private GameObject activeFallingBlock;
        private Block activeBlockComponent;
        private float fallTimer;

        // Color override for testing (-1 = random)
        private int nextColorOverride = -1;

        // Grid reference (will be set by GridManager or found at runtime)
        private GridManager gridManager;

        void Start()
        {
            // Subscribe to UniFlow commands
            UniFlow.UniFlowController.OnSetColor += HandleSetColor;
            UniFlow.UniFlowController.OnMove += MoveToColumnAndDrop;
            // Find GridManager if not set
            if (gridManager == null)
            {
                gridManager = FindAnyObjectByType<GridManager>();
                if (gridManager == null)
                {
                    Debug.LogWarning("[BlockSpawner] GridManager not found. Block placement validation disabled.");
                }
            }

            // Create default block prefab if none assigned
            if (blockPrefab == null)
            {
                CreateDefaultBlockPrefab();
            }

            // Spawn first block
            SpawnBlock();
        }

        void Update()
        {
            if (activeFallingBlock == null)
            {
                return;
            }

            // Handle falling logic only - PlayerInput handles keyboard controls
            fallTimer += Time.deltaTime;
            if (fallTimer >= fallSpeed)
            {
                fallTimer = 0f;
                MoveBlockDown();
            }
        }

        /// <summary>
        /// Spawns a new block at the top center of the play area.
        /// </summary>
        public void SpawnNewBlock() => SpawnBlock();

        /// <summary>
        /// Spawns a new block at the top center of the play area.
        /// </summary>
        public void SpawnBlock()
        {
            // Check if spawn position is blocked (game over condition)
            if (gridManager != null && !gridManager.IsPositionEmpty(spawnX, spawnY))
            {
                Debug.Log("[BlockSpawner] Spawn position blocked - Game Over!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                return;
            }

            // Clean up previous block if it exists
            if (activeFallingBlock != null)
            {
                Debug.LogWarning("[BlockSpawner] Previous block still active. Destroying.");
                Destroy(activeFallingBlock);
            }

            // Create spawn position from grid coordinates
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

            // Instantiate block
            activeFallingBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
            activeFallingBlock.name = "FallingBlock";
            activeFallingBlock.SetActive(true); // Ensure block is active

            // Get Block component
            activeBlockComponent = activeFallingBlock.GetComponent<Block>();
            if (activeBlockComponent == null)
            {
                activeBlockComponent = activeFallingBlock.AddComponent<Block>();
            }

            // Set color (use override if set, otherwise random)
            // Override persists until cleared with "random" or "-1"
            int colorIndex;
            if (nextColorOverride >= 0 && nextColorOverride < BlockColors.ColorCount)
            {
                colorIndex = nextColorOverride;
                Debug.Log($"[BlockSpawner] Using color override: {colorIndex}");
            }
            else
            {
                colorIndex = Random.Range(0, BlockColors.ColorCount);
            }
            activeBlockComponent.SetColor(colorIndex);

            // Set initial grid position (matches world position now)
            activeBlockComponent.SetGridPosition(spawnX, spawnY);

            fallTimer = 0f;

            Debug.Log($"[BlockSpawner] Spawned block at ({spawnX}, {spawnY}) with color {colorIndex}");
        }

        /// <summary>
        /// Moves the active block down by one row.
        /// </summary>
        private void MoveBlockDown()
        {
            if (activeFallingBlock == null || activeBlockComponent == null)
            {
                return;
            }

            int currentX = activeBlockComponent.gridX;
            int currentY = activeBlockComponent.gridY;
            int newY = currentY - 1;

            // Check if position below is valid
            if (CanMoveTo(currentX, newY))
            {
                // Move block
                activeBlockComponent.SetGridPosition(currentX, newY);
                activeFallingBlock.transform.position = new Vector3(currentX, newY, 0f);
            }
            else
            {
                // Block hit something - lock it in place
                LockBlock();
            }
        }

        /// <summary>
        /// Checks if the block can move to the specified grid position.
        /// </summary>
        private bool CanMoveTo(int x, int y)
        {
            // Check grid boundaries
            if (gridManager != null)
            {
                return gridManager.IsValidPosition(x, y) && gridManager.IsPositionEmpty(x, y);
            }

            // Fallback validation if no GridManager
            // Assume 8x16 grid for now
            if (x < 0 || x >= 8 || y < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Public method to immediately lock the current block (used by hard drop).
        /// </summary>
        public void LockCurrentBlock()
        {
            if (activeFallingBlock != null && activeBlockComponent != null)
            {
                LockBlock();
            }
        }

        /// <summary>
        /// Locks the current block in place and spawns a new one.
        /// </summary>
        private void LockBlock()
        {
            if (activeBlockComponent != null)
            {
                activeBlockComponent.Lock();

                // Notify GridManager to add block to grid
                if (gridManager != null)
                {
                    gridManager.PlaceBlock(activeBlockComponent, activeBlockComponent.gridX, activeBlockComponent.gridY);

                    // Notify GameManager a block was placed
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnBlockPlaced();
                    }
                }

                Debug.Log($"[BlockSpawner] Locked block at ({activeBlockComponent.gridX}, {activeBlockComponent.gridY})");
            }

            // Clear active block reference
            activeFallingBlock = null;
            activeBlockComponent = null;

            // Only spawn new block if game is still playing
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                SpawnBlock();
            }
        }

        /// <summary>
        /// Sets the GridManager reference.
        /// </summary>
        public void SetGridManager(GridManager manager)
        {
            gridManager = manager;
        }

        /// <summary>
        /// Creates a default block prefab if none is assigned.
        /// </summary>
        private void CreateDefaultBlockPrefab()
        {
            blockPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blockPrefab.name = "BlockPrefab";

            // Remove collider from prefab (we'll handle collision via grid)
            Collider col = blockPrefab.GetComponent<Collider>();
            if (col != null)
            {
                DestroyImmediate(col);
            }

            // Add Block component
            blockPrefab.AddComponent<Block>();

            // Hide the prefab template (move it far away instead of deactivating)
            blockPrefab.transform.position = new Vector3(-1000, -1000, -1000);

            Debug.Log("[BlockSpawner] Created default block prefab");
        }

        /// <summary>
        /// Gets the current fall speed.
        /// </summary>
        public float GetFallSpeed()
        {
            return fallSpeed;
        }

        /// <summary>
        /// Sets the fall speed (for difficulty progression).
        /// </summary>
        public void SetFallSpeed(float speed)
        {
            fallSpeed = Mathf.Max(0.1f, speed); // Minimum 0.1s
            Debug.Log($"[BlockSpawner] Fall speed set to {fallSpeed}s");
        }

        /// <summary>
        /// Gets the active falling block (for external systems).
        /// </summary>
        public GameObject GetActiveFallingBlock()
        {
            return activeFallingBlock;
        }

        private void HandleSetColor(string value)
        {
            if (value == "random" || value == "-1")
            {
                nextColorOverride = -1;
                Debug.Log("[BlockSpawner] Color override cleared (random)");
            }
            else if (int.TryParse(value, out int colorIndex))
            {
                nextColorOverride = colorIndex;
                Debug.Log($"[BlockSpawner] Next block color override: {colorIndex}");
            }
        }

        /// <summary>
        /// Instantly move the current block to target column and hard drop.
        /// Called by UniFlow "move" command for AI gameplay.
        /// </summary>
        public void MoveToColumnAndDrop(int targetCol)
        {
            if (activeFallingBlock == null || activeBlockComponent == null)
            {
                Debug.LogWarning("[BlockSpawner] No active block to move");
                return;
            }

            // Clamp to grid bounds
            targetCol = Mathf.Clamp(targetCol, 0, GridManager.GridWidth - 1);

            // Move horizontally
            int currentY = activeBlockComponent.gridY;
            if (CanMoveTo(targetCol, currentY))
            {
                activeBlockComponent.SetGridPosition(targetCol, currentY);
                activeFallingBlock.transform.position = new Vector3(targetCol, currentY, 0f);
            }
            else
            {
                Debug.LogWarning($"[BlockSpawner] Cannot move to column {targetCol}");
                return;
            }

            // Hard drop: find lowest valid Y
            int lowestY = currentY;
            while (CanMoveTo(targetCol, lowestY - 1))
            {
                lowestY--;
            }

            activeBlockComponent.SetGridPosition(targetCol, lowestY);
            activeFallingBlock.transform.position = new Vector3(targetCol, lowestY, 0f);

            Debug.Log($"[BlockSpawner] AI move: col {targetCol}, landed at ({targetCol}, {lowestY})");
            LockBlock();
        }

        void OnDestroy()
        {
            UniFlow.UniFlowController.OnSetColor -= HandleSetColor;
            UniFlow.UniFlowController.OnMove -= MoveToColumnAndDrop;
        }
    }
}
