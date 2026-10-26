# 12 – Natural running animation (Mixamo)

- **Date:** 2026-10-26
- **Tool:** done by hand in Unity, no Assistant prompt
- **Credits used:** 0

## Why

Tenzin's original Run animation (from the AI-generated character package) was made procedurally: his arms and legs swung straight back and forth like pendulums, which looked robotic next to Temple Run's runners. Natural-looking runs come from motion capture, so the run was replaced with a free motion-captured run from Mixamo.

## Steps

1. **Download from mixamo.com** (free Adobe account): searched "running", tried **Running** and **Sprint**, with **In Place** ticked; downloaded as **FBX for Unity, Without Skin, 30 fps**.
2. **Import:** dragged the files into `Assets/Tenzin/Animations/Mixamo/`.
3. **Import settings** (Inspector, on each FBX):
   - Rig tab: **Animation Type: Humanoid**, **Avatar Definition: Create From This Model**, Apply.
   - Animation tab: **Loop Time** and **Loop Pose** on; **Bake Into Pose** for Root Transform Rotation, Position (Y) (based upon Feet) and Position (XZ); Apply.
4. **Swap it in:** in `Assets/Tenzin/Tenzin.controller`, set the **Run** state's Motion to the Running clip, then **File → Save Project** (⌘S only saves the scene).

## Problems and fixes

| Problem | Cause | Fix |
|---|---|---|
| Arms held behind the body ("Naruto run") | Tenzin's model is in an A-pose, so his avatar's T-pose reference was off and retargeted arm motion came out shifted backwards | `Tenzin_Unity` → Rig → **Configure… → Pose → Enforce T-Pose** → Apply → Done |
| Sprint drifted left and right | The motion capture's hip sway and drift were being applied as steering | Tried Root Transform Rotation / Position (XZ) **Based Upon: Original**; in the end Sprint was dropped |

**Result:** the **Running** clip is now Tenzin's run, with natural knee bend, hip bounce and arm swing. Sprint was deleted.

**Note:** Enforce T-Pose also affects how his original Idle, Jump, Slide, Stumble and Fall clips play, since they share the same avatar. Check them; if any look worse, replace them with Mixamo versions the same way.

**Lesson:** for any new human animation, use Mixamo (or another motion-capture source) rather than asking the Assistant: it can only write movement as code, which looks robotic.
