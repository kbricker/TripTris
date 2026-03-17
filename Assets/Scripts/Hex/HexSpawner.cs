using UnityEngine;
using TripTris.Core;

namespace HexTris
{
    public class HexSpawner : MonoBehaviour
    {
        [SerializeField] private int spawnCol = 3;
        [SerializeField] private int spawnRow = 15;
        [SerializeField] private float fallSpeed = 0.8f;

        private GameObject activeFallingBlock;
        private HexBlock activeBlockComponent;
        private float fallTimer;
        private int nextColorOverride = -1;

        // Movement direction bias: -1=left, 0=neutral, 1=right
        // Used when transitioning between even/odd rows to pick which hex to nestle into
        private int moveBias = 0;

        // Drop preview ghost
        private GameObject ghostBlock;

        private HexGrid grid;

        void Start()
        {
            UniFlow.UniFlowController.OnSetColor += HandleSetColor;
            UniFlow.UniFlowController.OnMove += MoveToColumnAndDrop;

            grid = FindAnyObjectByType<HexGrid>();

            // Create ghost block for drop preview
            CreateGhostBlock();

            SpawnBlock();
        }

        void Update()
        {
            if (activeFallingBlock == null) return;

            var gm = HexGameManager.Instance;
            if (gm != null && gm.CurrentState != HexGameManager.GameState.Playing) return;

            fallTimer += Time.deltaTime;
            if (fallTimer >= fallSpeed)
            {
                fallTimer = 0f;
                MoveBlockDown();
            }

            UpdateGhostPosition();
        }

        public void SpawnBlock()
        {
            if (grid != null && grid.IsSpawnBlocked(spawnCol, spawnRow))
            {
                Debug.Log("[HexTris] Spawn blocked - Game Over!");
                if (HexGameManager.Instance != null)
                    HexGameManager.Instance.GameOver();
                return;
            }

            if (activeFallingBlock != null)
            {
                Destroy(activeFallingBlock);
            }

            activeFallingBlock = new GameObject("FallingHex");
            activeBlockComponent = activeFallingBlock.AddComponent<HexBlock>();

            int colorIndex = (nextColorOverride >= 0 && nextColorOverride < BlockColors.ColorCount)
                ? nextColorOverride
                : Random.Range(0, BlockColors.ColorCount);

            activeBlockComponent.Initialize(colorIndex);
            activeBlockComponent.SetGridPosition(spawnCol, spawnRow);

            Vector3 worldPos = HexGrid.GridToWorld(spawnCol, spawnRow);
            activeFallingBlock.transform.position = worldPos;

            moveBias = 0; // reset direction on new block
            fallTimer = 0f;
            Debug.Log($"[HexTris] Spawned at ({spawnCol},{spawnRow}) color {colorIndex}");
        }

        /// <summary>
        /// Set the movement bias direction. Called by HexPlayerInput on left/right moves.
        /// </summary>
        public void SetMoveBias(int direction)
        {
            moveBias = direction; // -1=left, 1=right
        }

        private void MoveBlockDown()
        {
            if (activeBlockComponent == null) return;

            int curCol = activeBlockComponent.col;
            int curRow = activeBlockComponent.row;

            // Find next valid row below (skip rows where current col doesn't exist)
            int newRow = -1;
            for (int r = curRow - 1; r >= 0; r--)
            {
                if (curCol < HexGrid.ColCount(r))
                {
                    newRow = r;
                    break;
                }
            }

            if (newRow < 0) { LockBlock(); return; }

            if (CanMoveTo(curCol, newRow))
            {
                activeBlockComponent.SetGridPosition(curCol, newRow);
                activeFallingBlock.transform.position = HexGrid.GridToWorld(curCol, newRow);
            }
            else
            {
                LockBlock();
            }
        }

        public bool CanMoveTo(int col, int row)
        {
            if (grid != null)
                return grid.IsValid(col, row) && grid.IsEmpty(col, row);
            return col >= 0 && col < HexGrid.ColCount(row) && row >= 0 && row < HexGrid.Rows;
        }

        public void LockCurrentBlock()
        {
            if (activeFallingBlock != null && activeBlockComponent != null)
                LockBlock();
        }

        private void LockBlock()
        {
            if (activeBlockComponent != null)
            {
                activeBlockComponent.Lock();

                if (grid != null)
                {
                    grid.Place(activeBlockComponent, activeBlockComponent.col, activeBlockComponent.row);

                    if (HexGameManager.Instance != null)
                        HexGameManager.Instance.OnBlockPlaced();
                }

                Debug.Log($"[HexTris] Locked at ({activeBlockComponent.col},{activeBlockComponent.row})");
            }

            activeFallingBlock = null;
            activeBlockComponent = null;

            if (HexGameManager.Instance == null || HexGameManager.Instance.CurrentState == HexGameManager.GameState.Playing)
                SpawnBlock();
        }

        // ── Ghost / Drop Preview ─────────────────────────────────

        private void CreateGhostBlock()
        {
            ghostBlock = HexMesh.CreateHex(HexGrid.HexSize * 0.85f, null,
                new Color(1f, 1f, 1f, 0.15f));
            ghostBlock.name = "DropGhost";

            // Make fill semi-transparent
            var fill = ghostBlock.transform.Find("Hex/Fill");
            if (fill != null)
            {
                var mr = fill.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    var mat = new Material(Shader.Find("Sprites/Default"));
                    mat.color = new Color(1f, 1f, 1f, 0.2f);
                    mr.material = mat;
                }
            }

            ghostBlock.SetActive(false);
        }

        private void UpdateGhostPosition()
        {
            if (ghostBlock == null || activeBlockComponent == null || grid == null)
            {
                if (ghostBlock != null) ghostBlock.SetActive(false);
                return;
            }

            int col = activeBlockComponent.col;
            int curRow = activeBlockComponent.row;

            // Simulate full drop including column shifts (col 7 → col 6 on odd rows)
            var (dropCol, dropRow) = grid.FindDropPosition(col, curRow);

            if (dropRow >= 0)
            {
                ghostBlock.SetActive(true);
                ghostBlock.transform.position = HexGrid.GridToWorld(dropCol, dropRow);
            }
            else
            {
                ghostBlock.SetActive(false);
            }
        }

        // ── Speed / Accessors ────────────────────────────────────

        public void SetFallSpeed(float speed)
        {
            fallSpeed = Mathf.Max(0.1f, speed);
        }

        public float GetFallSpeed() => fallSpeed;

        public GameObject GetActiveFallingBlock() => activeFallingBlock;

        // ── UniFlow Integration ──────────────────────────────────

        private void HandleSetColor(string value)
        {
            if (value == "random" || value == "-1")
                nextColorOverride = -1;
            else if (int.TryParse(value, out int idx))
                nextColorOverride = idx;
        }

        public void MoveToColumnAndDrop(int targetCol)
        {
            if (activeBlockComponent == null) return;

            int landCol = targetCol;
            int landRow = -1;

            if (grid != null)
            {
                landRow = grid.FindDropRow(landCol);
            }
            else
            {
                landRow = 0;
            }

            if (landRow < 0)
            {
                Debug.LogWarning($"[HexTris] No valid position at col {targetCol}");
                return;
            }

            activeBlockComponent.SetGridPosition(landCol, landRow);
            activeFallingBlock.transform.position = HexGrid.GridToWorld(landCol, landRow);

            Debug.Log($"[HexTris] AI move: col {landCol}, landed at ({landCol},{landRow})");
            LockBlock();
        }

        void OnDestroy()
        {
            UniFlow.UniFlowController.OnSetColor -= HandleSetColor;
            UniFlow.UniFlowController.OnMove -= MoveToColumnAndDrop;
            if (ghostBlock != null) Destroy(ghostBlock);
        }
    }
}
