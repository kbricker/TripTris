using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace HexTris
{
    public class HexPlayerInput : MonoBehaviour
    {
        [SerializeField] private float softDropMultiplier = 5f;

        private HexSpawner spawner;
        private HexGrid grid;
        private bool isSoftDropping;
        private float normalFallSpeed;

        void Start()
        {
            spawner = FindAnyObjectByType<HexSpawner>();
            grid = FindAnyObjectByType<HexGrid>();
        }

        void Update()
        {
            var gm = HexGameManager.Instance;
            if (gm != null && gm.CurrentState != HexGameManager.GameState.Playing) return;

            HandleMovement();
            HandleDrop();
            HandlePause();
        }

        private bool WasKeyPressedThisFrame(Key key)
        {
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard kb)
                {
                    var kc = GetKeyControl(kb, key);
                    if (kc != null && kc.wasPressedThisFrame) return true;
                }
            }
            return false;
        }

        private bool IsKeyPressed(Key key)
        {
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard kb)
                {
                    var kc = GetKeyControl(kb, key);
                    if (kc != null && kc.isPressed) return true;
                }
            }
            return false;
        }

        private KeyControl GetKeyControl(Keyboard kb, Key key)
        {
            return key switch
            {
                Key.A => kb.aKey,
                Key.D => kb.dKey,
                Key.S => kb.sKey,
                Key.Space => kb.spaceKey,
                Key.LeftArrow => kb.leftArrowKey,
                Key.RightArrow => kb.rightArrowKey,
                Key.DownArrow => kb.downArrowKey,
                Key.Escape => kb.escapeKey,
                _ => null
            };
        }

        private void HandleMovement()
        {
            if (WasKeyPressedThisFrame(Key.A) || WasKeyPressedThisFrame(Key.LeftArrow))
                MoveBlock(-1);
            if (WasKeyPressedThisFrame(Key.D) || WasKeyPressedThisFrame(Key.RightArrow))
                MoveBlock(1);
        }

        private void MoveBlock(int direction)
        {
            if (spawner == null || grid == null) return;
            var activeBlock = spawner.GetActiveFallingBlock();
            if (activeBlock == null) return;
            var block = activeBlock.GetComponent<HexBlock>();
            if (block == null) return;

            int newCol = block.col + direction;
            int curRow = block.row;

            if (!grid.IsValid(newCol, curRow) || !grid.IsEmpty(newCol, curRow))
                return;

            // Check that the block has somewhere to go from this column
            var (dropCol, dropRow) = grid.FindDropPosition(newCol, curRow);
            if (dropRow < 0)
                return; // no valid landing spot

            block.SetGridPosition(newCol, curRow);
            activeBlock.transform.position = HexGrid.GridToWorld(newCol, curRow);
            spawner.SetMoveBias(direction);
        }

        private void HandleDrop()
        {
            // Soft drop
            if (IsKeyPressed(Key.S) || IsKeyPressed(Key.DownArrow))
            {
                if (!isSoftDropping && spawner != null)
                {
                    isSoftDropping = true;
                    normalFallSpeed = spawner.GetFallSpeed();
                    spawner.SetFallSpeed(normalFallSpeed / softDropMultiplier);
                }
            }
            else if (isSoftDropping && spawner != null)
            {
                isSoftDropping = false;
                spawner.SetFallSpeed(normalFallSpeed);
            }

            // Hard drop
            if (WasKeyPressedThisFrame(Key.Space))
                HardDrop();
        }

        private void HardDrop()
        {
            if (spawner == null || grid == null) return;
            var activeBlock = spawner.GetActiveFallingBlock();
            if (activeBlock == null) return;
            var block = activeBlock.GetComponent<HexBlock>();
            if (block == null) return;

            // Simulate full drop with column shifts (col 7 → col 6 etc.)
            var (landCol, landRow) = grid.FindDropPosition(block.col, block.row);
            if (landRow < 0) return;
            int col = landCol;

            block.SetGridPosition(col, landRow);
            activeBlock.transform.position = HexGrid.GridToWorld(col, landRow);
            spawner.LockCurrentBlock();
        }

        private void HandlePause()
        {
            if (WasKeyPressedThisFrame(Key.Escape))
            {
                var gm = HexGameManager.Instance;
                if (gm != null) gm.PauseGame();
            }
        }
    }
}
