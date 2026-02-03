// TripTris Player Input - Using Unity Input System
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TripTris.Core;
using TripTris.Blocks;

namespace TripTris.Input
{
    /// <summary>
    /// Handles keyboard input for controlling the active falling block.
    /// Uses Unity's new Input System for keyboard polling.
    /// Supports both physical and virtual keyboards (for UniFlow testing).
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float softDropMultiplier = 5f;

        [Header("References")]
        [SerializeField] private BlockSpawner blockSpawner;
        [SerializeField] private GridManager gridManager;

        private bool isSoftDropping = false;

        void Start()
        {
            FindReferences();
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            HandleMovementInput();
            HandleDropInput();
        }

        /// <summary>
        /// Check if a key was pressed this frame.
        /// Checks all keyboards in the system including virtual ones from UniFlow.
        /// </summary>
        private bool WasKeyPressedThisFrame(Key key)
        {
            // Iterate over all input devices and filter for keyboards
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard kb)
                {
                    var keyControl = GetKeyControl(kb, key);
                    if (keyControl != null && keyControl.wasPressedThisFrame)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a key is currently held.
        /// </summary>
        private bool IsKeyPressed(Key key)
        {
            // Iterate over all input devices and filter for keyboards
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Keyboard kb)
                {
                    var keyControl = GetKeyControl(kb, key);
                    if (keyControl != null && keyControl.isPressed)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get KeyControl for a specific key from a keyboard.
        /// </summary>
        private KeyControl GetKeyControl(Keyboard kb, Key key)
        {
            return key switch
            {
                Key.A => kb.aKey,
                Key.D => kb.dKey,
                Key.S => kb.sKey,
                Key.W => kb.wKey,
                Key.Space => kb.spaceKey,
                Key.LeftArrow => kb.leftArrowKey,
                Key.RightArrow => kb.rightArrowKey,
                Key.UpArrow => kb.upArrowKey,
                Key.DownArrow => kb.downArrowKey,
                Key.Escape => kb.escapeKey,
                _ => null
            };
        }

        private void HandleMovementInput()
        {
            // Move left - A key or Left Arrow
            if (WasKeyPressedThisFrame(Key.A) || WasKeyPressedThisFrame(Key.LeftArrow))
            {
                MoveBlockLeft();
            }

            // Move right - D key or Right Arrow
            if (WasKeyPressedThisFrame(Key.D) || WasKeyPressedThisFrame(Key.RightArrow))
            {
                MoveBlockRight();
            }
        }

        private void HandleDropInput()
        {
            // Soft drop - S key or Down Arrow (hold to accelerate falling)
            if (IsKeyPressed(Key.S) || IsKeyPressed(Key.DownArrow))
            {
                if (!isSoftDropping)
                {
                    SoftDrop();
                }
            }
            else
            {
                if (isSoftDropping)
                {
                    EndSoftDrop();
                }
            }

            // Hard drop - Space (instant drop to bottom)
            if (WasKeyPressedThisFrame(Key.Space))
            {
                HardDrop();
            }
        }

        public void MoveBlockLeft()
        {
            if (blockSpawner == null) return;

            GameObject activeBlock = blockSpawner.GetActiveFallingBlock();
            if (activeBlock == null) return;

            Block blockComponent = activeBlock.GetComponent<Block>();
            if (blockComponent == null) return;

            int currentX = blockComponent.gridX;
            int currentY = blockComponent.gridY;
            int newX = currentX - 1;

            if (IsValidMove(newX, currentY))
            {
                blockComponent.SetGridPosition(newX, currentY);
                activeBlock.transform.position = new Vector3(newX, currentY, 0f);
            }
        }

        public void MoveBlockRight()
        {
            if (blockSpawner == null) return;

            GameObject activeBlock = blockSpawner.GetActiveFallingBlock();
            if (activeBlock == null) return;

            Block blockComponent = activeBlock.GetComponent<Block>();
            if (blockComponent == null) return;

            int currentX = blockComponent.gridX;
            int currentY = blockComponent.gridY;
            int newX = currentX + 1;

            if (IsValidMove(newX, currentY))
            {
                blockComponent.SetGridPosition(newX, currentY);
                activeBlock.transform.position = new Vector3(newX, currentY, 0f);
            }
        }

        public void SoftDrop()
        {
            if (blockSpawner == null) return;

            isSoftDropping = true;
            float currentSpeed = blockSpawner.GetFallSpeed();
            float fastSpeed = currentSpeed / softDropMultiplier;
            blockSpawner.SetFallSpeed(fastSpeed);
        }

        private void EndSoftDrop()
        {
            if (blockSpawner == null) return;

            isSoftDropping = false;
            float fastSpeed = blockSpawner.GetFallSpeed();
            float normalSpeed = fastSpeed * softDropMultiplier;
            blockSpawner.SetFallSpeed(normalSpeed);
        }

        public void HardDrop()
        {
            if (blockSpawner == null) return;

            GameObject activeBlock = blockSpawner.GetActiveFallingBlock();
            if (activeBlock == null) return;

            Block blockComponent = activeBlock.GetComponent<Block>();
            if (blockComponent == null) return;

            int currentX = blockComponent.gridX;
            int currentY = blockComponent.gridY;

            // Find the lowest valid position
            int lowestY = currentY;
            while (IsValidMove(currentX, lowestY - 1))
            {
                lowestY--;
            }

            // Move block to lowest position
            blockComponent.SetGridPosition(currentX, lowestY);
            activeBlock.transform.position = new Vector3(currentX, lowestY, 0f);

            // Immediately lock the block and spawn a new one
            blockSpawner.LockCurrentBlock();
        }

        private bool IsValidMove(int x, int y)
        {
            if (gridManager == null)
            {
                return x >= 0 && x < 8 && y >= 0 && y < 16;
            }

            if (!gridManager.IsValidPosition(x, y)) return false;
            if (!gridManager.IsPositionEmpty(x, y)) return false;

            return true;
        }

        private void FindReferences()
        {
            if (blockSpawner == null)
            {
                blockSpawner = FindAnyObjectByType<BlockSpawner>();
            }

            if (gridManager == null)
            {
                gridManager = FindAnyObjectByType<GridManager>();
            }
        }

        public void SetBlockSpawner(BlockSpawner spawner) => blockSpawner = spawner;
        public void SetGridManager(GridManager manager) => gridManager = manager;
    }
}
