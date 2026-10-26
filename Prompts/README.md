# Prompt log

Every prompt given to Unity Assistant, in order. Each file has the exact prompt, the model used, and what it produced.

| # | Prompt | What it did |
|---|---|---|
| 01 | [Endless runner basics](01-endless-runner-basics.md) | Capsule player, endless track, obstacles, coins, UI, camera |
| 02 | [Turns](02-turns.md) | 90° left/right corners, turn input, run-off-edge Game Over, camera follows turns |
| 03 | [Monastery look](03-monastery-look.md) | Stone path, rope rails, prayer flags, monastery buildings, butter lamps, sky, clouds, mountains |
| 04 | [Game feel](04-game-feel.md) | Start, pause and Game Over screens, best distance, code-made sounds, dust and sparkle effects, camera shake, rising difficulty |
| 05 | [Fix sparkle and sky](05-fix-sparkle-and-sky.md) | Sparkle and dust get a proper particle material; backdrop mountains pushed back so the sky shows. Two attempts, 163 credits |
| 06 | [Tenzin character](06-tenzin-character.md) | The capsule is replaced by the 3D Tenzin with Idle, Run, Jump, Slide and Fall animations; collider and camera fitted to him |
| 07 | [Track variety](07-track-variety.md) | Rope bridge, gap to jump, golden rooftop and gate arch to slide under, mixed into the track with spacing rules. 158 credits |
| 08 | [Visual fixes](08-visual-fixes.md) | No more orange glow on Tenzin, grey square gone, round sparkles, camera closer so Tenzin is bigger on screen |
| 09 | [The chase](09-the-chase.md) | A langur (from primitives) chases Tenzin; clipping obstacles makes him stumble, and a second stumble while it's close means it catches him. Chatter and screech sounds |
| 10 | [Visual polish](10-visual-polish.md) | Blue morning sky, white sunlight and sharper shadows, sky-tinted ambient, adjusted materials, bloom and colour grading. First attempt (Ultra) turned the game magenta and was reverted. 190 credits for both |
| 11 | [Phone and web (scripts)](11-phone-and-web.md) | Touch swipes, nearest-4 lamp lights, safe-area script, portrait camera, clean-up. Stopped before the scene wiring (still to do by hand). 229 credits |
| 12 | [Natural running animation](12-mixamo-run.md) | Done by hand, 0 credits: Mixamo motion-captured Running, Running Slide and Running Jump replace the robotic animations; Tenzin's avatar set to a proper T-pose to fix arms held behind him |
| 13 | [Meditation Flight power-up](13-meditation-flight.md) | Lamps earn a flight (200, 500, 900…): Tenzin meditates in the air, glowing, with light streaks; gold meter. 259 credits plus free fixes by Claude (corners while flying, meter, effects) |
| 14 | [Web effect fixes and controls](14-web-fixes-and-controls.md) | Done by Claude, 0 credits: effects fixed for the web build (shader included, emission kept, template materials), tilt to move, jump from slide, camera lag and lean |

## Adding a prompt

1. Copy the last file and bump the number, e.g. `02-short-name.md`.
2. Fill in the date, model, the exact prompt, and the result.
3. Add a row to the table above.
4. Commit the prompt file together with the changes it produced, e.g. `git commit -m "Fix jump height (see Prompts/02)"`.
