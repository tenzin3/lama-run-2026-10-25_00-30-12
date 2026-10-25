# Tenzin Run (Unity)

A Temple Run–style endless runner starring Tenzin, a young monk, built in Unity 6 (URP).

**Status:** early prototype. Tenzin is now a rigged 3D character with run, jump, slide and fall animations.

## How it looks

![Tenzin Run after prompt 08: Tenzin, larger on screen, runs along a golden temple rooftop lined with butter lamps and golden finials, toward the stone path and monastery buildings](Screenshots/08-visual-fixes.png)

*Latest: after [prompt 08](Prompts/08-visual-fixes.md), 25 Oct 2026, on a golden rooftop at 477 m.*

### Progress

<table>
  <tr>
    <td align="center" width="33%"><img src="Screenshots/01-endless-runner-basics.png" width="280" alt="Grey prototype"><br><sub>01 · Basics</sub></td>
    <td align="center" width="33%"><img src="Screenshots/03-monastery-look.png" width="280" alt="Monastery look"><br><sub>03 · Monastery look</sub></td>
    <td align="center" width="33%"><img src="Screenshots/04-game-feel.png" width="280" alt="Game feel"><br><sub>04 · Game feel</sub></td>
  </tr>
  <tr>
    <td align="center" width="33%"><img src="Screenshots/05-fix-sparkle-and-sky.png" width="280" alt="Sparkle and sky fix"><br><sub>05 · Sparkle and sky fix</sub></td>
    <td align="center" width="33%"><img src="Screenshots/06-tenzin-character.png" width="280" alt="Tenzin character"><br><sub>06 · Tenzin</sub></td>
    <td align="center" width="33%"><img src="Screenshots/07-track-variety.png" width="280" alt="Track variety"><br><sub>07 · Variety</sub></td>
  </tr>
  <tr>
    <td align="center" width="33%"><img src="Screenshots/08-visual-fixes.png" width="280" alt="Visual fixes"><br><sub>08 · Visual fixes</sub></td>
  </tr>
</table>

Every screenshot is kept in [`Screenshots/`](Screenshots/).

## Controls

| Action | Keyboard | Phone |
|---|---|---|
| Change lane | ← → or A / D | Swipe left / right |
| Jump | Space, ↑ or W | Swipe up |
| Slide | ↓ or S | Swipe down |

## Opening the project

Open this folder in Unity Hub with Unity 6000.6, open `Assets/Scenes/SampleScene.unity`, and press ▶ Play.

## How it's being built

Most of the game is built by prompting Unity Assistant. Every prompt and its result is logged in [`Prompts/`](Prompts/README.md).

## Credit

Tenzin is a character from animations by **Arkizen Studio**: https://www.instagram.com/arkizen.studio/

The in-game 3D model of Tenzin was generated with an AI 3D-character tool from reference images taken from Arkizen Studio's animation; see [Prompts/06](Prompts/06-tenzin-character.md).

The earlier browser versions of the game are at https://github.com/tenzin3/tenzin-game
