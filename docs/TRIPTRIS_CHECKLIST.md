# TripTris Implementation Checklist

## Phase 1: Core Grid & Blocks + Input + UI
- [x] Create folder structure (Scripts/Core, Scripts/Blocks, etc.)
- [x] Create BlockColors.cs with 4 jewel color definitions
- [x] Create GridManager.cs with 8x16 grid array
- [x] Create Block.cs component for individual blocks
- [x] Create BlockMaterial.cs for URP glow materials
- [x] Create Block prefab with cube mesh (runtime creation)
- [x] Create BlockSpawner.cs to spawn blocks at top
- [x] Implement basic gravity (blocks fall down)
- [x] Create PlayerInput.cs for keyboard controls (A/D/S/Space)
- [x] Create GameUI.cs - top corner UI showing blocks & rows
- [x] Create GameManager.cs to coordinate everything
- [x] Create TripTrisBootstrap.cs for runtime scene setup
- [ ] Test: Block spawns, falls, can be moved with keyboard

## Phase 2: Player Controls
- [ ] Create PlayerInput.cs for input handling
- [ ] Implement left movement (A / Left Arrow)
- [ ] Implement right movement (D / Right Arrow)
- [ ] Implement soft drop (S / Down Arrow)
- [ ] Implement hard drop (Space)
- [ ] Add wall collision (can't move outside grid)
- [ ] Add block collision (can't move through blocks)
- [ ] Add lock delay (0.3s after landing)
- [ ] Test: Move block left/right, verify collisions

## Phase 3: Row Clearing
- [ ] Implement IsRowComplete() check
- [ ] Implement IsRowSameColor() check
- [ ] Create RowClearEffect.cs for flash animation
- [ ] Implement flash effect (blocks glow bright)
- [ ] Implement dissolve effect (blocks fade out)
- [ ] Remove cleared blocks from grid
- [ ] Make blocks above fall down
- [ ] Test: Fill row with same color, verify clear

## Phase 4: Game Loop
- [ ] Create GameManager.cs (game states)
- [ ] Create ScoreManager.cs
- [ ] Implement scoring (100 × level per row)
- [ ] Implement combo multiplier
- [ ] Implement level progression
- [ ] Implement speed increase per level
- [ ] Implement game over detection (blocks at top)
- [ ] Add restart functionality
- [ ] Test: Play through multiple levels

## Phase 5: Polish
- [ ] Set up camera (front view, slight angle)
- [ ] Add grid visualization (subtle lines)
- [ ] Add background plane/environment
- [ ] Create particle effects for clearing
- [ ] Add ghost piece (preview where block lands)
- [ ] Create GameUI.cs (score, level display)
- [ ] Create GameOverUI.cs
- [ ] Test: Full game playthrough

## UniFlow Tests
- [ ] Create block_spawn_test.json
- [ ] Create movement_test.json
- [ ] Create row_clear_test.json
- [ ] Create full_game_test.json

---

## Progress Summary
**Phase 1**: [x] COMPLETE (11/12 items)
**Phase 2**: [ ] Not Started
**Phase 3**: [ ] Not Started
**Phase 4**: [ ] Not Started
**Phase 5**: [ ] Not Started

**Overall**: ~20% Complete

## Files Created (Phase 1)
- `Assets/Scripts/Core/BlockColors.cs` - 4 jewel color definitions (Ruby, Emerald, Sapphire, Amber)
- `Assets/Scripts/Core/GridManager.cs` - 8x16 grid array with row checking
- `Assets/Scripts/Core/GameManager.cs` - Game state, scoring, level progression
- `Assets/Scripts/Core/TripTrisBootstrap.cs` - Runtime scene setup with camera, post-processing, grid visualization
- `Assets/Scripts/Blocks/Block.cs` - Individual block component with color and flash effects
- `Assets/Scripts/Blocks/BlockMaterial.cs` - Shared URP materials with emission for bloom
- `Assets/Scripts/Blocks/BlockSpawner.cs` - Block spawning and falling logic
- `Assets/Scripts/Input/PlayerInput.cs` - Keyboard controls (A/D/Left/Right, S/Down, Space)
- `Assets/Scripts/UI/GameUI.cs` - Top-left corner UI showing Blocks, Rows, Score
