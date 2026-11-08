# Systems Index: BRASSLIGHT (codename VAMP)

> **Status**: Draft 2026-11-08. Derived from `design/gdd/game-concept.md` (Core Mechanics list).
> Design order: Foundation -> Core -> Feature -> Presentation -> Polish.
> Status values: Not Started, Drafted (awaiting `/design-review`), In Review, Approved.

## Systems

| # | System | Layer | GDD | Status | Depends on |
|---|---|---|---|---|---|
| 1 | Movement and camera | Foundation | `movement-system.md` | Not Started | Input |
| 2 | Input (KB/M, gamepad, touch) | Foundation | `input-system.md` | Not Started | - |
| 3 | Enemy definitions and AI | Foundation | `enemy-definitions.md` | Not Started | Movement |
| 4 | Gadget (weapon) system and evolutions | Core | `gadget-system.md` | Not Started | Movement, Enemies |
| 5 | Horde spawner and difficulty curve | Core | `horde-and-progression.md` | Drafted | Enemies, Movement |
| 6 | XP and level-up | Core | `horde-and-progression.md` | Drafted | Horde, Gadgets |
| 7 | Run flow (timer, boss, victory/defeat) | Core | `horde-and-progression.md` | Drafted | Horde |
| 8 | Recruit and Companion | Feature | `recruit-system.md` | Drafted | Horde (Elites), Enemies, Engineering |
| 9 | Engineering (plots, Brass, structures) | Feature | `engineering-system.md` | Drafted | Horde, Enemies, Recruit |
| 10 | Meta-progression (research points, upgrades, leaders) | Feature | `horde-and-progression.md` (RP only); `meta-upgrades.md` not started | Partially drafted | Run flow, Save |
| 11 | Save system and cloud sync | Foundation | `save-system.md` | Not Started | Meta |
| 12 | HUD and menus (level-up, build menu, results) | Presentation | `design/ux/hud.md` | Not Started | All gameplay systems |
| 13 | Audio | Presentation | `sound-bible.md` | Exists (direction only) | - |
| 14 | Achievements and platform services | Polish | `achievements.md` | Not Started | Run flow, Meta |

## Dependency Map (gameplay)

```
Input -> Movement -> Enemies -> Horde/Run flow -> XP/Level-up -> Gadgets
                                   |-> Elites -> Recruit -> Companions
                                   |-> Brass  -> Engineering -> Structures
Recruit <-> Engineering (Scrap pays Brass; Workshop heals companions)
Run flow -> Meta (RP) -> Save; Meta -> Recruit (crew capacity)
```

## Cross-system shared values (single source: `balance.json`)

| Value | Default | Owner doc |
|---|---|---|
| Run length / boss time | 900 s | horde-and-progression |
| Elite first / interval | 60 s / 75 s | horde-and-progression |
| Elite HP / size | x8 / x1.6 | horde-and-progression |
| Enemy HP multiplier | 1 + 0.10 m | horde-and-progression |
| Recruit time / ring / lifetime | 2.0 s / 1.6 / 12 s | recruit-system |
| Crew capacity | 3 (+1 per meta level, max 6) | recruit-system |
| Scrap reward | 15 Brass | recruit-system |
| Build dwell / plot radius / cell | 1.5 s / 1.2 / 18 | engineering-system |
| Brass drops | 25% normal (1), Elite 10 | engineering-system |
| Research points | floor(kills/20) + 2*min + 50 if victory | horde-and-progression |

## Open items

1. Missing GDDs referenced by the drafted docs: enemy-definitions, gadget-system, movement-system, save-system, meta-upgrades, hud.
2. Each drafted GDD lists **[CONFIRM]** assumptions; resolve against the implementation, then run `/design-review` on each.
3. Run `/review-all-gdds` after the first review pass.
4. Concept file lists a bestiary and evolutions; no GDD yet (gadget-system will own evolutions).
