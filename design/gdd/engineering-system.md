# Engineering System (Build Plots, Brass, Structures)

> **Status**: Drafted 2026-11-08 (pending `/design-review`). Defaults mirror `src/Brasslight/Assets/StreamingAssets/balance.json`.
> Items marked **[CONFIRM]** are assumptions where the supplied defaults were silent.

## 1. Overview

The world contains an infinite, deterministic lattice of **Build Plots**. Enemies drop **Brass**; the player spends it by standing on a plot for 1.5 s to choose one of six **structures** (Barricade, Steam Turret, Mine Field, Lightning Mast, Lure Beacon, Field Workshop). Standing on an existing structure for 1.5 s upgrades it (max level 5). Structures fight automatically, can be destroyed, and do not move, so each run balances holding a fortified position against roaming for XP and Remnants. All interaction is movement-only (Pillar 1).

## 2. Player Fantasy

"I bolted this together under fire." The leader as engineer: a ring of turrets and mines turns a lost position into a fortress. Aesthetics: **Fantasy**, **Expression** (layout choices), **Challenge** (choosing where to hold). SDT: Autonomy (what to build, where), Competence (a well-placed layout visibly pays off).

## 3. Detailed Rules

### 3.1 Build Plot lattice
1. The world is divided into square cells of 18 x 18 units. Cell index = `(floor(x/18), floor(z/18))` on the XZ plane.
2. Each cell holds exactly one plot, at a position derived by a deterministic hash of `(cellX, cellZ, runSeed)`, jittered inside the cell. **Cell (0,0) has no plot**, so nothing triggers at the start position.
3. Plot radius = 1.2. A plot's position and existence are a pure function of the seed, so plots can be generated lazily around the player and need no storage unless built on.
4. **[CONFIRM]** Jitter keeps the plot circle fully inside its cell (margin >= 1.2 from each edge).
5. A plot is either Empty or Occupied (holds one structure with a level and current HP). Only occupied plots need saving or tracking.

### 3.2 Brass
- Normal enemy death: 25% chance to drop 1 Brass. Elite death: drops 10 Brass (guaranteed).
- Brass is a pickup using the same pickup radius (1.0) and magnet radius (3.0) as Aether/XP.
- Scrapping a Remnant grants 15 Brass (Recruit GDD).
- Brass resets to 0 at run start and is not carried between runs.
- There is no cap on held Brass.

### 3.3 Building
1. A dwell timer starts when the player is within plot radius (distance <= 1.2) of an **Empty** plot. It resets to 0 on leaving.
2. At 1.5 s, the game **pauses** and shows 3 structures **[CONFIRM]**: the three offered are drawn from the six types without repetition (uniform). Unaffordable options are greyed and cannot be chosen.
3. The player picks an affordable structure (Brass is deducted, structure placed at level 1 with full HP) or cancels. After cancel or build, the plot cannot re-open the menu until the player has left the plot radius and re-entered.
4. If nothing is affordable the menu still opens (informing the player), then cancels.

### 3.4 Upgrading
1. Standing within plot radius of an **Occupied** plot for 1.5 s upgrades it by one level if `level < 5` and the player can afford `cost = baseCost * currentLevel`. This is automatic (no menu), with clear feedback.
2. If unaffordable or at max level, nothing happens, and the timer does not retrigger until the player leaves and re-enters.
3. On upgrade, damage-like stats and max HP scale (section 4); current HP is increased by the same absolute amount (not full-healed).

### 3.5 Structures (level 1 stats)

| Structure | Cost | Stats | Behaviour |
|---|---|---|---|
| Barricade | 10 | HP 300 | Solid; enemies cannot pass and must destroy or path around it. |
| Steam Turret | 20 | 12 dmg, 0.6 s cooldown, range 7 | Fires at the nearest enemy in range. |
| Mine Field | 15 | 40 dmg AoE radius 2.0, rearm 3 s | Detonates when an enemy enters; rearms after 3 s. Ignores the player and companions. |
| Lightning Mast | 25 | 18 dmg, chain to 4 targets, 1.2 s, range 6 | Hits nearest enemy in range, chains to up to 3 more (4 targets total). |
| Lure Beacon | 15 | HP 200; attracts enemies within 10 units | Active 6 s, inactive 4 s (period 10 s). While active, affected enemies move toward the beacon instead of the player. |
| Field Workshop | 20 | heals companions within 5 units 2% max HP/s; heals player 1 HP/s | Passive aura. |

Structures have HP (those without a listed HP use a default structure HP **[CONFIRM]**, as turrets, mines, masts and workshops can be attacked). Enemies damage structures when blocked by or in contact with them **[CONFIRM]**. At 0 HP the structure is destroyed, the plot becomes Empty, and no Brass is refunded.

### 3.6 Level scaling
Each level above 1 adds +35% of the level-1 value to damage and max HP. Rate-based stats (cooldowns, rearm, range, radius, cost) do not scale.

## 4. Formulas

- **Scale factor**: `S(L) = 1 + 0.35 * (L - 1)` (additive). L = 1..5 gives 1.00, 1.35, 1.70, 2.05, 2.40.
- **Stat at level**: `damage(L) = damage1 * S(L)`; `maxHP(L) = hp1 * S(L)`.
- **Upgrade cost** (L to L+1): `baseCost * L`. Cumulative to reach level 5: `baseCost * (1+2+3+4) = 10 * baseCost`; with initial build `11 * baseCost`.
- **DPS** (single target): Turret `12 / 0.6 = 20` at L1, 48 at L5. Lightning Mast `18 / 1.2 = 15` per target, up to 60 with 4 targets at L1 and 144 at L5. Mine `40 / 3 = 13.3` per target-burst, but AoE (radius 2.0) hits many.
- **Brass income**: expected normal-kill drop = 0.25 Brass. At 5 kills/s this is about 1.25 Brass/s. Example: ~4000 normal kills + 12 Elites in a full run is approx. 1000 + 120 = 1120 Brass.
- **Full-run affordability example**: Turret at L5 costs 220 Brass total; Mast L5 costs 275; Barricade L5 costs 110.

| Structure | Build | To L5 total |
|---|---|---|
| Barricade | 10 | 110 |
| Steam Turret | 20 | 220 |
| Mine Field | 15 | 165 |
| Lightning Mast | 25 | 275 |
| Lure Beacon | 15 | 165 |
| Field Workshop | 20 | 220 |

Range: costs 10-275, Brass income roughly 0.5 - 2 per second in mid-run.

## 5. Edge Cases

| Situation | Result |
|---|---|
| Player stands on plot while paused menu is open | Time is stopped, no dwell progress. |
| Enemy destroys a structure while the player stands on the plot | Plot becomes Empty; the dwell timer resets and the menu opens after the player leaves and re-enters. |
| Upgrade would be unaffordable | No change, no deduction, "not enough Brass" feedback. |
| Two plots whose radii overlap | Cannot occur if the jitter margin is respected (3.1.4). |
| Player at origin | Cell (0,0) has no plot, but plots in neighbouring cells may be nearby; allowed. |
| Same seed re-generated | Identical plot positions (tested). |
| Very far from origin (|x| > 1e5) | Float precision degrades; plots are computed from integer cell + float offset; run lengths make this unreachable. Clamp world bounds if needed. |
| Mine triggers with 0 enemies | Never; requires at least one enemy in trigger radius. |
| Lure Beacon destroyed during its active phase | Attraction ends immediately. |
| Multiple Lure Beacons overlap | Enemy picks the nearest active beacon. |
| Barricades enclose a plot or the player | Enemies must destroy barricades. Player movement is blocked only by barricade collision, so the player can be trapped by their own walls; Barricades must not block the player **[CONFIRM]**. |
| Degenerate: turret + Workshop camping | A fortress costs the player XP and Remnants; enemy HP scaling (+10% per minute) and the Elite cadence force movement. Monitor in playtests. |
| Degenerate: Lure Beacon + Mine Field combo | Intended synergy; protected by rearm 3 s and enemy cap. |
| Brass flood | If Brass is unspent, the upgrade ladder is the sink. Tune `upgradeCostMult` if players end runs rich. |

## 6. Dependencies

| System | Direction | Contract |
|---|---|---|
| Horde and Progression (`horde-and-progression.md`) | in | Provides enemy deaths (Brass drops), enemy HP multiplier, enemy list for targeting. |
| Recruit (`recruit-system.md`) | both | Scrap pays 15 Brass; Workshop heals companions; companions defend structures. |
| Enemy definitions (**not yet written**) | in | Enemy damage to structures, flying versus ground. |
| Movement and camera (**not yet written**) | in | Player position; Barricade collision. |
| Gadget system (**not yet written**) | none | Gadgets and structures are separate damage sources. |
| Save system (**not yet written**) | out | Mid-run structure state only if suspend/resume is supported. |
| HUD (**not yet written**) | out | Brass counter, plot dwell ring, structure menu. |

## 7. Tuning Knobs

| Knob | Category | Default | Safe range | Affects |
|---|---|---|---|---|
| `cellSize` | Gate | 18 | 12 - 28 | Plot density (one per 324 sq units). |
| `plotRadius` | Feel | 1.2 | 0.8 - 2.0 | Ease of standing on a plot. |
| `buildDwellTime` | Feel | 1.5 s | 0.8 - 3.0 | Commitment cost of building. |
| `structureCost.*` | Curve | see 3.5 | 5 - 60 | Economy pacing. |
| `upgradeCostMult` | Curve | `baseCost * level` | linear 0.5x - 2x | Late-run sink. |
| `maxStructureLevel` | Gate | 5 | 3 - 8 | Power ceiling. |
| `levelScalePct` | Curve | +35% per level | 20 - 50% | Value of upgrading versus building new. |
| `brassDropChance` | Curve | 0.25 | 0.10 - 0.50 | Overall Brass income. |
| `eliteBrass` | Curve | 10 | 5 - 25 | Reward of Elites. |
| Structure-specific numbers | Curve/Feel | see 3.5 | +/-50% | Each structure's role. |
| `lureDuration` / `lurePeriod` | Feel | 6 s / 10 s | 3-8 / 8-14 | Beacon uptime (60%). |

## 8. Acceptance Criteria

Functional:
1. For a fixed seed, plot positions for cells in [-50,50]^2 are identical across two runs and platforms; cell (0,0) has none; every plot lies fully inside its cell.
2. Standing on an empty plot for 1.5 s (+/- one tick) opens the menu and pauses the game; leaving at 1.4 s resets the timer.
3. Unaffordable options are not selectable; selecting an affordable one deducts exactly its cost.
4. Upgrading a Steam Turret from L1 to L2 costs 20 Brass; L4 to L5 costs 80 Brass; at L5 further dwell does nothing.
5. A level-5 Steam Turret deals 28.8 damage (12 x 2.4); Barricade max HP at L5 is 720.
6. Mine deals 40 AoE damage within radius 2.0 and does not re-fire for 3 s.
7. Lightning Mast hits at most 4 distinct targets per cast, at 1.2 s intervals, within range 6.
8. Lure Beacon is active for 6 s of every 10 s and affects only enemies within 10 units.
9. Field Workshop heals the player 1 HP/s and companions 2% max HP/s, within 5 units only.
10. Over 10,000 simulated normal kills (seeded), Brass dropped is within 25% +/- 2 percentage points of 2,500; each Elite drops exactly 10.
11. Destroying a structure empties its plot with no refund.

Experiential (playtest):
12. In 5 of 6 test runs, players build at least one structure before minute 3 without a tutorial prompt beyond the first-run hint.
13. At least half of test players change their route to defend or return to a structure at least once per run.
14. Players report neither "too much Brass left over" nor "never enough" as their main complaint in a majority of sessions.
