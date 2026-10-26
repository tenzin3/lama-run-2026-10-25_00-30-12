# 14 – Web effect fixes, tilt controls, jump from slide, camera lean

- **Date:** 2026-10-26
- **Tool:** done by Claude directly in the scripts, plus two settings changed by hand in Unity
- **Credits used:** 0

## Part 1: effects broken in the web build

In the browser (phone), the Meditation Flight effects showed as **magenta squares** and Tenzin didn't glow, although everything worked in the Unity editor.

| Problem | Cause | Fix |
|---|---|---|
| Magenta squares (trail, streaks, lamp sparkle) | The effect materials are created in code at runtime, looking up the URP **Particles/Unlit** shader by name. No saved material used that shader, so the web build left it out | **Project Settings → Graphics → Always Included Shaders**: added *Universal Render Pipeline/Particles/Unlit* (by hand). In code, removed the fallback to the legacy built-in particle shader (it draws magenta in URP); if the shader is ever missing, the effects now hide instead |
| No body glow | Tenzin's material didn't use emission, so the build stripped that shader feature | **Tenzin_Material**: Emission ticked, colour **RGB 2,2,2** (by hand). Pure black doesn't work: Unity treats black emission as "off" and still strips it |
| Then: halo a solid yellow square, streaks hard rectangles | The build still lacked the shader's **transparent** variant, so the soft alpha in the textures was ignored | Two saved template materials in `Assets/Resources/FX/` (**FX_ParticleAdditive**, **FX_ParticleAlpha**, transparent, URP Particles/Unlit) and a helper **`FxMaterials.cs`**; `EffectsManager.cs` and `MeditationFlight.cs` create all effect materials from these templates, so the build includes the right variants |

**Lesson:** anything created only in code (materials, shader settings) may work in the editor but be stripped from builds. Keep a saved material in the project for every shader/setting combination used at runtime, and always test the web build.

## Part 2: controls and camera

| Change | Where | Details |
|---|---|---|
| **Tilt to move** (Temple Run style) | `PlayerController.cs` | On phones, tilting moves Tenzin smoothly across the path (full edge at about 22°, 2.5° dead zone). Swipes still turn, jump and slide; sideways swipes only turn while tilt is active. Only switches on when the device sends accelerometer data, so computers and browsers without motion access keep swipe/key lane changes. Settings: on/off, full angle, dead zone, responsiveness, invert |
| **Jump from a slide** | `PlayerController.cs` | Swiping up mid-slide cancels the slide and jumps |
| **Camera lag and lean** | `FollowCamera.cs` | Camera trails sideways movement slightly (up to 0.9 m, 0.18 s) and leans into it (roll 3.5°, yaw 2.5°), so movement feels alive instead of locked. All values editable on Main Camera |
| Hint text on phones | `UIManager.cs` | "Tilt to move · Swipe to turn · Swipe up to jump · Swipe down to slide" |

## Part 3: endless run after a flight

**Bug (found on a phone):** right after a Meditation Flight landed, a corner came up; the turn wasn't taken in time, and Tenzin kept running straight into the clouds forever with no Game Over.

**Cause:** for 1 second after landing he is protected, and the "ran off the edge" check was ignored while protected. After that, nothing caught him: the corner stopped tracking him once he left its turn area, and the edge trigger only fires once.

**Fix (`PlayerController.cs`):**
- Corners are now turned automatically during the protected second after landing too, not only while flying.
- If he leaves a corner's turn area without turning, the corner keeps tracking him, so running past its edge still ends the run once he's no longer protected.

**Testing notes:** tilt works on an Android phone. Camera lag/lean and jump-from-slide tested in the editor and liked. Tilt still to test on phones (iPhone Safari may need a motion-permission prompt; Android Chrome should work directly).
