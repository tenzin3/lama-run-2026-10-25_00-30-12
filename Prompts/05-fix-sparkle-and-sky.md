# 05 – Fix sparkle and sky

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat (two attempts)
- **Credits used:** 163 in total
  - Attempt 1: 82 (660 → 578). Stopped after about 9 minutes; it spent the time investigating with test scripts and saved no changes.
  - Attempt 2: 81 (578 → 497). Made the fixes.

## What went wrong in attempt 1

The first prompt asked it to find what was tinting the sky. The skybox colours were in fact already correct, so it kept investigating the wrong things (skybox colours, camera view rays) and never reached a fix. The real cause was the giant backdrop mountains and clouds covering most of the sky.

**Lesson:** name the cause when you know it, and tell it to make changes directly instead of investigating.

<details>
<summary>Attempt 1 prompt (stopped)</summary>

```
Fix two visual bugs. Change only what's needed for these; keep all gameplay, UI and other visuals as they are.

1. Lamp sparkle shows as big flat yellow squares.
   In EffectsManager.cs, the particle systems have no proper particle material. For the lamp sparkle (and the dust puffs too, if they have the same problem):
   - Give the renderer a URP particle material ("Universal Render Pipeline/Particles/Unlit") using a soft round texture generated in code (white circle fading to transparent at the edges).
   - Lamp sparkle: additive blending, gold colour, small size (about 0.05–0.15), short lifetime (about 0.3–0.5 s), 10–15 particles, fading out over its lifetime.
   - Dust: alpha blending, pale stone colour, soft and semi-transparent.

2. The sky still looks hazy pink-grey.
   Find what is tinting it: check the skybox material (M_Himalaya_Skybox / HimalayanGradientSkybox.shader) and that it is the one assigned in Lighting settings, the fog colour and density in RenderSettings, the ambient light colour, and any Color Adjustments / tonemapping in the Global Volume.
   Target: clear deep blue (#2E5FA8-ish) at the top, lighter blue in the middle, warm peach only in a thin band at the horizon. Fog should be a light blue-grey matching the horizon, light enough that buildings far down the path stay visible.

- Tell me briefly what the cause of each bug was and what you changed.
- When the code compiles and the scene is set up, stop and give me your summary. Do not enter Play mode, do not write test scripts, and do not run extra verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

</details>

## Prompt (attempt 2)

```
Fix two visual bugs. Change only what's needed; keep all gameplay, UI and other visuals as they are.

1. Lamp sparkle draws as big flat yellow squares. In EffectsManager.cs, give the particle systems a URP particle material ("Universal Render Pipeline/Particles/Unlit") with a soft round texture generated in code (white circle fading to transparent). Lamp sparkle: additive, gold, small (0.05–0.15), lifetime 0.3–0.5 s, 10–15 particles, fading out. Dust: alpha-blended, pale stone colour, soft and semi-transparent.

2. The sky looks pink-grey, but the skybox colours are already correct. The cause is the backdrop: the mountains and cloud shapes from HimalayanBackdrop.cs are so large and close that they cover most of the sky. Move the mountains much further away and lower, so their peaks only rise a little above the horizon and most of the screen above the path shows the blue skybox. Lighten the mountains to a pale blue-grey with white peaks so they fade into the distance. Keep the cloud sea below the path level only.

- Tell me briefly what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

| File | Change |
|---|---|
| `Assets/Scripts/EffectsManager.cs` | Particles now use a URP particle material with a soft round texture made in code; sparkle is additive gold and small, dust is soft pale stone |
| `Assets/Materials/M_Himalaya_Mountain_Rock.mat` | Mountains lightened |
| `Assets/Scenes/SampleScene.unity` | Mountains and clouds moved further away and lower, so much more sky shows |

**Testing notes:** Ran in ▶ Play mode (screenshot below). Both fixes are clearly better: the sparkle is now small gold bits instead of big squares, and the mountains sit far back as pale blue-grey shapes, so much more of the sky is visible. Still not perfect: the sparkle bits look square rather than round, and the sky reads as a warm dusk (lavender fading to peach) rather than the deep-blue morning asked for.

![The game after this prompt](../Screenshots/05-fix-sparkle-and-sky.png)
