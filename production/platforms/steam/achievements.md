# Steam Achievements: BRASSLIGHT (codename VAMP)

> Owner: `steam-publishing-specialist`. Status: **PROPOSAL v0.1, 2026-11-08**, needs
> `game-designer` and owner review. Launch scope is 40 achievements
> (`design/gdd/game-concept.md`, Launch Scope).
> Source of truth for IDs: `assets/data/achievements.json` (does not exist yet; generate it
> from this table, same `id` strings). Only the `Vamp.Platform.Steam` assembly maps these to
> Steamworks; gameplay raises platform-neutral events through `IPlatformServices`.

## Conventions

- API name: `ACH_` + UPPER_SNAKE_CASE, stable forever once released in Steamworks (renaming is not possible after publish; verify live in Steamworks docs). Use them identically on Game Center / Play Games mapping tables.
- Display names and descriptions are localized in Steamworks (EN, DE, FR at launch). English shown here. Tone: Victorian expedition journal; no gore language.
- "Hidden" = description is hidden until unlocked (use sparingly; 3 proposed below).
- "Run" = one expedition (15-20 min). "Lifetime" = cross-run counter stored in a Steam **stat** (stat API name in the Stat column) and in the local save; the unlock is evaluated locally and mirrored, so offline play still works and syncs on next init.
- Numeric thresholds are proposals; final values live in data (`assets/data/achievements.json`), never hard-coded in MonoBehaviours.
- Names of the six leaders are placeholders: "Engineer" and "Naturalist" appear in the concept; others TBD. Achievements reference leaders by role, not name, until the roster is final.
- Structure and elite names are examples from the concept (Steam Turret, Lightning Mast, Martian walker); update when the content GDDs are final.
- Art: two icons per achievement (unlocked, locked/grey), sizes and format per Steamworks (**verify live** before art production). Owner: `art-director`.
- Distribution target: mix of easy (~35%), medium (~40%), hard (~20%), hidden/rare (~5%).

## Stats (Steamworks Stats, int unless stated)

| Stat API name | Meaning |
|---------------|---------|
| `STAT_KILLS_TOTAL` | Lifetime enemy kills |
| `STAT_RECRUITS_TOTAL` | Lifetime companions recruited |
| `STAT_SCRAPS_TOTAL` | Lifetime Remnants scrapped for Brass |
| `STAT_STRUCTURES_BUILT` | Lifetime structures built |
| `STAT_BOSSES_DEFEATED` | Lifetime bosses defeated |
| `STAT_BESTIARY_ENEMIES` | Distinct enemy types discovered |
| `STAT_BESTIARY_RECRUITS` | Distinct recruitable species recruited |
| `STAT_RUNS_COMPLETED` | Runs finished (win or loss) |

## Achievements

### Combat and kills (6)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 1 | `ACH_KILLS_100` | First Volley | Defeat 100 enemies. | `STAT_KILLS_TOTAL >= 100` | Easy |
| 2 | `ACH_KILLS_1K` | Steam and Thunder | Defeat 1,000 enemies. | `STAT_KILLS_TOTAL >= 1000` | Easy |
| 3 | `ACH_KILLS_10K` | Ten Thousand Leagues | Defeat 10,000 enemies. | `STAT_KILLS_TOTAL >= 10000` | Medium |
| 4 | `ACH_KILLS_100K` | A Century of Steam | Defeat 100,000 enemies. | `STAT_KILLS_TOTAL >= 100000` | Hard |
| 5 | `ACH_RUN_KILLS_1000` | Swarm Breaker | Defeat 1,000 enemies in a single expedition. | Run kills >= 1000 | Medium |
| 6 | `ACH_RUN_SURVIVE_15` | Fifteen Minutes of Fame | Survive until the 15-minute mark in any world. | Run timer reaches the boss mark (15:00) alive | Easy |

### Recruiting (6)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 7 | `ACH_RECRUIT_FIRST` | Fresh Recruit | Recruit your first Companion. | `STAT_RECRUITS_TOTAL >= 1` | Easy |
| 8 | `ACH_RECRUIT_FULL_CREW` | Full Complement | Fill your crew to capacity. | Companion count == crew capacity at any time in a run | Easy |
| 9 | `ACH_RECRUIT_25` | Menagerie | Recruit 25 Companions. | `STAT_RECRUITS_TOTAL >= 25` | Medium |
| 10 | `ACH_RECRUIT_UNDER_FIRE` | Steady Nerve | Complete a recruitment with at least 20 enemies nearby. | Recruit completes while enemy count within configured radius >= 20 | Medium |
| 11 | `ACH_SCRAP_10` | Salvage Rights | Scrap 10 Remnants for Brass. | `STAT_SCRAPS_TOTAL >= 10` | Easy |
| 12 | `ACH_COMPANION_LEVEL_10` | Seasoned Hand | Have a Companion reach level 10. | Any companion level >= 10 | Medium |

### Engineering (7)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 13 | `ACH_BUILD_FIRST` | Bolted Together | Build your first structure. | `STAT_STRUCTURES_BUILT >= 1` | Easy |
| 14 | `ACH_BUILD_ALL_TYPES` | Master Engineer | Build every available structure type in one expedition. | One of each structure type built within a single run (types available in that world) | Medium |
| 15 | `ACH_STRUCTURE_MAX` | Overbuilt | Upgrade a structure to its maximum level. | Any structure reaches max level | Medium |
| 16 | `ACH_STRUCTURE_KILLS_500` | Let the Machines Work | Have structures defeat 500 enemies in one expedition. | Run kills attributed to structures >= 500 | Medium |
| 17 | `ACH_WORKSHOP_REPAIR` | Field Surgeon | Repair 5,000 Companion HP with Field Workshops in one expedition. | Run workshop healing >= 5000 | Medium |
| 18 | `ACH_FORTRESS_RUN` | Fortress Run | Survive to 15:00 with at least 8 structures standing. | Alive at boss mark with >= 8 structures alive | Hard |
| 19 | `ACH_SYNERGY_EVOLUTION` | Lightning Tripod | Trigger a crew-and-structure evolution. | First evolution that combines a Companion with a structure (e.g., Martian walker + Lightning Mast) | Medium |

### Worlds (5)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 20 | `ACH_WORLD_HOLLOW_EARTH` | Centre of the Earth | Complete The Hollow Earth. | Defeat world 1 boss | Medium |
| 21 | `ACH_WORLD_DEEP_SEA` | Twenty Thousand Leagues | Complete Twenty Thousand Leagues Down. | Defeat world 2 boss | Medium |
| 22 | `ACH_WORLD_MARS` | War of the Worlds | Complete the Martian Invasion. | Defeat world 3 boss | Medium |
| 23 | `ACH_WORLD_MOON` | Lunar Landing | Complete the Lunar Caverns. | Defeat world 4 boss | Medium |
| 24 | `ACH_WORLDS_ALL` | Around the Impossible World | Complete all four voyages. | Achievements 20-23 unlocked | Hard |

### Bosses (4)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 25 | `ACH_BOSS_FLAWLESS` | Not a Scratch | Defeat a boss without taking damage during the boss fight. | Boss fight start to boss death with zero player damage | Hard |
| 26 | `ACH_BOSS_FAST` | Overwhelming Force | Defeat a boss within 60 seconds of its arrival. | Boss kill time <= 60 s from spawn | Hard |
| 27 | `ACH_BOSS_FULL_CREW` | Together to the End | Defeat a boss with a full crew still alive. | Boss dies while companion count == capacity | Medium |
| 28 | `ACH_BOSS_BY_STRUCTURES` | Hoist by Their Own Petard | Have structures deal most of the damage to a boss. | Structure damage share > 50% of boss HP | Medium |

### Leaders (5)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 29 | `ACH_LEADER_FIRST_UNLOCK` | A New Face in the Party | Unlock your first additional Expedition Leader. | Leaders unlocked >= 2 | Easy |
| 30 | `ACH_LEADERS_UNLOCK_ALL` | Full Roster | Unlock all six Expedition Leaders. | Leaders unlocked == 6 | Hard |
| 31 | `ACH_LEADERS_WIN_EACH` | Everyone Has Their Day | Complete a voyage with each Expedition Leader. | At least one world clear per leader (6 of 6) | Hard |
| 32 | `ACH_LEADER_ENGINEER` | Engineer's Pride | As the Engineer, have 10 structures standing at once. | Leader == Engineer and structures alive >= 10 | Medium |
| 33 | `ACH_LEADER_NATURALIST` | Beastmaster | As the Naturalist, recruit 3 Companions within 5 minutes. | Leader == Naturalist and 3 recruits within a 300 s window | Medium |

### Bestiary and progression (7)

| # | API name | Display name | Description | Unlock condition | Diff. |
|---|----------|--------------|-------------|------------------|-------|
| 34 | `ACH_BESTIARY_10` | Field Notes | Discover 10 creatures in the Bestiary. | `STAT_BESTIARY_ENEMIES >= 10` | Easy |
| 35 | `ACH_BESTIARY_ALL` | Natural Philosopher | Discover every creature in the Bestiary. | `STAT_BESTIARY_ENEMIES == total` (data-driven total) | Hard |
| 36 | `ACH_BESTIARY_RECRUIT_ALL` | The Complete Crew | Recruit every recruitable species. | `STAT_BESTIARY_RECRUITS == total` (~10 at launch) | Hard |
| 37 | `ACH_BESTIARY_WORLD_PAGE` | Fully Documented | Complete the Bestiary page of one world. | All entries of one world discovered and its elites recruited | Medium |
| 38 | `ACH_RESEARCH_FIRST` | Funding Secured | Buy your first permanent expedition upgrade. | First research-point purchase | Easy |
| 39 | `ACH_EVOLUTION_FIRST` | Evolution Complete | Evolve a gadget for the first time. | First gadget evolution in any run | Easy |
| 40 | `ACH_RUNS_50` | Seasoned Explorer | Complete 50 expeditions. | `STAT_RUNS_COMPLETED >= 50` | Medium |

### Hidden achievements

Proposal: flag #19 `ACH_SYNERGY_EVOLUTION`, #28 `ACH_BOSS_BY_STRUCTURES` and #33 `ACH_LEADER_NATURALIST` as hidden in Steamworks to preserve discovery. Owner and game-designer to confirm. Hiding is set per achievement in Steamworks, not in code.

## Distribution check

| Category | Count |
|----------|-------|
| Combat and kills | 6 |
| Recruiting | 6 |
| Engineering | 7 |
| Worlds | 5 |
| Bosses | 4 |
| Leaders | 5 |
| Bestiary and progression | 7 |
| **Total** | **40** |

## Implementation notes (for unity-specialist)

- Gameplay raises events (`EnemyKilled`, `CompanionRecruited`, `StructureBuilt`, `BossDefeated`, `RunEnded`, ...); an `AchievementTracker` in `Vamp.Core` evaluates data-driven rules and calls `IPlatformServices.UnlockAchievement(id)` / `SetStat(name, value)`. `NullPlatformServices` queues unlocks locally; Steam implementation flushes on init and calls `StoreStats`.
- Never call `StoreStats` in the hot loop; batch at run end and on unlock.
- Unlocks are idempotent; local save is the source of truth for progress; Steam is a mirror.
- Tests (EditMode): threshold edge cases (99 vs 100 kills), run-scoped counters reset between runs, offline queue replay, all 40 IDs in JSON match this file.
- Steam Deck / offline: unlock popups use the Steam overlay on Steam, and an in-game toast always (so the Null path also shows feedback).
- Steamworks setup: define all achievements and stats in the Steamworks "Stats & Achievements" page, publish, then test with the `internal` branch.
- Cross-store mirroring (Game Center, Play Games) is tracked separately in the iOS/Android checklists, using the same IDs in a mapping table.
