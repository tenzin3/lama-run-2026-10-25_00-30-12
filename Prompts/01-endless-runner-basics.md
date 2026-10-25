# 01 – Endless runner basics

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model
- **Credits used:** 86 (74 to build it, 12 more on the Assistant's own Play-mode checks, which ran for about 23 minutes until stopped)

## Prompt

```
Set up the core of a Temple Run–style endless runner in this URP project.

- Player: use a capsule as a placeholder (it will be replaced by a rigged character later). It runs forward automatically, moves between 3 lanes (left/right arrows or A/D, swipe on mobile), jumps (Space/up/swipe up) and slides (down/S/swipe down).
- Track: spawn straight path segments ahead of the player from a few prefabs and remove them behind, so the path is endless. Lanes 2 units apart.
- Obstacles: low barriers to jump, high beams to slide under, and blocks that fill a lane. Collectible coins in lines.
- Speed slowly increases over time. Show distance and coins in a simple UI. On hitting an obstacle, show a Game Over screen with a Restart button.
- Camera follows from behind and slightly above, like Temple Run.
- Put scripts in Assets/Scripts with clear names, and explain what you created and how to test it.
```

## Result

What the Assistant created:

| Script (`Assets/Scripts/`) | Job |
|---|---|
| `PlayerController.cs` | Capsule player: auto-run, 3 lanes, jump, slide; keyboard (new Input System) and touch/mouse swipes |
| `TrackManager.cs` | Spawns track segments ahead and removes them behind |
| `TrackSegment.cs` | One piece of track (knows its start, end and length) |
| `Obstacle.cs` | Ends the run when the player hits it |
| `Coin.cs` | Spins, and is collected on contact |
| `GameManager.cs` | Speed that rises over time, distance, coins, game over and restart |
| `UIManager.cs` | Distance, coins and speed on screen; Game Over panel with Restart button |
| `FollowCamera.cs` | Camera behind and slightly above the player |

Also: prefabs in `Assets/Prefabs/` (Player, Coin, three obstacle types, track segments), materials in `Assets/Materials/`, and the setup in `Assets/Scenes/SampleScene.unity`.

**Testing notes:** Tested with ▶ Play: it worked. The capsule runs forward, switches lanes, jumps and slides; the track, obstacles, coins, UI and Game Over / Restart all work. No fixes needed.
