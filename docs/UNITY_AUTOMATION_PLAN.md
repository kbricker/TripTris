# Unity Automation Workflow Plan

## Current State (2026-02-02)

### What We've Built
- **TripTris**: 8x16 block puzzle game with falling blocks, 4 jewel colors
- **UniFlow Enhancements**: Added compilation detection APIs (not yet validated)
- **UI Positioning**: Fixed - now in top-left corner

### What's Not Working
- **Row clearing**: Test sequence doesn't fill bottom row correctly
- **Hard drop**: Code changes to immediately lock blocks haven't compiled
- **Compilation detection**: New UniFlow features not validated

---

## Key Learnings About Unity Automation

### 1. Unity State Detection

| State | Detection Method |
|-------|------------------|
| Unity Running | Read `Library/EditorInstance.json` → check if `process_id` is alive |
| Edit vs Play Mode | `EditorApplication.isPlaying` or poll `status.json` |
| Compiling | `EditorApplication.isCompiling` or watch `Library/ScriptAssemblies/*.dll` |
| Compilation Done | `[DidReloadScripts]` attribute or `CompilationPipeline.compilationFinished` |

### 2. File Locations (Windows)

```
Editor Log:     C:\Users\kbricker\AppData\Local\Unity\Editor\Editor.log
Assembly DLL:   Library/ScriptAssemblies/Assembly-CSharp.dll
UniFlow Status: uniflow/workspace/status.json
UniFlow Trigger: uniflow/workspace/commands/trigger.json
```

### 3. Compilation Log Patterns

```
[ScriptCompilation] Requested script compilation because:  ← START
Begin MonoManager ReloadAssembly                           ← RELOAD
- Finished resetting the current domain, in X.XXX seconds  ← COMPLETE
Domain Reload Profiling: XXXXms                            ← TIMING
```

### 4. Common Mistakes to Avoid

- ❌ Editing code while Unity is in Play mode
- ❌ Running tests without waiting for compilation
- ❌ Assuming code changes compiled without verification
- ❌ Not checking Unity state before operations

---

## Correct Workflow

### Before Making Code Changes
1. Check `status.json` - ensure Unity is in `idle` state
2. If in Play mode, send `pause` trigger and wait

### After Making Code Changes
1. Send `refresh` trigger to Unity
2. Watch for compilation signals:
   - `compile_complete.signal` file (new UniFlow feature)
   - Or poll `status.json` for `is_compiling: false`
3. **Verify changes compiled** - look for version logs or new behavior
4. Only THEN run tests

### Running Tests
1. Confirm Unity is idle
2. Clear previous output
3. Send `run` trigger
4. Poll for completion or timeout
5. Check results

---

## UniFlow Enhancements Added

### New Trigger Commands
- `recompile` - Request script recompilation
- `force_recompile` - Full recompile with cache clear
- `refresh` - Refresh asset database

### Enhanced Status Fields (in status.json)
```json
{
  "status": "idle",
  "is_compiling": false,
  "is_playing": false,
  "last_compile_time": "2026-02-02T...",
  "assembly_modified": "2026-02-02T...",
  "unity_version": "6000.0.x"
}
```

### New Signal File
- `output/compile_complete.signal` - Written by `[DidReloadScripts]` callback

### Python Bridge Methods
- `trigger_recompile()` / `trigger_force_recompile()`
- `trigger_refresh()`
- `is_compiling()` / `is_playing()`
- `wait_for_compile()` / `wait_for_idle()`
- `get_compile_signal()` / `wait_for_compile_signal()`

---

## Next Steps

### Immediate (Next Session)
1. [ ] Validate UniFlow compilation detection works
2. [ ] Fix hard drop to immediately lock blocks
3. [ ] Create working row-clear test
4. [ ] Verify row clearing works

### Future
- [ ] Add visual effects for row clearing
- [ ] Implement color-match bonus scoring
- [ ] Add game over screen
- [ ] Sound effects

---

## Test Sequences

### row_clear_v2.json
Uses soft drop (hold down key) instead of hard drop to avoid timing issues.

### row_clear_slow.json
Long waits between blocks for natural falling.

### triptris_test.json
Basic game verification - blocks spawn and fall.

---

## Repository Info

- **TripTris**: git@github.com:kbricker/TripTris.git
- **UniFlow**: Local at d:\kbricker\projects\unity\uniflow
