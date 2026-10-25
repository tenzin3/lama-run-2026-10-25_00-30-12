# 06 – Tenzin character

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 70 (497 → 427)

## Before this prompt: making the 3D model

The character model was made outside Unity with an AI 3D-character tool, from reference images taken from Arkizen Studio's animation (a full-body front view, a face close-up and a back view). It took two rounds:

1. **First version:** rigged and game-ready, but adult proportions, a mauve robe, and only an Idle animation.
2. **Revision:** a 12–13-year-old boy (1.48 m), bigger eyes, ears, the forehead scar, deep burgundy robe draped over the left shoulder, URP material, and six in-place animations at 30 fps: Idle, Run, Jump, Slide, Stumble, Fall.

It was imported with **Assets → Import Asset Package…** (`Tenzin_Unity.unitypackage`) into `Assets/Tenzin/`.

<details>
<summary>Revision prompt given to the 3D tool</summary>

```
Revise the Tenzin character (Tenzin_Character.blend / Tenzin_Unity package) to match the reference images more closely and to work in a Temple Run–style runner game. Attached: front full-body, face close-up, and back view.

LIKENESS (most important)
- He is a boy of about 12–13, not an adult. Larger head relative to the body (about 1/6 of total height), shorter and thinner neck, narrow shoulders, slim limbs. Total height about 1.45–1.5 m.
- Face: large expressive eyes with big dark irises, strong but young brows, small nose, full cheeks, slightly serious expression. Ears stick out noticeably. A small scar on the upper forehead, as in the face reference. Bald head with a faint stubble shadow.
- Colours: robe deep maroon/burgundy (about #5E1F2A to #6B2333), not mauve. Shirt mustard yellow (about #D9A13A), a ribbed sleeveless tank top. Skin warm tan. Sneakers off-white with grey soles.

ROBE
- Shawl wraps over his LEFT shoulder, drapes diagonally across the chest to the right hip, and hangs down the back as in the back reference; his right shoulder and arm are bare (tank top showing).
- More layered folds where the shawl wraps the torso; a knotted sash at the waist with two hanging tails; the skirt ends at mid-shin with soft vertical folds.
- Fix the visible seam/gap where the right arm meets the shoulder and shirt.

RIG AND DEFORMATION (for gameplay)
- Keep a valid Unity Humanoid rig (Mixamo-compatible bone layout), +Z forward, Y up, origin on the ground between the feet.
- Weight the robe skirt so it stays intact in these poses: full sprint stride, jump with knees tucked, slide (low crouch with one leg forward), stumble, and fall. Add 2–4 skirt bones for gentle secondary motion if that helps. Show render checks of these five poses.
- If you can, include in-place animation clips at 30 fps: Idle, Run (seamless loop), Jump, Slide, Stumble, Fall. Otherwise keep the rig compatible with Mixamo animations retargeted through Unity Humanoid.

UNITY DELIVERY
- The project uses URP (Unity 6000.6.7f1). Supply the material using the "Universal Render Pipeline/Lit" shader with the base colour texture assigned (not Built-in Standard).
- Main mesh 30,000 triangles or fewer, plus the LOD1 mesh. Texture 2048×2048 if it improves the face and robe detail.
- Same deliverables as before: .unitypackage with prefab, FBX, LOD1 FBX, GLB, .blend, textures, front / back / three-quarter renders, pose renders, and validation reports.
```

</details>

## Prompt

```
Replace the capsule player with the Tenzin character. Keep all existing gameplay, track, UI, sounds and effects working.

- Use Assets/Tenzin/Tenzin.prefab as the player's visual, as a child of the Player object. Hide or remove the capsule's mesh renderer, but keep the Player's own movement and collision logic. Face him along the player's forward direction. Keep Apply Root Motion OFF and keep the TenzinSkirtFollow component enabled.
- He is 1.48 m tall. Resize the player's collider to fit him standing, and make the slide collider low enough to pass under the high beams. Check the obstacle heights still work: low barriers jumpable, high beams slidable.
- Animations: the prefab's Animator has states Idle, Run, Jump, Slide, Stumble and Fall (no transitions). From PlayerController, switch states with Animator.CrossFade (about 0.1 s): Idle on the start screen, Run while running, Jump when jumping, Slide when sliding, then back to Run when the jump lands or the slide ends, and Fall on Game Over. Scale the Run animation speed with the run speed so his feet roughly match the ground.
- Turns and lane changes: smoothly rotate his body toward the new direction and lean slightly into lane changes.
- Make sure he casts a shadow and that the camera framing still shows the path ahead clearly; adjust the follow camera height/distance if he looks too small or blocks the view.
- Explain what you changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

From the Assistant's summary, checked against the changed files:

| Area | Change |
|---|---|
| Player prefab and scene | The capsule visual is replaced by `Assets/Tenzin/Tenzin.prefab` as a child named **Tenzin**, facing +Z. Apply Root Motion stays off, `TenzinSkirtFollow` stays on. The character's own collider was removed so the Player's collider is the only one used for gameplay. Shadow casting and receiving are on. |
| Collider (`PlayerController.cs`) | Standing: height 1.48 m, centre 0.74 m, radius 0.30 m. Sliding: height 0.65 m, centre 0.325 m, radius 0.30 m. |
| Obstacle clearance | Low barrier is 0.70 m high and the jump peaks at about 1.39 m, so it clears. High beam's lower edge is at 1.30 m: standing (1.48 m) hits it, sliding (0.65 m) passes under. |
| Animations (`PlayerController.cs`) | `Animator.CrossFade` with about 0.1 s blends: Idle on the start screen, Run while running, Jump, Slide (the old squash-the-capsule effect was removed), back to Run on landing or when the slide ends, and Fall on Game Over (hitting an obstacle or running off an edge). While running, `Animator.speed` = current speed ÷ 12, so his footfalls follow the ground speed. |
| Turns and lanes | At corners his body turns smoothly to the new direction (360° per second) instead of snapping. On lane changes he leans up to 12° and turns up to 10° toward the new lane, then straightens. |
| Camera (`FollowCamera.cs`, scene) | Offset moved from (0, 4.0, −6.0) to (0, 3.2, −4.8), pitch 17.5°, so the smaller character sits in the lower third with the path ahead visible. |
| `Assets/Tenzin/` | The imported character package (model, LOD, URP material, texture, six animations, controller, skirt-follow script, preview scene). |

**Testing notes:** Ran in ▶ Play mode (screenshot below): Tenzin replaces the capsule and runs down the monastery path in his robe, seen from behind, mid-stride. Things to look at next: he is quite small on screen, his head picks up a strong warm glow from the lamps, and a small grey square (likely a dust particle) shows behind him.

![The game after this prompt](../Screenshots/06-tenzin-character.png)
