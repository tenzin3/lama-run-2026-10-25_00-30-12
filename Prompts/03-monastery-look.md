# 03 – Monastery look

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 72 (846 → 774)

## Prompt

```
Give the endless runner a Himalayan monastery look, using only Unity primitives, URP materials and simple scripts (no imported assets, no AI-generated textures).

- Path: warm grey stone with slightly varied tile colours. Low stone walls along both edges, topped with wooden posts and a rope rail.
- Obstacles: low barrier = wooden beam on two posts; high beam = a wooden crossbeam hung with maroon cloth; lane block = a stack of logs or a whitewashed stone block with a maroon band near the top.
- Coins: replace with small golden butter lamps (gold cup with a soft glowing flame using an emissive material and a point light only on every few lamps for performance).
- Scenery beside the path (not blocking it): occasional whitewashed monastery buildings with a maroon band and dark wooden roof, some with golden roofs, and strings of prayer flags (blue, white, red, green, yellow) across the path every few segments. Spawn and remove them with the track segments, including on corners.
- Sky and distance: a morning sky (deep blue above, warm peach near the horizon), soft fog, a sea of clouds below the path and distant snowy mountain shapes far away.
- Lighting: warm low sun with soft shadows.
- Keep all gameplay exactly as it is (lanes, turns, jump, slide, collisions, UI, Game Over).
- Explain what you created and changed.
- When the code compiles and the scene is set up, stop and give me your summary. Do not enter Play mode, do not write test scripts, and do not run extra verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

New scripts and shader:

| File | Job |
|---|---|
| `Assets/Scripts/ButterLamp.cs` | Butter-lamp pickup (replaces the coin look): flickering flame, with a real point light on only every few lamps |
| `Assets/Scripts/HimalayanBackdrop.cs` | Keeps the distant mountains and cloud sea around the player as the track turns |
| `Assets/Shaders/HimalayanGradientSkybox.shader` | Gradient morning sky (deep blue at the top, warm near the horizon) |

New materials (`Assets/Materials/`): stone (light, mid, dark, wall), whitewash, maroon, maroon cloth and trim, gold, butter, emissive flame, dark wood, wooden posts, rope, mountain rock, snow, cloud, skybox, and five prayer-flag colours.

Changed: every track-segment prefab (including both corners) now has stone tiles, edge walls, posts and rope rails, and spawns monastery buildings and prayer-flag strings beside the path. The obstacle prefabs became the wooden beam, the cloth-hung crossbeam and the log stack / whitewashed block. The coin prefab is now a butter lamp. The player material changed colour. The scene was updated with the new sky, fog, lighting and backdrop.

**Testing notes:** Ran in ▶ Play mode (screenshot below). The monastery look came through: stone path with rope rails, prayer flags across the path, whitewashed buildings with maroon bands and golden roofs, and butter lamps. The sky came out a hazy pink-grey rather than the deep blue asked for, and the distant mountains are large flat grey shapes. These could be improved in a later prompt.

![The game after this prompt](../Screenshots/03-monastery-look.png)
