# TripTris Game Plan
## Color-Matching Falling Block Puzzle Game

---

## Game Overview

**TripTris** is a 3D falling block puzzle game where single blocks fall from the top. Players move blocks left/right to create full rows of the same color. When a complete row is filled with matching colors, it clears and blocks above fall down.

### Core Mechanics
- **Grid**: 8 columns × 16 rows
- **Blocks**: Single cubes (one at a time)
- **Colors**: 4 jewel tones (Ruby, Emerald, Sapphire, Amber)
- **Clearing**: Full row of SAME color clears
- **Movement**: Left/Right only (2D in 3D space)
- **Visual**: Glowing blocks with bloom, flash & dissolve on clear

---

## Game Design Specifications

### Grid System
```
Width:  8 columns (0-7)
Height: 16 rows (0-15)
Block Size: 1 Unity unit
Origin: Bottom-left at (0, 0, 0)
```

### Block Colors (Jewel Tones)
| Color | Name | RGB Values | Emission Intensity |
|-------|------|------------|-------------------|
| Ruby | Red | (0.9, 0.15, 0.15) | 2.5 |
| Emerald | Green | (0.15, 0.85, 0.35) | 2.5 |
| Sapphire | Blue | (0.15, 0.35, 0.95) | 2.5 |
| Amber | Yellow | (1.0, 0.75, 0.15) | 2.5 |

### Controls
- **A/Left Arrow**: Move block left
- **D/Right Arrow**: Move block right
- **S/Down Arrow**: Soft drop (faster fall)
- **Space**: Hard drop (instant placement)
- **Q/E**: Cycle block color (optional feature)

### Speed Progression
| Level | Fall Speed | Rows/Second |
|-------|-----------|-------------|
| 1-3 | Slow | 0.5 |
| 4-6 | Medium | 1.0 |
| 7-9 | Fast | 1.5 |
| 10+ | Expert | 2.0+ |

### Scoring
- Row clear: 100 × level
- Combo (consecutive): 1.5× multiplier
- Hard drop bonus: +2 points per row dropped

---

## Architecture

### Scripts to Create

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs          # Main game controller
│   ├── GridManager.cs          # Grid state & collision
│   └── ScoreManager.cs         # Score tracking
├── Blocks/
│   ├── Block.cs                # Individual block behavior
│   ├── BlockSpawner.cs         # Spawns new blocks
│   └── BlockColors.cs          # Color definitions
├── Effects/
│   ├── BlockMaterial.cs        # URP material management
│   ├── RowClearEffect.cs       # Flash & dissolve effect
│   └── GlowPulse.cs            # Emission pulsing
├── Input/
│   └── PlayerInput.cs          # Input handling
└── UI/
    ├── GameUI.cs               # Score, level display
    └── GameOverUI.cs           # Game over screen
```

### Key Components

#### 1. GridManager
- 2D array tracking block positions: `Block[8, 16]`
- Check if position is valid/empty
- Check for complete same-color rows
- Handle row clearing and block falling

#### 2. Block
- Stores color type (0-3)
- References material for glow
- Handles landing detection
- Flash effect on clear

#### 3. BlockSpawner
- Spawns blocks at top center (column 3 or 4)
- Random color selection
- Tracks active falling block

#### 4. GameManager
- Game state (Playing, Paused, GameOver)
- Level progression
- Coordinates all systems

---

## Implementation Phases

### Phase 1: Core Grid & Blocks
1. Create GridManager with 8x16 array
2. Create Block prefab with glowing material
3. Implement BlockSpawner
4. Basic falling mechanic (gravity)

### Phase 2: Player Controls
1. Implement left/right movement
2. Implement soft drop
3. Implement hard drop
4. Collision detection with walls/blocks

### Phase 3: Row Clearing
1. Detect complete rows
2. Check if row is same color
3. Flash effect on matching blocks
4. Dissolve/remove blocks
5. Drop blocks above cleared row

### Phase 4: Game Loop
1. Score system
2. Level progression
3. Speed increase
4. Game over detection
5. Restart functionality

### Phase 5: Polish
1. Camera setup (3D view from front)
2. Background/environment
3. Particle effects on clear
4. Sound effects (placeholder)
5. UI elements

---

## Technical Notes

### Material Management
- Use shared materials (1 per color) for efficiency
- Use MaterialPropertyBlock for per-block emission animation
- Bloom post-processing already set up from GlowingCube

### Collision Detection
- Grid-based (no physics engine needed)
- Check grid array before moving
- Lock delay: 0.3s after landing before placing

### Row Clear Logic
```csharp
bool IsRowComplete(int row) {
    for (int x = 0; x < 8; x++)
        if (grid[x, row] == null) return false;
    return true;
}

bool IsRowSameColor(int row) {
    int firstColor = grid[0, row].colorType;
    for (int x = 1; x < 8; x++)
        if (grid[x, row].colorType != firstColor) return false;
    return true;
}
```

---

## UniFlow Test Sequences

### Test 1: Block Spawning
- Verify block appears at top
- Verify block falls
- Verify block lands at bottom

### Test 2: Movement
- Verify left/right movement
- Verify wall collision
- Verify block collision

### Test 3: Row Clearing
- Fill a row with same color
- Verify flash effect
- Verify row clears
- Verify blocks fall

---

## File Structure
```
Assets/
├── Scripts/         # All game scripts
├── Prefabs/         # Block prefab
├── Materials/       # Shared materials (4 colors)
├── Scenes/          # Game scene
└── Resources/       # Runtime-loaded assets
```

---

## Dependencies
- Unity 6.3 LTS
- Universal Render Pipeline (URP) 17.3.0
- Input System 1.18.0
- UniFlow (for testing)
