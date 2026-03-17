using System.Collections.Generic;
using UnityEngine;

namespace HexTris
{
    public class HexGrid : MonoBehaviour
    {
        public const int MaxCols = 8;
        public const int Rows = 16;
        public const float HexSize = 0.5f;

        public static readonly float HexWidth = Mathf.Sqrt(3f) * HexSize;   // ~0.866
        public static readonly float RowSpacing = 1.5f * HexSize;           // 0.75
        public static readonly float OddRowOffset = HexWidth * 0.5f;        // ~0.433

        private HexBlock[,] grid;

        // Precomputed diagonal lists (only diagonals with >= 7 hexes)
        private List<List<(int col, int row)>> neDiagonals;
        private List<List<(int col, int row)>> nwDiagonals;

        void Awake()
        {
            grid = new HexBlock[MaxCols, Rows];
            neDiagonals = ComputeDiagonals(true);
            nwDiagonals = ComputeDiagonals(false);
            Debug.Log($"[HexTris] Grid initialized. NE diags: {neDiagonals.Count}, NW diags: {nwDiagonals.Count}");
        }

        public static int ColCount(int row)
        {
            return (row % 2 == 0) ? 8 : 7;
        }

        public static Vector3 GridToWorld(int col, int row)
        {
            float x = col * HexWidth + (row % 2 == 1 ? OddRowOffset : 0f);
            float y = row * RowSpacing;
            return new Vector3(x, y, 0f);
        }

        public static (int col, int row) WorldToGrid(Vector3 worldPos)
        {
            int row = Mathf.RoundToInt(worldPos.y / RowSpacing);
            row = Mathf.Clamp(row, 0, Rows - 1);

            float xOffset = (row % 2 == 1) ? OddRowOffset : 0f;
            int col = Mathf.RoundToInt((worldPos.x - xOffset) / HexWidth);
            col = Mathf.Clamp(col, 0, ColCount(row) - 1);

            return (col, row);
        }

        public bool IsValid(int col, int row)
        {
            return row >= 0 && row < Rows && col >= 0 && col < ColCount(row);
        }

        public bool IsEmpty(int col, int row)
        {
            if (!IsValid(col, row)) return false;
            return grid[col, row] == null;
        }

        public void Place(HexBlock block, int col, int row)
        {
            if (!IsValid(col, row)) return;
            grid[col, row] = block;
        }

        public HexBlock Remove(int col, int row)
        {
            if (!IsValid(col, row)) return null;
            var block = grid[col, row];
            grid[col, row] = null;
            return block;
        }

        public HexBlock GetBlockAt(int col, int row)
        {
            if (!IsValid(col, row)) return null;
            return grid[col, row];
        }

        // Cube coordinate conversions for offset (odd-r) hex grid
        private static int OffsetToQ(int col, int row)
        {
            return col - (row - (row & 1)) / 2;
        }

        private static int OffsetToS(int col, int row)
        {
            return -OffsetToQ(col, row) - row;
        }

        // Compute diagonals grouped by cube coordinate (q for NE, s for NW)
        private List<List<(int col, int row)>> ComputeDiagonals(bool isNE)
        {
            var groups = new Dictionary<int, List<(int col, int row)>>();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < ColCount(r); c++)
                {
                    int key = isNE ? OffsetToQ(c, r) : OffsetToS(c, r);
                    if (!groups.ContainsKey(key))
                        groups[key] = new List<(int col, int row)>();
                    groups[key].Add((c, r));
                }
            }

            var result = new List<List<(int col, int row)>>();
            foreach (var kv in groups)
            {
                if (kv.Value.Count >= 3)  // include all diagonals with 3+ hexes
                    result.Add(kv.Value);
            }
            return result;
        }

        public List<(int col, int row)> GetHorizontalLine(int row)
        {
            var line = new List<(int col, int row)>();
            if (row < 0 || row >= Rows) return line;
            for (int c = 0; c < ColCount(row); c++)
                line.Add((c, row));
            return line;
        }

        private bool IsLineSameColor(List<(int col, int row)> line)
        {
            if (line.Count == 0) return false;

            var first = grid[line[0].col, line[0].row];
            if (first == null) return false;

            for (int i = 1; i < line.Count; i++)
            {
                var block = grid[line[i].col, line[i].row];
                if (block == null) return false;
                if (block.colorType != first.colorType) return false;
            }
            return true;
        }

        /// <summary>
        /// Find runs of 3+ consecutive same-color blocks along a diagonal.
        /// Returns all such runs as position lists.
        /// </summary>
        private List<List<(int col, int row)>> FindDiagonalRuns(List<(int col, int row)> diagonal, int minLength = 3)
        {
            var runs = new List<List<(int col, int row)>>();
            if (diagonal.Count < minLength) return runs;

            var currentRun = new List<(int col, int row)>();
            int currentColor = -1;

            foreach (var pos in diagonal)
            {
                var block = grid[pos.col, pos.row];
                if (block != null && block.colorType == currentColor)
                {
                    currentRun.Add(pos);
                }
                else
                {
                    // End current run, check if long enough
                    if (currentRun.Count >= minLength)
                        runs.Add(currentRun);

                    // Start new run
                    currentRun = new List<(int col, int row)>();
                    if (block != null)
                    {
                        currentColor = block.colorType;
                        currentRun.Add(pos);
                    }
                    else
                    {
                        currentColor = -1;
                    }
                }
            }

            // Check final run
            if (currentRun.Count >= minLength)
                runs.Add(currentRun);

            return runs;
        }

        /// <summary>
        /// A diagonal run is valid if it connects the bottom edge to a side wall.
        /// The bottom block (row 0) must NOT be at col 0 or 7 (no corner starts).
        /// The side block (col 0 or 7) must NOT be at row 0 (must be above bottom).
        /// </summary>
        private bool IsDiagonalValid(List<(int col, int row)> run)
        {
            if (run.Count < 3) return false;

            bool hasInteriorBottom = false; // block at row 0 that's NOT on a side wall
            bool hasUpperSide = false;      // block at col 0 or 7 that's NOT on row 0

            foreach (var pos in run)
            {
                if (pos.row == 0 && pos.col > 0 && pos.col < 7)
                    hasInteriorBottom = true;
                if ((pos.col == 0 || pos.col == 7) && pos.row > 0)
                    hasUpperSide = true;
            }

            return hasInteriorBottom && hasUpperSide;
        }

        public List<(List<(int col, int row)> positions, string type, int points)> CheckAllClears()
        {
            var clears = new List<(List<(int col, int row)>, string, int)>();

            // Horizontal rows: full row same color = 500pts
            for (int r = 0; r < Rows; r++)
            {
                var line = GetHorizontalLine(r);
                if (IsLineSameColor(line))
                    clears.Add((line, "horizontal", 500));
            }

            // NE diagonals: 3+ consecutive same color, must touch row 0 AND col 0 or 7
            foreach (var diag in neDiagonals)
            {
                var runs = FindDiagonalRuns(diag);
                foreach (var run in runs)
                {
                    if (IsDiagonalValid(run))
                        clears.Add((run, "NE", run.Count * 100));
                }
            }

            // NW diagonals: same rule
            foreach (var diag in nwDiagonals)
            {
                var runs = FindDiagonalRuns(diag);
                foreach (var run in runs)
                {
                    if (IsDiagonalValid(run))
                        clears.Add((run, "NW", run.Count * 100));
                }
            }

            // 4-diamond: compact rhombus of 4 same-color hexes = 250pts
            var diamondClears = FindDiamondClears();
            foreach (var d in diamondClears)
                clears.Add((d, "diamond", 250));

            // Flower: 6 same-color hexes surrounding 1 different-color center = 1000pts
            var flowerClears = FindFlowerClears();
            foreach (var f in flowerClears)
                clears.Add((f, "flower", 1000));

            return clears;
        }

        /// <summary>
        /// Find 4-hex diamonds. One shape only:
        ///    X       ← shared neighbor above the pair
        ///   X X      ← pair (adjacent, same row)
        ///    X       ← shared neighbor below the pair
        /// All 4 same color = 250 pts.
        /// </summary>
        private List<List<(int col, int row)>> FindDiamondClears()
        {
            var result = new List<List<(int col, int row)>>();
            var found = new HashSet<string>();

            for (int r = 1; r < Rows - 1; r++)
            {
                for (int c = 0; c < ColCount(r) - 1; c++)
                {
                    if (grid[c, r] == null || grid[c + 1, r] == null) continue;
                    int color = grid[c, r].colorType;
                    if (grid[c + 1, r].colorType != color) continue;

                    // Find shared neighbor above (row r+1)
                    int topCol = -1;
                    var n1 = GetNeighbors(c, r);
                    var n2 = GetNeighbors(c + 1, r);
                    foreach (var a in n1)
                    {
                        if (a.row != r + 1) continue;
                        foreach (var b in n2)
                        {
                            if (a.col == b.col && a.row == b.row) { topCol = a.col; break; }
                        }
                        if (topCol >= 0) break;
                    }

                    // Find shared neighbor below (row r-1)
                    int botCol = -1;
                    foreach (var a in n1)
                    {
                        if (a.row != r - 1) continue;
                        foreach (var b in n2)
                        {
                            if (a.col == b.col && a.row == b.row) { botCol = a.col; break; }
                        }
                        if (botCol >= 0) break;
                    }

                    if (topCol < 0 || botCol < 0) continue;
                    if (!IsValid(topCol, r + 1) || !IsValid(botCol, r - 1)) continue;
                    if (grid[topCol, r + 1] == null || grid[botCol, r - 1] == null) continue;
                    if (grid[topCol, r + 1].colorType != color) continue;
                    if (grid[botCol, r - 1].colorType != color) continue;

                    string key = $"{botCol},{r - 1}-{c},{r}-{c + 1},{r}-{topCol},{r + 1}";
                    if (found.Add(key))
                    {
                        result.Add(new List<(int col, int row)> {
                            (botCol, r - 1), (c, r), (c + 1, r), (topCol, r + 1)
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Find flowers: center hex with all 6 neighbors same color (center is different).
        /// Clears the 6 outer hexes (not the center).
        /// </summary>
        private List<List<(int col, int row)>> FindFlowerClears()
        {
            var result = new List<List<(int col, int row)>>();

            for (int r = 1; r < Rows - 1; r++)
            {
                int nc = ColCount(r);
                for (int c = 0; c < nc; c++)
                {
                    if (grid[c, r] == null) continue;
                    int centerColor = grid[c, r].colorType;

                    var neighbors = GetNeighbors(c, r);
                    var validNeighbors = new List<(int col, int row)>();
                    int matchColor = -1;
                    bool allSame = true;

                    foreach (var n in neighbors)
                    {
                        if (!IsValid(n.col, n.row) || grid[n.col, n.row] == null)
                        {
                            allSame = false;
                            break;
                        }
                        int nc2 = grid[n.col, n.row].colorType;
                        if (matchColor < 0) matchColor = nc2;
                        if (nc2 != matchColor)
                        {
                            allSame = false;
                            break;
                        }
                        validNeighbors.Add(n);
                    }

                    // All 6 neighbors must be same color, AND different from center
                    if (allSame && validNeighbors.Count == 6 && matchColor != centerColor)
                    {
                        result.Add(validNeighbors);
                    }
                }
            }
            return result;
        }

        // Collapse all columns. Col 7 stacks on even rows only.
        public void CollapseAll()
        {
            for (int c = 0; c < MaxCols; c++)
                CollapseColumn(c);
        }

        private void CollapseColumn(int col)
        {
            var blocks = new System.Collections.Generic.List<HexBlock>();
            for (int r = 0; r < Rows; r++)
            {
                if (col >= ColCount(r)) continue;
                if (grid[col, r] != null)
                {
                    blocks.Add(grid[col, r]);
                    grid[col, r] = null;
                }
            }

            int blockIdx = 0;
            for (int r = 0; r < Rows && blockIdx < blocks.Count; r++)
            {
                if (col >= ColCount(r)) continue;
                grid[col, r] = blocks[blockIdx];
                blocks[blockIdx].SetGridPosition(col, r);
                blocks[blockIdx].transform.position = GridToWorld(col, r);
                blockIdx++;
            }
        }

        /// <summary>
        /// Simulate dropping a block from (col, fromRow), stepping down one row at a time.
        /// When the column is invalid on the next row (e.g. col 7 on odd row),
        /// the block shifts to the nearest valid column (col 7 → col 6).
        /// Returns the (landCol, landRow) where the block would come to rest.
        /// </summary>
        /// <summary>
        /// Simulate dropping a block from (col, fromRow).
        /// For cols 0-6: descends every row normally.
        /// For col 7: skips odd rows (col 7 doesn't exist there), lands on even rows.
        /// Returns (-1,-1) if block can't fall at all.
        /// </summary>
        public (int col, int row) FindDropPosition(int col, int fromRow = -1)
        {
            if (fromRow < 0) fromRow = Rows - 1;

            int landCol = -1;
            int landRow = -1;

            for (int r = fromRow - 1; r >= 0; r--)
            {
                // Skip rows where this column doesn't exist (col 7 on odd rows)
                if (col >= ColCount(r)) continue;

                if (grid[col, r] == null)
                {
                    landCol = col;
                    landRow = r;
                }
                else
                {
                    break;
                }
            }

            return (landCol, landRow);
        }

        /// <summary>
        /// Simple drop row finder (no column shifting). Used by AI MoveToColumnAndDrop.
        /// </summary>
        public int FindDropRow(int col, int fromRow = -1)
        {
            if (fromRow < 0) fromRow = Rows - 1;
            int landRow = -1;
            for (int r = fromRow; r >= 0; r--)
            {
                if (col >= ColCount(r)) continue;
                if (grid[col, r] == null)
                    landRow = r;
                else
                    return landRow;
            }
            return landRow;
        }

        // ── Hex neighbor helpers ─────────────────────────────────

        /// <summary>
        /// Get the 6 neighbors of a hex position in offset coordinates.
        /// </summary>
        public static List<(int col, int row)> GetNeighbors(int col, int row)
        {
            var neighbors = new List<(int col, int row)>();
            bool even = row % 2 == 0;

            // Even row offsets       // Odd row offsets
            // E:  (c+1, r)           // E:  (c+1, r)
            // W:  (c-1, r)           // W:  (c-1, r)
            // NE: (c,   r+1)         // NE: (c+1, r+1)
            // NW: (c-1, r+1)         // NW: (c,   r+1)
            // SE: (c,   r-1)         // SE: (c+1, r-1)
            // SW: (c-1, r-1)         // SW: (c,   r-1)

            if (even)
            {
                neighbors.Add((col + 1, row));
                neighbors.Add((col - 1, row));
                neighbors.Add((col, row + 1));
                neighbors.Add((col - 1, row + 1));
                neighbors.Add((col, row - 1));
                neighbors.Add((col - 1, row - 1));
            }
            else
            {
                neighbors.Add((col + 1, row));
                neighbors.Add((col - 1, row));
                neighbors.Add((col + 1, row + 1));
                neighbors.Add((col, row + 1));
                neighbors.Add((col + 1, row - 1));
                neighbors.Add((col, row - 1));
            }

            return neighbors;
        }

        public bool IsSpawnBlocked(int col, int row)
        {
            if (!IsValid(col, row)) return true;
            return grid[col, row] != null;
        }

        public void ClearGrid()
        {
            for (int c = 0; c < MaxCols; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    if (grid[c, r] != null)
                    {
                        Object.Destroy(grid[c, r].gameObject);
                        grid[c, r] = null;
                    }
                }
            }
        }
    }
}


