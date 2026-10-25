# 09 – The chase

- **Date:** 2026-10-25
- **Tool:** Unity Assistant, Agent mode, Unity Default model, new chat
- **Credits used:** 80 (2,198 → 2,118; 2,000 credits were added before this prompt)

## Prompt

```
Add a Temple Run–style chaser: a grey langur monkey (like the one in the reference animation) that chases Tenzin. Keep all existing gameplay, track segments, visuals, UI, sounds and Tenzin's animations working.

THE LANGUR
- Build it from primitives as a placeholder that can be swapped for a real model later: grey-silver fur body, black face, long curling tail, long arms; about 0.9 m tall. Put it under a "Chaser" object with the visual as a child, so the visual can be replaced without touching the logic.
- Animate it procedurally in code: a bounding gallop (body bob and stretch, arms and legs swinging, tail swaying).
- It follows Tenzin's exact path (lanes and corners) by replaying his recent positions with a delay, so it never cuts through walls or corners.

CHASE RULES
- At the start of a run the langur bursts out right behind Tenzin, then falls back out of view over about 4 seconds.
- Stumble: if Tenzin clips the side of an obstacle while changing lanes, or hits a low barrier while already in the air descending, he doesn't die: play his Stumble animation (state "Stumble", then back to Run), slow him slightly for 0.5 s, and the langur catches up to just behind him, visible on screen.
- If he stumbles again while the langur is close (within about 8 seconds), the langur catches him: Game Over, with Tenzin's Fall animation and the langur leaping onto him.
- If he runs about 8 seconds without stumbling, the langur falls back out of view again.
- Head-on hits on obstacles are still an immediate Game Over, as now.

FEEL
- Langur chatter sounds generated in code (short high squeaks) when it's close, and a screech when it catches him.
- A small on-screen hint the first time he stumbles: "The langur is right behind you!"

- Explain what you created and changed.
- Make the changes directly and save the scene and assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## Result

From the Assistant's summary, checked against the changed files.

**The langur** (`Assets/Prefabs/Chaser_Langur.prefab`, in the scene): a **Chaser** object with the body under a child **Visual**, so it can be swapped for a real model later without touching the logic. About 0.9 m tall, built from primitives: silvery-grey fur, a pale cream neck ruff and chest, a charcoal-black face, palms and feet, dark glossy eyes, long arms and legs, and a 5-segment tail curling up over its back. New materials: `M_Langur_Fur`, `M_Langur_PaleFur`, `M_Langur_Skin`, `M_Langur_Eyes`.

New scripts:

| Script | Job |
|---|---|
| `LangurProceduralAnimator.cs` | The gallop made in code: body bob, pitch, squash and stretch and sway, with stride speed following Tenzin's run speed; arms reaching and legs kicking in alternation; head kept steady and locked on Tenzin; tail arching and swaying. When it catches him, it blends into a leap with arms reaching forward. |
| `LangurChaser.cs` | Records Tenzin's recent positions, rotations and distance, and places the langur a set distance back along that trail, so it follows his exact lanes and corners. Run start: bursts out about 1.9 m behind him (on screen), then drops back to about 8.5 m (behind the camera) over 4 seconds. On a stumble it surges back to about 1.9 m within about 0.6 s and stays close for 8 seconds; 8 seconds without stumbling and it drops back; a second stumble while close and it leaps onto him: Game Over. |

Changed scripts:

| Script | Change |
|---|---|
| `PlayerController.cs` | Stumbles when he touches a barrier or lane block while changing lanes, or comes down onto a low barrier from a jump. A stumble plays the Stumble animation then back to Run, slows him by 30% for 0.5 s, and briefly ignores that obstacle so he doesn't get stuck on it. If the langur is already close, it's Game Over with the Fall animation. Head-on hits (running into a block or barrier, or a high beam without sliding) are still instant Game Over. |
| `AudioManager.cs` | Langur chatter (high chirps, 1.8–2.8 kHz) while it's close, and a harsh screech when it catches him, both generated in code |
| `UIManager.cs` | A gold-accented banner, "The langur is right behind you!", the first time he stumbles |

**Testing notes:** Ran in ▶ Play mode (screenshot below, start screen): Tenzin stands in his Idle pose, with "Tenzin Run", "Tap or press Space to run" and a best distance of 1,958 m, and the langur's pale head and tail are just visible at the bottom of the screen behind him, ready to burst out. To check in play: the burst at the start, stumbles bringing it close, and the catch on a second stumble.

![The game after this prompt](../Screenshots/09-the-chase.png)
