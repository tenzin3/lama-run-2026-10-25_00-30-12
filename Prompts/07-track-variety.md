# 07 – Track variety

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 158 (427 → 269), the most expensive prompt so far

## Prompt

```
Add variety to the track with four new segment types. Keep all existing gameplay, segments, visuals, Tenzin's animations, UI and sounds working.

The path height stays the same everywhere (no stairs or slopes): PlayerController assumes a flat path at the current height.

1. Rope bridge: a wooden-plank bridge 3 lanes wide, with rope rails and posts, over open air (cloud sea visible below, no ground under the planks). Obstacles: occasional low wooden beams to jump.
2. Gap: a break in the path across all lanes that must be jumped. Use a trigger at path level that ends the run (same Game Over as running off a corner edge, reuse OffEdgeTrigger) only if Tenzin is not in the air. Gap length must be at most half the distance a jump covers at the starting speed, so it's always clearable.
3. Golden rooftop: a section where the path runs along a flat golden temple roof (gold material) with upturned eaves and small golden finials along both edges, and lines of butter lamps.
4. Gate arch: a whitewashed stone arch with a maroon band over the path, with a low lintel hung with small bells; Tenzin must slide under it (same clearance as the high beam: lower edge 1.30 m, his slide height 0.65 m).

TrackManager:
- Mix the new segments in with the existing ones. Never two special segments in a row, never a special segment directly before or after a corner, and only start spawning gaps after 200 m.
- Each new segment's start and end must line up exactly with the neighbouring segments, including on corners in any direction.

- Explain what you created and changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

From the Assistant's summary, checked against the changed files.

New segment prefabs (`Assets/Prefabs/Segments/`):

| Segment | What it is |
|---|---|
| `Segment_RopeBridge` | 30 m long, 3 lanes (6.8 m) wide, over open air with the cloud sea visible below and between the planks. 42 timber planks on stringers, timber portal posts with prayer flags at each end, three rope rails (hand, mid, foot) on posts every 2.5 m, plus hanging cables underneath. Occasional low wooden beams to jump, with arcs of lamps over them. |
| `Segment_Gap` | 13.25 m of stone road, a 3.50 m chasm across all lanes, then 13.25 m of road. At the starting speed (12 m/s) a jump lasts 0.654 s and covers 7.85 m, so 3.50 m is under half a jump. Broken stone edges, exposed timbers, warning prayer-flag posts and an arc of lamps over the gap. |
| `Segment_GoldenRooftop` | The path runs along a flat golden temple roof with upturned eaves, brackets, and golden finials (urns, spires, jewel tips) along both edges, and lines of butter lamps. |
| `Segment_GateArch` | Whitewashed pillars on dark stone plinths, a maroon frieze band with gold trim, a timber cornice, a pagoda roof and a Dharma-wheel finial. A dark-timber lintel across all 3 lanes with its lower edge at 1.30 m (a HighBeam obstacle: standing 1.48 m hits it, sliding 0.65 m passes under), small golden bells along it, and low lamps at 0.45 m to collect while sliding. |

Changed scripts:

| Script | Change |
|---|---|
| `OffEdgeTrigger.cs` | New `_onlyWhenGrounded` option: the gap's trigger only ends the run if Tenzin is on the ground, so a jump clears it while running in or landing short is Game Over (same as running off a corner) |
| `TrackSegment.cs` | New `IsSpecial` flag |
| `TrackManager.cs` | References to the four new prefabs; special segments appear with a 35% chance; never two special segments in a row, never directly after a corner, and the segment after a special one is always a plain straight, so a special segment never sits right before a corner either; gaps only after 200 m; the rising obstacle density skips special segments so their hand-placed jump and slide clearances stay intact |

The scene's TrackManager was wired to all four prefabs and saved.

**Testing notes:** Ran in ▶ Play mode (screenshot below, at 99 m): Tenzin runs through the gate arch, between tall whitewashed walls under the maroon frieze, golden roof and finial, with a line of butter lamps ahead. To check: whether gaps (after 200 m) and the slide under the arch work as intended. Seen again: a strong warm glow on Tenzin's head from lamp lights, and the small grey square behind him.

![The game after this prompt](../Screenshots/07-track-variety.png)
