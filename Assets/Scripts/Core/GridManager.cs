using UnityEngine;
using TripTris.Blocks;

namespace TripTris.Core
{
    /// <summary>
    /// Manages the 8x16 game grid for TripTris.
    /// Handles block placement, validation, and row checking.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public const int GridWidth = 8;
        public const int GridHeight = 16;

        private Block[,] grid;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Initializes the grid array.
        /// </summary>
        private void InitializeGrid()
        {
            grid = new Block[GridWidth, GridHeight];
        }

        /// <summary>
        /// Clears all blocks from the grid.
        /// </summary>
        public void ClearGrid()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {
                        Destroy(grid[x, y].gameObject);
                        grid[x, y] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a position is within the grid bounds.
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;
        }

        /// <summary>
        /// Checks if a position is empty (no block placed).
        /// </summary>
        public bool IsPositionEmpty(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return false;
            }
            return grid[x, y] == null;
        }

        /// <summary>
        /// Places a block at the specified grid position.
        /// </summary>
        public bool PlaceBlock(Block block, int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogWarning($"Attempted to place block at invalid position ({x}, {y})");
                return false;
            }

            if (!IsPositionEmpty(x, y))
            {
                Debug.LogWarning($"Attempted to place block at occupied position ({x}, {y})");
                return false;
            }

            grid[x, y] = block;
            return true;
        }

        /// <summary>
        /// Removes a block from the specified grid position.
        /// </summary>
        public Block RemoveBlock(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogWarning($"Attempted to remove block from invalid position ({x}, {y})");
                return null;
            }

            Block block = grid[x, y];
            grid[x, y] = null;
            return block;
        }

        /// <summary>
        /// Checks if a row is completely filled with blocks.
        /// </summary>
        public bool IsRowComplete(int row)
        {
            if (row < 0 || row >= GridHeight)
            {
                return false;
            }

            for (int x = 0; x < GridWidth; x++)
            {
                if (grid[x, row] == null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a row is completely filled with blocks of the same color.
        /// </summary>
        public bool IsRowSameColor(int row)
        {
            if (!IsRowComplete(row))
            {
                return false;
            }

            Color firstColor = grid[0, row].BlockColor;
            for (int x = 1; x < GridWidth; x++)
            {
                if (!ColorsMatch(grid[x, row].BlockColor, firstColor))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the block at the specified position, or null if empty.
        /// </summary>
        public Block GetBlockAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }
            return grid[x, y];
        }

        /// <summary>
        /// Clears a specific row and returns the blocks that were removed.
        /// </summary>
        public Block[] ClearRow(int row)
        {
            if (row < 0 || row >= GridHeight)
            {
                return new Block[0];
            }

            Block[] clearedBlocks = new Block[GridWidth];
            for (int x = 0; x < GridWidth; x++)
            {
                clearedBlocks[x] = grid[x, row];
                grid[x, row] = null;
            }
            return clearedBlocks;
        }

        /// <summary>
        /// Moves all blocks above the specified row down by one.
        /// </summary>
        public void CollapseRowsAbove(int clearedRow)
        {
            for (int y = clearedRow; y < GridHeight - 1; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    grid[x, y] = grid[x, y + 1];
                    grid[x, y + 1] = null;

                    // Update block's grid position and transform
                    if (grid[x, y] != null)
                    {
                        grid[x, y].SetGridPosition(x, y);
                        grid[x, y].transform.position = new Vector3(x, y, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if any blocks have reached the top of the grid (game over condition).
        /// </summary>
        public bool IsGridFull()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (grid[x, GridHeight - 1] != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Helper method to compare colors with tolerance.
        /// </summary>
        private bool ColorsMatch(Color a, Color b)
        {
            return Mathf.Approximately(a.r, b.r) &&
                   Mathf.Approximately(a.g, b.g) &&
                   Mathf.Approximately(a.b, b.b);
        }

        /// <summary>
        /// Debug method to visualize the grid state.
        /// </summary>
        public void DebugPrintGrid()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Grid State:");
            for (int y = GridHeight - 1; y >= 0; y--)
            {
                sb.Append($"Row {y:D2}: ");
                for (int x = 0; x < GridWidth; x++)
                {
                    sb.Append(grid[x, y] != null ? "X" : ".");
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }
    }
}
