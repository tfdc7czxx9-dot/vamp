# BRASSLIGHT Test Plan: Vertical Slice to 1.0

> Owner: qa-lead. Created 2026-11-08. Companion docs in this folder:
> `first-editor-session-checklist.md`, `device-matrix.md`, `smoke-checklist.md`.
> Targets and budgets are quoted from `.claude/docs/technical-preferences.md`. Design rules are from
> `design/gdd/{recruit-system,engineering-system,horde-and-progression}.md`.

## 1. Current state and the main risk

- The simulation (`Vamp.Core`) is covered by automated EditMode tests. These also run in plain .NET (`dotnet test tests/dotnet`) and in CI (`core-tests.yml`).
- The Unity layer (`Vamp.Game`: rendering, input, IMGUI HUD/menus, audio, save, post-processing) has **never run in Unity 6.3** (TD-01). It is only compile-checked against reference assemblies. Zero automated coverage exists for it, and none is planned beyond smoke-level PlayMode tests.
- No hardware pass has happened yet (TD-05, TD-06). Performance numbers are unverified.
- Priority order: (1) first editor session, (2) smoke on desktop, (3) device matrix, (4) balance and playtests, (5) soak.

## 2. Story type classification and evidence

| Area | Type | Evidence | Gate |
|---|---|---|---|
| Spawn curve, XP curve, RP formula, Elite schedule | Logic | EditMode tests (below) | BLOCKING |
| Recruit, scrap, companion stats | Logic | EditMode tests | BLOCKING |
| Build plots, structures, upgrade costs | Logic | EditMode tests | BLOCKING |
| Meta research, unlocks, save migration | Logic | EditMode tests | BLOCKING |
| Achievements | Logic | EditMode tests | BLOCKING |
| `balance.json` integrity | Config/Data | Test plus smoke | ADVISORY |
| Unity wiring (input to sim, screens, save to disk) | Integration | First-editor checklist and PlayMode smoke (to add) | BLOCKING for 1.0 |
| HUD and menus (IMGUI, later UI Toolkit) | UI | Manual walkthrough docs in `production/qa/evidence/` | ADVISORY |
| Visuals, VFX, DOF/bloom, game feel | Visual/Feel | Screenshots and lead sign-off | ADVISORY |
| Performance | Performance | Profiler captures per device | Release gate (section 6) |

## 3. Automated coverage (summary from `src/Brasslight/Assets/Tests/EditMode/`)

The files hold 58 test methods. Parameterised cases bring this to the 67 test cases the runner reports.

| File | Covers | Maps to |
|---|---|---|
| `core_math_test.cs` | RNG determinism, bounds, zero seed; `Vec2` normalize/clamp | Determinism foundation |
| `balance_config_test.cs` | Default config resolves; unknown gadget kind and unknown world enemy are reported; every world has a recruitable Elite | Config/Data |
| `horde_progression_test.cs` | XP curve vs GDD, spawn rate vs GDD, spawns over time, level-up queue and choice, distinct offers, resume after choice, gadget level scaling, boss kill = win, contact damage = defeat, revive (half HP once), determinism (same seed + input), enemy cap | Horde GDD AC 1, 2, 5, 7, 8, 9 |
| `recruit_system_test.cs` | Elite leaves Remnant, 2.0 s ring recruit, leaving ring pauses progress, expiry, full crew scraps for Brass, crew capacity bonus, companion HP fraction, companion attacks | Recruit GDD AC 1-5, 7 |
| `engineering_system_test.cs` | Origin cell has no plot, plot determinism and in-cell, dwell opens build choice, build costs Brass, unaffordable rejected and cancel resumes, no retrigger until leave, upgrade on own structure, level multiplier, destroyed structure frees plot | Engineering GDD AC 1-4, 11 |
| `meta_progress_test.cs` | Geometric costs, buy fails without points, max level, modifiers, victory unlocks next world and awards points, save migration repairs nulls, leader unlock | Meta and save |
| `achievement_rules_test.cs` | Lifetime kill thresholds fire once, world win, full crew | Achievements |
| `content_features_test.cs` | Evolution offer (max gadget + partner passive), no evolution without partner, evolved damage, ranged enemy shots, boss volley/charge, Lightning Mast synergy, new leader bonuses | C-01, C-04, C-05, C-02 |
| `full_run_smoke_test.cs` | Full 15-minute bot run ends cleanly; balance autopilot probes by seed | Soak-like sim regression |

### Known gaps in automated coverage (action items for lead-programmer)

These GDD acceptance criteria have no obvious test:

1. Recruit AC 6 (Workshop regen 2%/s inside 5 units), AC 8 (companion death frees slot), AC 9 (completion on the same tick as expiry recruits), and a Remnant-lifetime tolerance check of 12.0 s.
2. Engineering AC 5 (L5 Turret damage 28.8, Barricade max HP 720), AC 6 (Mine AoE and 3 s rearm), AC 7 (Mast max 4 targets), AC 8 (Beacon 6 s / 10 s duty cycle), AC 9 (Workshop heals), AC 10 (10,000-kill Brass drop rate within 25% +/- 2 points), and the 1.4 s dwell reset.
3. Horde AC 3 (61 +/- 2 spawns in the first 60 s), AC 4 (spawn distance 14-18), AC 6 (Elite times 60..885 s, 12 total), AC 10 (RP examples 76 and 280), AC 11 (clock frozen under modals).
4. Not testable in EditMode, so manual: Barricade vs player collision ([CONFIRM] in the GDD), companion formation, camera, rendering, input, save to disk, localisation display.
5. CI already has a localisation key parity check (EN/DE/FR). Keep it as a BLOCKING gate.

Rules for new tests: deterministic seeds only, no file I/O, one scenario per test, named `test_[scenario]_[expected]`.

## 4. Manual testing scope

| Area | What and how | Doc |
|---|---|---|
| First Unity open | Full walkthrough, errors captured | `first-editor-session-checklist.md` |
| Every build | 10-minute smoke | `smoke-checklist.md` |
| Devices | Per-device checks, perf, thermal | `device-matrix.md` |
| Feel | Movement speed (5 u/s), joystick dead zone and radius, recruit risk, build dwell, screen shake, damage numbers, hit feedback | Playtest sessions, `/playtest-report` |
| Visuals | Sprite billboards sort and face camera, DOF/bloom per tier, fog per world, readability of ring (recruit vs scrap), plot and structure silhouettes with 3 companions and 2 structures on a 6-inch phone | Screenshots in `production/qa/evidence/` |
| UI | All 12 screens (Title, Expedition, Research, Options, Credits, Bestiary, Pause, LevelUp, Build, Results, HUD, Loading) on mouse, keyboard, gamepad, touch; text fit in EN/DE/FR (DE and FR run long); safe area | Walkthrough per input method |
| Platform | Steam: offline, overlay pauses the game (`Platform.IsOverlayActive`), cloud save. Mobile: background/foreground auto-pause, no Quit button on mobile | Device matrix |
| Audio | Music/SFX sliders, no clipping, mix, silence on mute | Listening pass |
| Localisation | EN/DE/FR on every screen, no missing keys, no overflow; content-name translations for enemies are pending (C-06), so expect English names | Per-language pass |
| Balance | GDD experiential criteria: recruit feels risky (80% of testers), first win after ~5-8 runs, build before minute 3, median first run 6-12 min | Playtests of 5+ testers (C-08) |

### Exploratory focus areas (high regression risk)

- Modal collisions: level-up while standing on a plot, multiple queued level-ups, Elite death during a menu.
- Escape is both Cancel and Pause. Verify no same-frame reopen or double toggle (see `GameRoot.Update`).
- Pause from the HUD button vs key vs focus loss vs Steam overlay.
- Language switch coroutine mid-menu.
- Quality tier changes during a run (pipeline, lights, shadows, DOF, frame rate cap).
- Abandon run: defeat flow, RP award (0 RP at < 20 kills and < 1 minute), save write.
- Attract mode on title: restarts when the sim ends, must never award RP or affect save/achievements.

## 5. Regression scope

Regression suite = (all EditMode tests) + (smoke checklist) + (targeted manual per change).

| Change touches | Re-run |
|---|---|
| `balance.json` / `Core` balance code | `dotnet test tests/dotnet`, smoke, a full bot run, one 5-minute manual run |
| `Core` simulation | All EditMode tests plus recruit/build manual check |
| `Hud*.cs`, `GameInput.cs` | Smoke, all-screens pass on every input method |
| `PostFx.cs`, `WorldView.cs`, `QualityTier.cs` | Smoke plus perf capture on Steam Deck and low-end Android |
| `SaveStore.cs`, `Meta` | Save persistence checks (first-editor section K), migration test with an old save file, offline Steam |
| Loc files / `Loc.cs` | CI key parity, three-language pass |
| `Platform/*` | Offline backend and Steam backend on a clean machine; no Steam running |
| Build settings / Setup | Smoke on each platform build (not only the editor) |

Keep a folder of "old" `save.json` samples from each released build so migration is always tested. Never skip or disable a failing test to pass CI.

## 6. Release quality gates

### Bug severity and policy

| Sev | Definition | Policy |
|---|---|---|
| S1 | Crash, data loss (save/research points), progression blocker, soft-lock in a menu or modal, unreachable Results | Zero open at any external build (playtest, beta, store upload). Fix first, stop-ship. |
| S2 | Broken feature, severe visual glitch, controls unusable on one input method, fps below floor on a target device, wrong localisation making a screen unreadable | Zero open at beta and release. Exceptions need written producer plus QA lead approval and a patch plan. |
| S3 | Cosmetic, edge case, minor inconvenience | Fix by capacity. At release, list known issues; target fewer than 15 open and none on the critical path. |
| S4 | Polish, typos, suggestions | Backlog |

Triage: every bug has repro steps, build id, device, severity and frequency. Run `/bug-triage` weekly and before each gate. Verified fixes close via `/bug-report verify` then `close`.

### Gates by milestone

| Gate | Requirements |
|---|---|
| G0: Unity layer boots | `first-editor-session-checklist.md` sections A-N pass with no red Console errors; EditMode tests pass in the Unity Test Runner (67 cases) |
| G1: Vertical slice complete | G0, plus all Logic/Integration stories have passing tests, zero S1 and S2, smoke passes on Windows and one Android device, balance probe run completed, GDD experiential playtests (5+ testers) documented |
| G2: Closed beta / store test tracks | Zero S1, S2 with an owner and date, device matrix executed on all primary devices, 4-hour soak with no memory growth beyond budget, localisation complete, privacy and data-minimal checks |
| G3: Release candidate (each store) | Zero S1 and S2, full smoke on the exact upload artefact, device matrix green, no regressions vs previous candidate, store compliance checklists complete, sign-offs from QA lead, technical-director, producer, creative-director |

### Stability and performance gates (release)

| Metric | Threshold |
|---|---|
| Crash-free sessions | At least 99.5% in beta (offline, so measured via tester reports, platform dashboards such as Play Console vitals/Xcode Organizer, and Steam reports); 100% in the scripted smoke/soak |
| Crash-free users | At least 99% |
| ANR rate (Android) | Below the Play "bad behaviour" threshold *(verify current value)* and below 0.5% |
| Save integrity | Zero corrupted or lost saves in testing; migration test passes |
| Frame rate | 60 fps on PC, Deck and modern mobile (Medium/High); 30 fps floor on low-tier Android (Low). 1% lows not below 90% of target for more than 2 seconds in a 15-minute run |
| Frame budget | 16.6 ms total; gameplay simulation at most 4 ms with 500 enemies on Steam Deck; post-processing at most 3 ms on Medium |
| Draw calls | At most 150 batches mobile, at most 300 PC |
| Memory | At most 1.0 GB on mobile, 2 GB on PC; no growth of more than 10% between minute 5 and minute 60 in a soak |
| GC | Zero per-frame allocations in the hot loop (Profiler) |
| Build size | Android AAB base at most 150 MB; iOS initial download at most 200 MB |
| Load | Boot to playable title at most 10 s on the lowest device; first run playable in under 60 s from launch (concept) |
| Thermals | No throttle-induced fps drop below floor in a 20-minute run on mobile |

## 7. Compatibility and platform tests

See `device-matrix.md`. Also: Windows with Steam closed (offline fallback), Linux build on Steam Deck in Game Mode, macOS Apple Silicon (and Intel if supported), landscape-only on mobile, notch/safe-area, controller glyphs (TD-06), on-screen keyboard (no text entry exists today, so confirm none is needed).

## 8. Playtest coordination

- Protocol: observe without help; time to first recruit; time to first build; whether tester can predict recruit vs scrap; whether the player character is identifiable in 1 s on a 6-inch phone (Recruit AC 12).
- Questionnaire items map to GDD experiential criteria (Horde 12-13, Recruit 10-12, Engineering 12-14).
- Minimum 5 testers per round, mix of survivor-like veterans and newcomers, and at least 2 on touch.

## 9. Open questions to resolve before G1

1. The existing tests live in `src/Brasslight/Assets/Tests/EditMode`, whereas the studio standard says `tests/unit/[system]/`. `tests/dotnet` links them. Confirm this is the accepted layout, and document it as an exception.
2. Add a minimal PlayMode smoke test (boots, enters Run, steps 600 frames, no errors) once the editor session is clean, so CI can guard the Unity layer.
3. Decide the crash reporting mechanism for a data-minimal premium game (platform dashboards only vs local log export). Do not add analytics that collect personal data.
4. Confirm [CONFIRM] items that need a test once decided (Barricade vs player collision, companion i-frames, Boss Remnant).
