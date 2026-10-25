# 08 – Visual fixes

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 71 (269 → 198)

## Prompt

```
Fix four small visual problems. Change only what's needed for these; keep all gameplay, track segments, UI, sounds and animations exactly as they are.

1. Tenzin's head glows orange when he runs past butter lamps. The lamps' point lights (ButterLamp.cs / LampLight objects) are lighting him too strongly. Put Tenzin (the Tenzin child of Player, including all its renderers) on a new "Player" layer and remove that layer from the culling mask of every lamp point light, including in all segment prefabs. Also lower each lamp light's range to about 2.5 m.

2. A small flat grey square appears on the path just behind Tenzin while running. It is most likely a run-dust particle drawing without its texture, or a leftover UI image. If it's a particle, make sure the dust uses the soft round texture with a transparent material; if it's a UI element, remove it.

3. Lamp sparkles still look square. In EffectsManager.cs, the URP particle shader reads its texture from "_BaseMap", not "_MainTex": assign the generated soft-circle texture to _BaseMap (and _MainTex), and make sure the material's surface type is Transparent with the correct blend mode (additive for sparkle, alpha for dust).

4. Tenzin looks small on screen. In FollowCamera, move the camera closer and a little lower (offset about (0, 2.6, -3.8), pitch about 14°), aiming slightly ahead of him, so he fills more of the lower third while the path ahead stays visible.

- Tell me briefly what each cause was and what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

Causes and fixes, from the Assistant's summary, checked against the changed files:

| Problem | Cause | Fix |
|---|---|---|
| Orange glow on Tenzin's head | The butter-lamp point lights had a 3.5–4.0 m range and lit everything, Tenzin included | New **Player** layer (`ProjectSettings/TagManager.asset`); Tenzin, with all his bones and both LOD meshes, moved onto it in `Tenzin.prefab`, `Player.prefab` and the scene; lamp lights' range lowered to 2.5 m and the Player layer removed from their culling masks in the segment prefabs, `Coin.prefab`, and at runtime in `ButterLamp.cs` |
| Grey square behind Tenzin | The run-dust particles used a URP particle material that wasn't set to transparent, so each drew as an opaque grey quad | In `EffectsManager.cs`: soft-circle texture on `_BaseMap` and `_MainTex`; material set to Transparent with alpha blending, transparent render queue and no depth writing |
| Square sparkles | URP's Particles/Unlit shader reads `_BaseMap` and needs the transparent surface keyword; neither was set | Same soft-circle texture on the sparkle material, set to Transparent with additive blending |
| Tenzin small on screen | Camera offset (0, 3.2, −4.8) with 17.5° pitch | Offset (0, 2.6, −3.8) and pitch 14° in `FollowCamera.cs` and on the Main Camera in the scene |

**Testing notes:** Ran in ▶ Play mode (screenshot below, at 477 m on a golden rooftop): Tenzin is bigger on screen, his head no longer glows orange, and the grey square is gone. The golden rooftop's eaves, finials and lamps are clearly visible, with the ordinary track continuing ahead.

![The game after this prompt](../Screenshots/08-visual-fixes.png)
