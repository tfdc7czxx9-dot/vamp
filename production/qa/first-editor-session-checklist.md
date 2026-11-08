# First Editor Session Checklist (TD-01)

> For the owner. Use this the first time `src/Brasslight` is opened in Unity 6.3 LTS (6000.3.x).
> The Unity layer has never run; expect small fixes (shader keywords, material setup, IMGUI scaling).
> Goal: find and capture every problem in one pass, not to fix on the spot. Tick boxes, note the result.
>
> Date: ________  Unity version: ________  OS/GPU: ________  Commit: ________

## How to capture a failure (use for every step)

1. **Console**: Window > General > Console. Click "Clear" before the step. After the failure, set the filter to show Errors and Warnings, and copy the first error with the full stack trace (select the line, Ctrl/Cmd+C, or screenshot the lower detail pane).
2. **Screenshot**: the Game view and (for visual issues) the Scene view. Name them `step-<ID>-<what>.png`.
3. **Editor log** if Unity crashed or froze: Linux `~/.config/unity3d/Editor.log`, macOS `~/Library/Logs/Unity/Editor.log`, Windows `%LOCALAPPDATA%\Unity\Editor\Editor.log`. Keep the last 200 lines.
4. Note: what you pressed, which screen, which input device, whether it repeats.
5. Put everything in `production/qa/evidence/first-editor-session/` and send me the list of failed step IDs. I will file bugs.

Severity rule of thumb: crash, Console exception every frame, black screen, cannot get past a step = **S1**. Wrong visual or one broken menu = **S2**. Cosmetic = **S3**.

---

## A. Preparation (see `docs/BUILDING.md` section 1)

| ID | Step | What good looks like | If it fails, capture |
|---|---|---|---|
| A1 | Template copied (`ProjectSettings/`, `Packages/`, `Assets/Settings/`), project opened from `src/Brasslight` | Opens without the "Safe Mode" dialog | Safe Mode compile error list, screenshot |
| A2 | Wait for the first import and compile to finish | Console has no red compile errors (Clear then Reimport All if unsure). Yellow warnings are acceptable, list them | Every red error, with file and line |
| A3 | Window > Package Manager: Input System installed (In Project) | Listed with a version | Screenshot |
| A4 | Project Settings > Player > Active Input Handling | "Input System Package (New)" (Setup will set it) | Screenshot |

## B. Setup menu

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| B1 | Menu bar shows **Brasslight** | Menu present with Setup Project, Build, Platforms | Screenshot of the menu bar |
| B2 | Brasslight > Setup Project | Completes; Unity may ask to restart for the input backend: accept | Console errors during setup |
| B3 | After restart, check Player settings: product name, landscape-only, Linear colour space, Android IL2CPP + ARM64 + min API 26 | Matches `BUILDING.md` | Screenshots of Player settings |
| B4 | `Assets/Brasslight/Materials` exists with material templates | Folder has materials, none pink in preview | Screenshot |
| B5 | `Assets/Scenes/Boot.unity` exists and is scene 0 in File > Build Profiles (Build Settings) | Listed and ticked first | Screenshot |
| B6 | Window > General > Test Runner > EditMode > Run All | All green: 67 cases pass | Screenshot of red tests and their messages |

## C. Play in the Boot scene

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| C1 | Open Boot.unity, press **Play** | Short loading state, then the title, no red Console errors. Aim for under 10 s | Console, screenshot |
| C2 | Check the Game view aspect set to 16:9 (1920x1080 first) | Text sharp and readable, not tiny or huge (IMGUI scaling is a known risk) | Screenshot at 1920x1080 and at 1280x800 |
| C3 | Console during 60 s of idle title | No repeating exceptions or warnings flooding | Console |
| C4 | Frame rate: Game view > Stats | Roughly 60 fps in the editor on a mid PC (editor is slower than a build, just check that it is sane) | Stats screenshot |

## D. Title attract mode

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| D1 | Background behind the title menu | A live demo run is visible: diorama world, player machine moving on its own, enemies swarming, gadget fire/VFX, auto-picks of level-ups and builds | Screenshot; if black or pink, screenshot Scene view + Console |
| D2 | Sprites | Billboards face the camera and are not pink/magenta (shader or material missing) | Screenshot |
| D3 | Post-processing | Soft depth of field (tilt-shift) and bloom visible on Auto/High | Screenshot |
| D4 | Wait 3+ minutes (or until the demo ends) | Attract run restarts by itself with no Results screen, no error, no points added | Console |
| D5 | Audio | Placeholder theme plays; no crackle or silence | Note |
| D6 | Title menu entries | Start, Research (with points), Bestiary (n/m), Options, Credits, Quit (Quit hidden on mobile only) | Screenshot |

## E. Each menu screen (mouse, then keyboard, then gamepad)

Navigation: mouse click; keyboard W/S/A/D or arrows, Enter/Space to confirm, Esc/Backspace to go back; gamepad stick/D-pad, A (south) confirm, B (east) back, Start pause.

| ID | Screen | What good looks like | Capture |
|---|---|---|---|
| E1 | Title > Start (Expedition) | World selector with arrows, leader selector, description line, "Begin" and Back. Locked world/leader shows a lock reason and Begin is greyed. Left/Right changes the selection | Screenshot, Console |
| E2 | Expedition > leader locked | Unlock button visible with cost, greyed if you lack points | Screenshot |
| E3 | Title > Research | List of upgrades with level n/max, description and cost; Back writes the save | Screenshot |
| E4 | Title > Bestiary | Creatures per world; unknown shown as hidden, recruited marked with a star | Screenshot |
| E5 | Title > Options | Music, SFX, Quality, DOF, Shake, Damage numbers, Language, Back | Screenshot |
| E6 | Title > Credits | Text visible, Back works | Screenshot |
| E7 | Esc/B from every sub-screen returns to Title | No stuck screen, no double-trigger | Console |
| E8 | Focus highlight is always visible and cannot get lost | A highlighted entry on every screen; disabled entries are skipped or clearly greyed | Screenshot |
| E9 | No text clipped or overflowing | Check each screen at 1920x1080 and 1280x800 | Screenshot |

## F. Start a run

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| F1 | Select world 1 (The Hollow Earth) and the first leader, Begin | Switches to the run within about 1 s; HUD shows timer (0:00), level 1, XP bar, HP, Brass counter | Console |
| F2 | Move with WASD / arrows | Smooth movement, speed feels like a brisk walk (5 units/s), camera follows without jitter | Video if jerky |
| F3 | Move with a gamepad stick (and D-pad) | Same movement; analog magnitude respected | Note device model |
| F4 | Auto-attacks | Starting gadget fires by itself at nearby enemies, with hit effects and damage numbers | Screenshot |
| F5 | Enemies | First enemies appear at the edge of the screen about 1 per second and walk toward you | Screenshot |
| F6 | Pickups | Aether crystals and Brass drop, get magnetised at about 3 units, collected at about 1 | Screenshot |
| F7 | Player damage | Taking a hit flashes/shakes, then a short invulnerability (about 0.5 s). HP bar drops | Screenshot |
| F8 | Run 3 minutes with no errors | Console clean; frame rate stable | Console, Stats |

## G. Recruit an Elite (first Elite appears at 1:00; Elite warning in the HUD)

Shortcut: for a faster test, temporarily lower `eliteFirst` in `StreamingAssets/balance.json` (restore it after, do not commit the change).

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| G1 | Elite warning at about 0:55-1:00 | HUD warning appears; a larger (about 1.6x) enemy spawns | Screenshot |
| G2 | Kill the Elite | Remnant (stunned creature/wreck) with a glowing ring at the death spot, plus 10 Brass | Screenshot |
| G3 | Walk into the ring and stand still | Progress fills in about 2.0 s. Ring shows recruit state | Video |
| G4 | Leave the ring at 50% and return | Progress paused (not reset); completes after the remaining time | Note |
| G5 | Completion with room in the crew | A Companion appears, follows you in a circle (radius about 2.2), attacks the nearest enemy about every 0.8 s, shows an HP bar | Screenshot |
| G6 | Let a Remnant expire (12 s) | Disappears with no reward | Note |
| G7 | Fill the crew (3) and complete another | Scrap: +15 Brass, no companion. The ring looks **different** before you start (scrap vs recruit) | Screenshot of both rings |
| G8 | Companion death | Removed permanently; crew slot freed | Note |

## H. Build on a plot

Plots are on a lattice (18-unit cells; none in the start cell). Collect at least 20 Brass first.

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| H1 | Find a plot | Visible marker/ring on the ground; plot position is stable across the run | Screenshot |
| H2 | Stand on it for 1.5 s | Dwell ring fills, then the **game pauses** and the Build menu shows 3 structures with costs. Unaffordable ones are greyed | Screenshot |
| H3 | Choose an affordable structure | Brass deducted exactly, structure appears at level 1 and the game resumes | Screenshot |
| H4 | Choose "Leave" / Esc / B | Menu closes and the game resumes. It does not reopen until you walk off and return | Note |
| H5 | Stand on your own structure for 1.5 s with enough Brass | Auto-upgrade with clear feedback (cost = base cost x level), no menu | Screenshot |
| H6 | Not enough Brass | "Not enough Brass" feedback, no deduction | Screenshot |
| H7 | Each structure type works | Barricade blocks enemies (and must not trap you), Turret shoots, Mine explodes on enemy approach and rearms, Mast chains lightning, Beacon pulls enemies (6 s on / 4 s off), Workshop heals | Screenshot per type |
| H8 | Structures can be destroyed | The plot becomes empty with no refund | Note |

## I. Level-up and build cards (every input method)

Quick way to see cards: the first level-up needs only 5 XP (about 1 minute).

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| I1 | First level-up | Game pauses (enemies freeze, timer stops), 3 distinct cards: name, level, description | Screenshot |
| I2 | **Mouse**: hover highlights; click picks | Selected option applies, game resumes | Console |
| I3 | **Keyboard**: A/D or Left/Right moves the highlight (cards are horizontal), Enter/Space picks | Same | Note |
| I4 | **Gamepad**: stick/D-pad moves, A picks | Same. Esc/B must not skip a level-up unintentionally | Note |
| I5 | Two level-ups in quick succession | They resolve one after another; each pauses | Note |
| I6 | Level-up and Build menu at the same time | Only one modal at a time; the second opens after the first | Note |
| I7 | Text legible in all three languages (check again in L) | No clipping | Screenshot |
| I8 | Evolution card | When a gadget reaches max level and the partner passive is owned, an Evolve card appears | Screenshot |

## J. Pause, resume, abandon

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| J1 | Esc / P / gamepad Start | Pause panel (Resume, Options, Abandon); the run clock stops | Screenshot |
| J2 | On-screen pause button (click it; also on touch later) | Same | Note |
| J3 | Resume (menu, or Esc/B) | Continues; the same key press does not immediately re-pause | Note |
| J4 | Options from Pause > Back | Returns to **Pause**, not Title | Note |
| J5 | Click out of the Game view / switch window | Auto-pauses (focus loss) | Note |
| J6 | Abandon | Ends the run as a defeat and shows Results | Console |

## K. Results, research purchase, save

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| K1 | Let the player die (stand still), or Abandon after 3+ minutes | Results: kills, time survived, level, research points earned (RP = kills/20 + 2 x minutes) | Screenshot |
| K2 | Continue | Back to Title with attract mode running again; points shown on the Research entry | Screenshot |
| K3 | Research: buy an upgrade | Points deducted, level n/max increases, a greyed entry when you cannot afford it, "max" at the end | Screenshot |
| K4 | Stop Play, press Play again | Points and upgrade levels persist | Screenshot of Research |
| K5 | Locate the save file (Application.persistentDataPath, e.g. Linux `~/.config/unity3d/<Company>/<Product>/save.json`) | File exists and is readable JSON | Copy of file |
| K6 | **Quit the built app or exit Play, delete nothing, restart** | Settings (language, quality, volume) and progress persist | Note |
| K7 | Win condition (optional, slow): use the bot-tuned shortcut by lowering `bossTime` in balance.json to about 60 s | Boss appears with a warning; killing it shows Victory, a bonus of 50 RP, and the next world unlocks | Screenshot |

## L. Options

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| L1 | Music and SFX sliders (Left/Right, or click) | Value changes in 10% steps; audio level changes immediately | Note |
| L2 | **Quality** cycles Auto > Low > Medium > High > Ultra > Auto | Label shows the tier; Auto shows the detected tier in parentheses | Screenshot |
| L3 | Low | Frame rate cap 30, fewer lights, no shadows, no DOF, reduced bloom, lower render scale (0.75) | Screenshot |
| L4 | Medium | Cap 60, Gaussian DOF, render scale 0.9 | Screenshot |
| L5 | High / Ultra | Bokeh DOF, SMAA, higher bloom quality. Ultra has the best filtering | Screenshot |
| L6 | **DOF toggle** | On/Off visibly changes the blur. On Low it has no effect (expected) | Before/after screenshots |
| L7 | Screen shake and damage numbers toggles | Take effect in the next run/frame | Note |
| L8 | **Language EN / DE / FR** | Entire UI re-labels at once, including menu items, level-up and build texts; no raw keys like `menu.start` shown; no clipped text (DE and FR are longer) | Screenshot of each language on Title, Options, LevelUp |
| L9 | Language persists after restart (K6) | Yes | Note |
| L10 | Content names (enemy/gadget names) may be still English in DE/FR (C-06 known) | Not a bug | None |

## M. Gamepad full path (Steam Deck or any pad)

| ID | Step | What good looks like | Capture |
|---|---|---|---|
| M1 | Title to Run to LevelUp to Build to Pause to Results with the pad only | No step requires a mouse/keyboard | Note where it blocks |
| M2 | Pad connected after launch (hot-plug) | Works without restart | Note |

## N. Wrap-up

| ID | Step | What good looks like |
|---|---|---|
| N1 | Console after the whole session | No uncaught exceptions. List all warnings |
| N2 | Test Runner (B6) again | Still 67 passing |
| N3 | Commit the `ProjectSettings/`, `Packages/`, `.meta` files as BUILDING.md says (owner's decision) | Clean tree except intentional changes |
| N4 | Fill in the result table below and send it to qa-lead | |

### Result sheet

| Section | Pass / Fail / Skipped | Failed step IDs | Evidence folder |
|---|---|---|---|
| A Preparation | | | |
| B Setup | | | |
| C Boot | | | |
| D Attract | | | |
| E Menus | | | |
| F Run | | | |
| G Recruit | | | |
| H Build | | | |
| I Cards | | | |
| J Pause | | | |
| K Results/save | | | |
| L Options | | | |
| M Gamepad | | | |
| N Wrap-up | | | |

**Exit criteria for G0**: sections A-F, I, J, K pass with no S1 and no repeated Console exceptions. Anything else becomes a bug with a severity. Sections G, H, L are required before the vertical slice gate.
