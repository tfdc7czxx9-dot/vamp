# Device Test Matrix

> Owner: qa-lead. Targets are taken from `.claude/docs/technical-preferences.md` and ADR-0002. Related: TD-05 (device perf pass) and TD-06 (Steam Deck verification).
> Device names are examples; pick the closest available hardware. OS versions and store minimums must be re-verified before each phase *(verify)*.

## 1. Quality tiers and targets

Tier behaviour in code (`QualityTier.cs`, `PostFx.cs`, `WorldView.cs`):

| Tier | Target fps | Notes from code |
|---|---|---|
| Low | 30 fps floor (frame cap 30) | No shadows, no DOF, bloom 0.35, render scale 0.75, 2 dynamic lights |
| Medium | 60 fps | Gaussian DOF, render scale 0.9, 4 lights. Post-processing budget at most 3 ms. Auto pick on Steam Deck and integrated GPUs |
| High | 60 fps | Bokeh DOF, SMAA, 8 lights |
| Ultra | 60 fps | Highest bloom filtering, 12 lights; auto only on >= 6 GB VRAM and >= 16 GB RAM |

Auto detection: mobile with >= 6000 MB RAM gets Medium, otherwise Low.
Global budgets: 16.6 ms frame; simulation at most 4 ms with 500 enemies on Steam Deck; at most 150 batches mobile / 300 PC; memory at most 1.0 GB mobile / 2 GB PC; enemy cap 500 mobile / 1000 PC.

## 2. Devices

| ID | Device class | Example hardware | Priority | Builds | Tiers to test |
|---|---|---|---|---|---|
| D1 | Steam Deck (LCD or OLED), SteamOS, Game Mode | Steam Deck 1280x800 | P0 | Linux | Auto (expect Medium), Low, High |
| D2 | Low-end Android | 3-4 GB RAM, ARM64, Android 8-10 (API 26+), e.g. Galaxy A12 / Redmi 9 class | P0 | AAB/APK | Auto (expect Low), Low, Medium |
| D3 | Current Android flagship/midrange | Android 14-16, 8 GB+, e.g. Pixel 8 / Galaxy S23 class | P0 | AAB | Auto, High |
| D4 | Older iPhone | Oldest supported by Unity 6.3 iOS minimum, e.g. iPhone 8 / SE 2 / XR class | P0 | iOS | Auto, Low, Medium |
| D5 | iPad | Current iPad plus one older (non-notch, 4:3) | P1 | iOS | Auto, High |
| D6 | Windows mid PC | 6-8 core CPU, GTX 1650 / RX 6500 class, 16 GB, 1080p | P0 | Windows | Auto (High), Medium, Ultra |
| D7 | Windows low PC (optional) | Integrated GPU, 8 GB | P2 | Windows | Auto (Medium), Low |
| D8 | macOS | Apple Silicon M1 (and an Intel Mac if supported) | P1 | macOS | Auto, High |
| D9 | Steam client Windows, offline | Same as D6 with Steam closed or offline | P0 | Windows | Auto |
| D10 | Android tablet / foldable (optional) | 16:10 and unusual aspect ratios | P2 | AAB | Auto |

## 3. Per-device checks

Mark each cell: Pass / Fail (bug id) / N/A. Record build id, date and tester.

### 3.1 Common to all devices

| # | Check | Pass criteria |
|---|---|---|
| C1 | Install and first launch | Under 10 s to the title; attract mode visible; no crash |
| C2 | Smoke checklist | Passes (`smoke-checklist.md`) |
| C3 | Frame rate in a 15-minute run at each tier | See table 3.2. Capture with the profiler/OS tool; record min, average and 1% low |
| C4 | Enemy cap respected | At most 500 (mobile) / 1000 (PC) active normal enemies; the game stays playable at the cap |
| C5 | Memory | Peak at most 1.0 GB mobile / 2 GB PC; no growth over 20 minutes |
| C6 | GC | No periodic hitches; zero per-frame allocation in the Profiler (development build) |
| C7 | Audio | Music/SFX balanced, no clipping on device speakers or headphones; mute works |
| C8 | Language EN/DE/FR | Text fits, no raw keys |
| C9 | Save persistence | Kill the app mid-run, relaunch: progress prior to the run is intact; research points are kept after Results |
| C10 | Visual correctness | No pink materials, billboards sort correctly, fog and DOF look right for the tier |

### 3.2 Performance targets by device and tier

| Device | Tier | Target | Measure |
|---|---|---|---|
| D1 Deck | Medium | 60 fps, sim at most 4 ms at 500 enemies; post at most 3 ms | Steam Deck perf overlay plus Unity Profiler, battery draw noted |
| D1 Deck | High | Record only (stretch) | |
| D2 low Android | Low | 30 fps stable, 500 enemies cap, memory under 1.0 GB | Android Studio profiler / `adb` gfxinfo |
| D2 low Android | Medium | Record only; must not be auto-selected | |
| D3 modern Android | Medium/High | 60 fps | |
| D4 older iPhone | Low/Medium | 30 fps floor at Low; 60 fps at Medium if possible | Xcode Instruments |
| D5 iPad | High | 60 fps | |
| D6 Windows | Medium/High/Ultra | 60 fps (1000 enemy cap on PC) | PresentMon / Profiler |
| D8 macOS | High | 60 fps | |

Fail criterion: average below target, or 1% lows below 90% of target. Fail on D2 at Low, or on any P0 device at its Auto tier, is an S2 bug.

### 3.3 Input, display and platform per device

| # | Check | Applies to | Pass criteria |
|---|---|---|---|
| P1 | Safe area / notch | D3, D4, D5, D10 | HUD (timer, HP, XP, Brass, pause button) and menus fully inside `Screen.safeArea`; nothing under the notch, camera cut-out or home indicator; rotates between the two landscape orientations without breaking |
| P2 | Landscape only | Mobile | Portrait never appears; rotation lock respected |
| P3 | Touch joystick | D2-D5, D10 | Floating joystick appears where the thumb lands, radius sensible on small and big screens (DPI-based), no dead zone on the first touch, stays active while the thumb slides, releases cleanly, does not start on the pause button; multi-touch and palm touches do not break it |
| P4 | Touch menus | Mobile | All targets at least 48 dp / 44 pt; tapping cards, build offers, sliders and toggles works; Left/Right option changes possible with taps; Back is reachable |
| P5 | Gamepad | D1, D6, D8, mobile with BT pad | All screens navigable; glyph/hint text correct for the controller (TD-06); Start pauses; B/east backs out; hot-plug and unplug do not crash; controller on mobile (Xbox/PS/MFi) |
| P6 | Keyboard/mouse | D6, D8, D7 | Full path, window resize, fullscreen/windowed, 1080p/1440p/4K and ultrawide scaling |
| P7 | Steam Deck specifics | D1 | 1280x800 text readable (no clipping), no mouse-only UI, Steam Input layout works, game launches in Game Mode, fits within 3 s resume |
| P8 | Suspend/resume | D1, D2-D5 | Sleep/wake (Deck), app switch, lock screen, incoming call/notification: game is paused on return (auto-pause on focus loss), timers and run state intact, audio resumes, no crash after 30 min in background |
| P9 | Steam overlay | D6, D1 | Opening the overlay pauses the run |
| P10 | Offline | D9, mobile airplane mode | Full game works with no network; Steam absent falls back to the offline backend; no error dialogs |
| P11 | App kill | Mobile | Swipe away during a run: relaunch ok, save intact (a run in progress is not required to resume) |
| P12 | Orientation, split-screen, multi-window | Android | Not supported modes handled without crashing |
| P13 | Store-specific | D2, D3, D4, D5 | Install from internal test track/TestFlight (not only sideloaded); correct app id (not the placeholder), icon, name; no unexpected permission prompts |
| P14 | Install size | Android, iOS | AAB base at most 150 MB; iOS initial at most 200 MB |

### 3.4 Thermal and battery (mobile and Deck)

| # | Check | Pass criteria |
|---|---|---|
| T1 | 20-minute continuous play at the Auto tier, from a cool start, room temperature | Frame rate stays within 15% of its first-5-minute average; never below the floor (30 fps on Low) |
| T2 | Device temperature | Not uncomfortable to hold; no OS thermal warning; no forced app close |
| T3 | Battery drain | Record % per 20 min. Flag more than 25% per 20 minutes on phones or more than 30 W average on Deck |
| T4 | Repeat T1 at 60 minutes (soak) on D2 and D4 | No memory growth over 10%, no crash |
| T5 | Low battery / battery saver mode | Game still runs, no crash; frame cap respected |

### 3.5 Results log (copy per device)

```
Device: ____  OS: ____  Build: ____  Tester: ____  Date: ____
Auto tier detected: ____
Tier tested | avg fps | 1% low | peak RAM | batches | notes
P1 P2 P3 P4 P5 P6 P7 P8 P9 P10 P11 P12 P13 P14: ____
T1 T2 T3 T4 T5: ____
Bugs filed: ____
```

## 4. Coverage by milestone

| Milestone | Required devices |
|---|---|
| G1 vertical slice | D6, plus D1 or D2 for an early signal |
| G2 beta | D1, D2, D3, D4, D6, D8, D9 |
| G3 release candidate | All P0 and P1 devices, full checks; P2 devices spot-checked |

If a device is unavailable, document the gap and the closest substitute. Do not mark that row as passed. Emulators and simulators do not count for performance, thermals, touch feel or safe area.
