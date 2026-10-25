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

## Adding a prompt

1. Copy the last file and bump the number, e.g. `02-short-name.md`.
2. Fill in the date, model, the exact prompt, and the result.
3. Add a row to the table above.
4. Commit the prompt file together with the changes it produced, e.g. `git commit -m "Fix jump height (see Prompts/02)"`.
