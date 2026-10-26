# 11 – Phone and web (scripts)

- **Date:** 2026-10-26
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 229 (1,928 → 1,699), the most expensive prompt so far
- **Status:** partly done. The scripts were changed, but the Assistant stopped with a "Remaining Work Checklist" before updating the scene.

## Prompt

```
Make the game work well on phones and in a web browser. Keep all gameplay, track, Tenzin, the langur, visuals, UI and sounds working exactly as they are on desktop.

STRICT LIMITS
- Do NOT modify the URP pipeline assets, renderer assets, Quality settings, Graphics settings, Build/Player settings, or switch the build platform. Only change scripts, the scene, prefabs and UI.

TOUCH CONTROLS
- Swipes anywhere on screen: left/right change lane (or turn at corners), up jumps, down slides. Detect a swipe as soon as the finger has moved about 6% of the screen width (don't wait for the finger to lift), one action per swipe. A tap on the start screen starts the run.
- Keep keyboard and mouse controls working as now.
- Pause button and Sound button: at least 60×60 pixels on a phone, easy to tap, and taps on them must not count as swipes.

SCREEN LAYOUT
- Canvas Scaler: Scale With Screen Size, reference 1080×1920, match 0.5, so the UI looks right in portrait and landscape and on any screen size.
- Keep UI inside the phone's safe area (notch and rounded corners) using Screen.safeArea.
- Hide the keyboard controls hint on touch devices; show "Swipe to move · Swipe up to jump · Swipe down to slide" instead.
- Camera: in portrait, widen the field of view enough that all three lanes and the path ahead stay visible.

PERFORMANCE (for phones and WebGL)
- Set Application.targetFrameRate to 60.
- Butter-lamp point lights: at most 4 active at once, the ones nearest Tenzin; the rest use only their emissive glow.
- Make sure track segments, lamps, obstacles and particles that are behind the player are removed or reused, so nothing builds up over a long run.
- Generated sounds and textures are created once at startup, not repeatedly.

- Explain what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

Checked against the changed files. No rendering, quality, graphics or build settings were touched.

**Done (scripts, about 430 lines across 8 files):**

| Script | Change |
|---|---|
| `PlayerController.cs` | Touch swipes (Input System `Touchscreen`), detected while the finger moves, alongside keyboard and mouse |
| `ButterLamp.cs` | Only the 4 lamps nearest Tenzin keep their point light on, updated once per frame; the rest glow only |
| `SafeArea.cs` (new) | Fits a UI panel inside `Screen.safeArea` and updates when the screen or orientation changes |
| `UIManager.cs` | Code for an in-game sound button (`_hudMuteButton`, `_hudMuteText`) and the touch controls hint |
| `FollowCamera.cs` | Wider view in portrait |
| `TrackManager.cs`, `EffectsManager.cs`, `AudioManager.cs` | Clean-up of things behind the player; sounds and textures generated once |

**Not done (from its "Remaining Work Checklist"):**
- Canvas Scaler still 1920×1080 instead of 1080×1920.
- `SafeArea` not added to `HUDPanel` in the scene.
- The in-game sound button not created in the scene, so the new `UIManager` fields are unconnected.
- `Application.targetFrameRate = 60` not added anywhere.

These can be done by hand in the Inspector instead of spending more credits (Canvas → Canvas Scaler → Reference Resolution 1080 × 1920; HUDPanel → Add Component → SafeArea; save).

**Lesson:** prompts that change many scripts *and* the large scene file at once are expensive and may stop early. Split them: scripts in one prompt, scene and UI wiring in another, or do small scene changes by hand.

**Testing notes:** Tested in ▶ Play mode in the landscape Game view and it works (screenshot below): the start screen, sky, buildings, Tenzin and the langur all look as before, so nothing broke. Portrait layout and touch on a real phone are still to test once the scene steps above are done.

![The game after this prompt](../Screenshots/11-phone-and-web.png)
