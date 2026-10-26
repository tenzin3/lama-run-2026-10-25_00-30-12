# 13 – Meditation Flight power-up

- **Date:** 2026-10-26
- **Tool:** Unity Assistant (Agent mode, Unity Default model, new chat), then fixes by Claude directly in the scripts
- **Credits used:** 259 (Assistant only; Claude's fixes cost no Unity credits)

## Before the prompt (by hand)

Downloaded **Sitting Idle** from Mixamo (FBX for Unity, Without Skin, 30 fps) into `Assets/Tenzin/Animations/Mixamo/`. Import settings: Humanoid (Create From This Model), Loop Time on, Bake Into Pose on for rotation, Y and XZ, all Based Upon Original.

## Prompt

```
Add a "Meditation Flight" power-up earned by collecting butter lamps. Keep all existing gameplay, track, Tenzin's other animations, the monkey chase, visuals, UI and sounds working.

STRICT LIMITS
- Do NOT modify URP pipeline assets, renderer assets, Quality, Graphics or Build/Player settings, and do not install packages.
- Avoid editing the large SampleScene file where possible: create the new HUD bar and effects from code at runtime.

EARNING IT
- Track lamps collected this run. Flights trigger automatically at 200 lamps, then after 300 more, then 400 more, each gap 100 bigger (thresholds 200, 500, 900, 1400, ...). Reset at the start of each run. Lamps collected during a flight count toward the next one.

THE FLIGHT (new MeditationFlight.cs; all numbers as editable fields)
- Tenzin rises smoothly to about 3 m above the path over 0.5 s, plays a new "Meditate" state in Assets/Tenzin/Tenzin.controller using "Sitting Idle" (Assets/Tenzin/Animations/Mixamo/Sitting Idle.fbx) (add the state, no transitions, like the others), and flies forward at 1.6x the current run speed for 6 seconds.
- While flying: no obstacle can hurt him (no stumbles, no Game Over), lane changes still work, he turns automatically at corners (no input needed, can't fall off), and gaps are safe.
- A line of butter lamps appears at flying height along his path for him to collect.
- The monkey chaser drops back out of view.
- Visual: a soft golden glow/aura around him and a trail of small golden particles. Sound, generated in code: a deep singing-bowl tone on take-off and a soft low hum while flying.
- Landing: over the last 0.7 s he floats back down and crossfades to Run; he stays invincible for 1 more second after landing.

HUD
- A small lotus-coloured progress bar under the lamp counter: "Meditation 143 / 200" (counting lamps since the last flight toward the next threshold). It glows when a flight starts. Create it in code, keep it inside the safe area, and make sure it works in portrait and landscape.

- Explain what you created and changed.
- Make the changes directly and save the assets. Do not investigate with test scripts or debug logs, do not enter Play mode, and do not run verification passes. I will test it myself.
- If something fails to compile, fix it once and report back instead of looping.
```

## What the Assistant built

| File | Change |
|---|---|
| `MeditationFlight.cs` (new, about 770 lines) | Thresholds 200 / 500 / 900 / 1,400… (gap grows by 100), take-off, 6 s flight at 1.6× speed, invincibility, lamps spawned at flying height along the path (following corners), landing plus 1 s invincibility, singing-bowl strike (220 Hz) and looping hum (110 Hz) made in code, point light, particle trail, a flat "aura disc" |
| `Tenzin.controller` | New **Meditate** state using Sitting Idle |
| `PlayerController.cs` | Adds MeditationFlight at runtime; Meditate / Run crossfades; 1.6× speed and flight height; auto-turn at corners; damage, stumbles, falls and monkey catches skipped while flying or invincible |
| `LangurChaser.cs` | `DropBackOutOfView()` keeps the monkey far behind during a flight |
| `GameManager.cs` | Lamp pickups feed the flight counter; counter reset on a new run |
| `UIManager.cs` | Pink "Meditation 143 / 200" bar created at runtime |

## Problems found in testing, and fixes (by Claude, no credits)

| Problem | Cause | Fix |
|---|---|---|
| During a flight Tenzin flew straight past corners and off the path, then kept running on nothing | Corners are detected by 3 m-tall trigger boxes on the path; at 3 m flight height he passed over them, so the auto-turn never ran | `CornerSegment.cs` keeps a list of active corners and can test the turn area by position, ignoring height; `PlayerController.cs` looks up the corner ahead from that list while flying |
| The pink bar with numbers looked cluttered | Design | `UIManager.cs`: replaced with a Temple Run-style **vertical gold meter** under the pause button that fills bottom-to-top, no text, glowing while flying |
| A flat yellow square under Tenzin while flying | The "aura disc" was a plain horizontal quad with no soft texture | Removed. Tenzin's **body now glows gold** (emission, picked up by bloom), with a **soft halo** behind him |
| Square trail particles | No texture on the particle material | Soft round texture added |
| Fixed fan of rays looked stiff (first replacement) | Design | Replaced with a **burst of light streaks**: about 160 per second shooting out of his body in random directions (stretched particles, gold to white, travelling with him) |

All effect settings are editable in the Inspector under **Meditation Flight** (glow strength, streak rate, speed, lifetime, width, stretch, halo size, glow height).

**Testing notes:** Tested in ▶ Play mode: the flight follows the path through corners, the meter fills and glows, and the glow, halo and light streaks look right. The body glow may not appear in the web build if Unity strips the emission shader variant; the halo and streaks will still show.
