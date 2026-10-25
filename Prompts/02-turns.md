# 02 – Turns

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 68 (914 → 846)

## Prompt

```
Add Temple Run–style 90° turns to the endless runner.

- TrackManager: besides straight segments, sometimes spawn a corner segment that turns left or right (never two corners in a row, at least 3 straight segments between corners). The track continues in the new direction after the corner. Keep removing segments behind the player.
- Player: when the player is inside a corner's turn zone, pressing left/right (A/D, arrows, or swipe) turns the run 90° that way instead of changing lane. Outside a turn zone, left/right changes lanes as now. After turning, keep the same lane index in the new direction.
- If the player reaches the end of a corner without turning the correct way, they run off the edge: Game Over.
- Camera: rotate smoothly to follow the new direction (about 0.3 s), staying behind and above the player.
- Obstacles and coins must still spawn correctly on segments facing any direction.
- Keep all existing features working (lanes, jump, slide, coins, speed-up, UI, Game Over/Restart).
- Explain which scripts you changed and what you added.
- When the code compiles and the scene is set up, stop and give me your summary. Do not enter Play mode, do not write test scripts, and do not run extra verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

New scripts (`Assets/Scripts/`):

| Script | Job |
|---|---|
| `CornerSegment.cs` | A track segment that turns left or right: knows its turn zone, pivot point and the new running direction |
| `CornerTriggerZone.cs` | Tells the player when they enter or leave a corner's turn zone |
| `OffEdgeTrigger.cs` | Game Over when the player runs off the end of a corner without turning |
| `TurnDirection.cs` | Left / Right |

Changed scripts:

| Script | Change |
|---|---|
| `PlayerController.cs` | Left/right turns the run inside a turn zone and changes lane outside it; checks for running off a corner's edge |
| `TrackManager.cs` | Spawns corner segments between straights and continues the track in the new direction |
| `TrackSegment.cs` | Supports segments facing any direction (adds an `IsCorner` flag) |
| `FollowCamera.cs` | Rotates smoothly to the new direction after a turn |
| `GameManager.cs` | Distance now counts the path actually run, not just forward progress |

New prefabs: `Segment_Corner_Left` and `Segment_Corner_Right` in `Assets/Prefabs/Segments/`. The scene (`SampleScene.unity`) was updated to use them.

**Testing notes:** Tested with ▶ Play: it worked. Corners appear, turning left and right works, and the rest of the game (lanes, jump, slide, coins, Game Over / Restart) still works. No fixes needed.
