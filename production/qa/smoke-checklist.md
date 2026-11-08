# Smoke Checklist (10 minutes, every build before upload)

> Run on the exact artefact that will be uploaded (installed build, not the editor), on at least one
> device of the target platform. A single failure in section 0-6 means the build is NOT ready: do not
> upload, file a bug, and tell qa-lead. Section 7 items are logged as bugs but do not block alone.

Build: ________  Version: ________  Platform: ________  Device/OS: ________
Tester: ________  Date: ________  Start time: ________

## 0. Pre-checks (1 min)

- [ ] `dotnet test tests/dotnet` / CI "core-tests" green for this commit (67 cases)
- [ ] Unity EditMode tests green on the build commit
- [ ] Version and build number are correct (`BRASSLIGHT_VERSION`, `BRASSLIGHT_BUILD_NUMBER`); the app id is not the placeholder `com.yourname.brasslight` (for store uploads)
- [ ] Build size within budget (AAB base at most 150 MB; iOS at most 200 MB)
- [ ] No `steam_appid.txt` and no debug/development flag in the shipped package

## 1. Launch (1 min)

- [ ] Fresh install (delete the old save once per release candidate) launches without crash
- [ ] Title appears within 10 s; attract-mode demo is visible and moving
- [ ] No pink/black rendering, no missing text keys
- [ ] Music plays

## 2. Menus (1.5 min)

- [ ] Start, Research, Bestiary, Options, Credits each open and Back returns to the title
- [ ] Quit exists on desktop, hidden on mobile
- [ ] Expedition screen: world and leader selectors change; locked items are clearly locked
- [ ] Input works on the platform's primary input (mouse/keyboard, gamepad, or touch)

## 3. Core run (3 min)

- [ ] Begin a run on The Hollow Earth with the first leader
- [ ] Movement responds correctly (keyboard/pad/touch joystick); the camera follows
- [ ] Gadget auto-fires, enemies spawn and die, Aether and Brass are collected
- [ ] Level-up appears (pause, 3 distinct cards), pick one by the platform's input, play resumes
- [ ] Elite: warning shows, and after the kill a Remnant ring appears. Standing in it for about 2 s recruits a companion (use the dev shortcut or test build with a short `eliteFirst` if needed, never ship that file)
- [ ] Plot: standing about 1.5 s opens Build, building a structure deducts Brass and places it
- [ ] Frame rate looks stable (no stutter spikes); no console/logcat errors in development builds

## 4. Pause and lifecycle (1 min)

- [ ] Pause via key/button/pad Start; the clock stops; Resume works
- [ ] Focus loss / app switch / lock screen pauses the run, and returning shows the Pause panel with the state intact
- [ ] Abandon returns through Results to the title

## 5. Results, meta and save (1.5 min)

- [ ] Results show kills, time, level, research points (consistent with the formula for the run)
- [ ] Research purchase works (points deducted, level increases)
- [ ] Quit and relaunch: research points, upgrades and settings persist

## 6. Options (1 min)

- [ ] Quality tier cycles, changes visibly, and the frame cap/visuals follow
- [ ] DOF toggle visibly works (except on Low)
- [ ] Language EN / DE / FR re-labels the UI; no clipped text on Title and Options
- [ ] Volume sliders work

## 7. Platform extras (log only, escalate if broken)

- [ ] Desktop: windowed/fullscreen toggle (if present), Alt-Tab pauses
- [ ] Steam: runs with the Steam client closed (offline fallback) and with it open; overlay pauses
- [ ] Mobile: landscape only; HUD inside the safe area; touch targets are comfortable
- [ ] Deck: Game Mode launch, gamepad-only path, text readable at 1280x800
- [ ] Repeat a quick 2-minute run on a second quality tier

## Result

| Result | Action |
|---|---|
| All of 0-6 pass | Smoke PASS: build may go to manual QA / upload. Store report in `production/qa/smoke-YYYY-MM-DD.md` |
| Any item in 0-6 fails | Smoke FAIL: block the upload, file the bug with the severity, rebuild after the fix, restart the smoke from section 0 |

Smoke verdict: PASS / FAIL    Bugs filed: ________    Notes: ________
