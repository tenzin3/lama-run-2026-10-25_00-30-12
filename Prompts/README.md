# Prompt log

Every prompt given to Unity Assistant, in order. Each file has the exact prompt, the model used, and what it produced.

| # | Prompt | What it did |
|---|---|---|
| 01 | [Endless runner basics](01-endless-runner-basics.md) | Capsule player, endless track, obstacles, coins, UI, camera |
| 02 | [Turns](02-turns.md) | 90° left/right corners, turn input, run-off-edge Game Over, camera follows turns |
| 03 | [Monastery look](03-monastery-look.md) | Stone path, rope rails, prayer flags, monastery buildings, butter lamps, sky, clouds, mountains |

## Adding a prompt

1. Copy the last file and bump the number, e.g. `02-short-name.md`.
2. Fill in the date, model, the exact prompt, and the result.
3. Add a row to the table above.
4. Commit the prompt file together with the changes it produced, e.g. `git commit -m "Fix jump height (see Prompts/02)"`.
