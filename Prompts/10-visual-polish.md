# 10 – Visual polish

- **Date:** 2026-10-25 / 26
- **Tool:** Unity Assistant, Agent mode, new chats (two attempts: Unity Ultra, then Unity Default)
- **Credits used:** 190 in total for both attempts (2,118 → 1,928). The split between the two attempts wasn't recorded.

## Attempt 1: Unity Ultra, failed (game turned magenta)

The first prompt asked for SSAO on the renderer and new High / Medium quality levels as well as colour and lighting. Ultra rewrote both URP renderer assets, both pipeline assets, and the quality settings (renaming "PC" / "Mobile" to "High" / "Medium" and changing their platforms). Afterwards the whole 3D view rendered as solid magenta, Unity's sign that it couldn't draw with its shaders.

**Recovery:** all changes were discarded in VS Code (back to the prompt 09 commit), then Unity was quit **without saving** and reopened, because the broken settings were still loaded in the running editor.

**Lesson:** the model wasn't the main cause; the prompt was. Prompts that change Unity's core rendering setup (pipeline assets, renderer features, quality and graphics settings) are risky. Forbid those files explicitly, and commit right before any rendering prompt.

<details>
<summary>Attempt 1 prompt (Ultra, broke rendering)</summary>

```
Give the game a polished, cinematic look. Change only visuals; keep all gameplay, track segments, Tenzin, the langur, UI and sounds working exactly as they are.

POST-PROCESSING (URP Global Volume)
- Bloom: soft glow on butter-lamp flames, gold roofs and finials, and the sun (threshold high enough that the stone path and whitewash don't glow).
- Colour grading: ACES tonemapping, slightly warm highlights and cool blue shadows, a touch more saturation for the maroon robes, prayer flags and gold.
- A subtle vignette.
- No depth of field and no motion blur (they hurt readability at speed).
- Enable SSAO on the URP renderer for soft contact shadows under buildings, rails and obstacles.

SKY AND LIGHT
- The sky still reads as dusk. Make it a bright Himalayan morning: deep blue at the top, lighter blue lower down, a thin warm band only right at the horizon. Keep the gradient skybox shader, just adjust its colours, and set fog to a light blue-grey matching the horizon.
- Sun: warm white, from the side and slightly behind the camera so Tenzin and the buildings get a lit side and clear shadows. Soft shadows, shadow distance about 60 m.
- Ambient light from the sky (trilight or skybox) so shaded sides are blue-tinted, not grey.

MATERIALS (procedural, generated in code or set on the materials; no imported textures)
- Stone path: subtle variation and a slight roughness difference between tiles.
- Whitewash walls: faint stains and slightly warmer tone at the bottom.
- Gold: metallic and smooth so it reflects the sky; wood: darker, less shiny.
- Snow on the mountains: bright white with blue shading.

PERFORMANCE
- Keep it running smoothly: put the heavier effects (SSAO, soft shadows) on a "High" quality level and lighter versions on "Medium", which phones will use later.

- Explain what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

</details>

## Attempt 2: Unity Default, worked

### Prompt

```
Improve the game's colours and lighting. Change only visuals; keep all gameplay, track segments, Tenzin, the langur, UI and sounds working exactly as they are.

STRICT LIMITS
- Do NOT modify the URP pipeline assets (Assets/Settings/*RPAsset.asset), the renderer assets (Assets/Settings/*Renderer.asset), Quality settings, or Graphics settings. Do not add renderer features (no SSAO).
- Only change: the skybox material's colours, fog and ambient settings in the scene, the Directional Light, material colours/smoothness/metallic values, and the existing Global Volume profile.

SKY AND LIGHT
- The sky reads as dusk. Make it a bright Himalayan morning: in M_Himalaya_Skybox, deep blue at the top, lighter blue lower down, a thin warm band only right at the horizon. Fog: light blue-grey matching the horizon.
- Directional Light (sun): warm white, from the side and slightly behind the camera so Tenzin and the buildings have a lit side and clear shadows.
- Ambient light: sky-tinted, so shaded sides look slightly blue, not grey.

MATERIALS (adjust existing materials only)
- Gold: metallic and smooth so it catches the light. Wood: darker, not shiny. Whitewash: slightly warm. Snow: bright white.

POST-PROCESSING (existing Global Volume profile only)
- Bloom on bright things only (lamp flames, gold, sun); threshold high enough that stone and whitewash don't glow.
- Tonemapping: ACES. Slightly warm highlights and cool shadows, a little more saturation. A subtle vignette.
- No depth of field, no motion blur.

- Explain what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

### Result

Checked against the changed files. It kept to the limits: no pipeline, renderer, quality or graphics settings were touched.

| Area | Change |
|---|---|
| Sky (`M_Himalaya_Skybox.mat`) | Top deep blue (0.06, 0.28, 0.75), middle light blue, warm peach only at the horizon |
| Fog and ambient (scene) | Fog light blue-grey; ambient sky / horizon / ground colours changed from warm and grey to blue-tinted |
| Sun (scene) | Colour warm white instead of orange (temperature 5,000 K → 5,800 K), intensity 1.3, and turned to a higher angle so buildings and Tenzin have a lit side and clear shadows |
| Materials | Gold, snow, whitewash, dark wood, wooden posts, beams and the lamp cups (`M_Coin`) adjusted |
| Post-processing (`SampleSceneProfile.asset`) | Motion blur removed; split toning (warm highlights, cool shadows) and colour adjustments (exposure, colour filter, saturation) added; bloom, ACES tonemapping and vignette set as asked |

**Testing notes:** Ran in ▶ Play mode (screenshot below, start screen): no magenta. The sky is now a clear blue morning with a warm band at the horizon, the buildings are crisp white with maroon bands, the snow is bright, and Tenzin and the buildings cast sharp shadows. The langur peeks in at the bottom.

![The game after this prompt](../Screenshots/10-visual-polish.png)
