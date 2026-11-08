# Recruit and Companion System

> **Status**: Drafted 2026-11-08 (pending `/design-review`). Defaults mirror `src/Brasslight/Assets/StreamingAssets/balance.json`.
> Items marked **[CONFIRM]** are design assumptions where the supplied defaults were silent. Verify against the implementation.

## 1. Overview

When an Elite enemy dies it leaves a **Remnant**, a stunned beast or wrecked machine surrounded by a glowing **recruit ring**. The player stands inside the ring for the **recruit time** (2.0 s) and the Remnant becomes a **Companion**: an ally that follows the player in formation and attacks the nearest enemy. Crew size is limited by **crew capacity** (3 by default, up to 6 through meta upgrades). If the crew is full, standing in the ring **scraps** the Remnant for 15 Brass instead. No button is involved; the whole system is driven by where the player stands (Pillar 1).

## 2. Player Fantasy

"I arrived with one rattling machine and I leave at the head of a caravan." The player beats a monster, then chooses to risk standing still in the swarm to win it as an ally. Aesthetics served: **Fantasy** (expedition leader), **Expression** (crew composition), **Challenge** (recruiting under pressure) and **Relatedness** (attachment to named companions). SDT: Autonomy (recruit or scrap, which Elite to chase), Relatedness (companions as characters).

## 3. Detailed Rules

### 3.1 Remnant creation
1. Every Elite death spawns exactly one Remnant at the Elite's death position. The Elite also drops 10 Brass (see Engineering GDD).
2. A Remnant has: `ringRadius = 1.6`, `lifetime = 12.0 s`, `progress = 0.0` (range 0..1), and a reference to the Elite's type (determines companion stats and attack).
3. The Remnant is non-targetable and non-collidable. Enemies ignore it.

### 3.2 Progress accumulation
1. Each fixed tick, if the player's position is within `ringRadius` of the Remnant centre (distance <= 1.6, inclusive), `progress += dt / recruitTime`.
2. If the player is outside the ring, progress is unchanged (**pause, no decay**).
3. The lifetime timer counts down every tick regardless of player position or progress. It does not pause.
4. **[CONFIRM]** If several Remnants overlap the player, only the nearest Remnant (by centre distance, ties broken by lowest spawn id) accumulates progress that tick.
5. Taking damage does not interrupt or reset progress. The tension comes from the swarm, not from a punishment rule.

### 3.3 Completion
When `progress >= 1.0`:
- **Crew has room** (`crewCount < crewCapacity`): the Remnant is removed and a Companion is created at the Remnant position (see 3.4). A "recruited" event is raised.
- **Crew full** (`crewCount >= crewCapacity`): the Remnant is removed, the player gains 15 Brass, a "scrapped" event is raised and no Companion is created. The ring visual must differ (UX) so the player knows beforehand that this will be a scrap.
- Capacity is evaluated at the moment of completion, not when progress started.

### 3.4 Companions
- **Max HP** = 50% of the dead Elite's max HP (including the run-time HP multiplier it had when it spawned). Starts at full HP.
- **Attack**: every 0.8 s, if an enemy exists within 6 units, deal `eliteContactDamage * 1.5` to the **nearest** enemy within 6 units. No projectile; instant hit with a visual effect. The Elite's contact damage is defined by its base enemy type in the enemy data.
- **Movement**: the companion follows the player in formation on a circle of radius 2.2 around the player. Slot angles are spread evenly across the current crew count and reassigned when the crew changes. Companions do not block the player.
- **Regeneration**: 2% of max HP per second while within 5 units of a friendly Field Workshop. No other passive regeneration.
- **Damage taken**: companions take enemy contact damage like the player but have no invulnerability window **[CONFIRM]** (or reuse 0.5 s; implementation decides, document the result).
- **Death**: at 0 HP the companion is removed permanently and the crew slot is freed. Its Remnant does not respawn.

### 3.5 Crew capacity
`crewCapacity = 3 + crewCapacityLevel`, where `crewCapacityLevel` is a meta upgrade in 0..3 (max capacity 6). There is no in-run way to change capacity.

## 4. Formulas

| Symbol | Meaning | Default | Range |
|---|---|---|---|
| `T_rec` | recruit time | 2.0 s | 1.0 - 4.0 |
| `R_ring` | ring radius | 1.6 | 1.0 - 3.0 |
| `L_rem` | Remnant lifetime | 12 s | 6 - 20 |
| `H_e` | Elite max HP = `baseHP * 8 * (1 + 0.10*m)` | - | - |
| `D_e` | Elite contact damage (type data) | per type | - |

- **Progress per tick**: `progress' = min(1, progress + dt / T_rec)` if `dist(player, remnant) <= R_ring`.
- **Companion max HP**: `H_c = 0.5 * H_e`.
- **Companion DPS**: `DPS_c = (D_e * 1.5) / 0.8 = 1.875 * D_e`.
- **Companion regen**: `+0.02 * H_c` HP per second near a Workshop.
- **Crew capacity**: `3 + crewCapacityLevel`.

Worked example (assumed Elite base HP 40, D_e = 10, killed at m = 10): `H_e = 40*8*2.0 = 640`; `H_c = 320`; `DPS_c = 18.75`; Workshop regen = 6.4 HP/s.

Time window: the player has `L_rem - T_rec = 10 s` of slack to reach the ring and, if progress is paused repeatedly, total time inside the ring needed is exactly 2.0 s.

## 5. Edge Cases

| Situation | Result |
|---|---|
| Lifetime and progress hit their limits on the same tick | Completion wins (progress is checked before expiry). |
| Remnant expires with progress < 1 | Removed with no reward. Progress is lost. |
| Crew becomes full while progress is partial (e.g. another recruit) | Outcome is decided at completion: scrap instead of recruit. |
| A companion dies while a Remnant is mid-progress | Room is available at completion, so the recruit goes ahead. |
| Capacity reduced below current crew | Cannot happen in a run (capacity is fixed per run). |
| Player dies during recruiting | Run ends; nothing carries over. |
| Elite killed by a structure or companion | Remnant still spawns. Credit source is irrelevant. |
| Elite killed at the same position as another Remnant | Both coexist; only nearest accumulates (3.2.4). |
| Player at exactly `R_ring` | Counts as inside (inclusive). |
| Boss death | No Remnant (boss ends the run). **[CONFIRM]** |
| Degenerate: scrap farming for Brass | Bounded by Elite cadence (one per 75 s, so at most 12 per run, 15 Brass each = 180 Brass). Acceptable. |
| Degenerate: kiting Elites to a safe spot | Remnant spawns where the Elite dies, so the player chooses location; the 12 s lifetime and the swarm are the counterpressure. |
| Dominant strategy: always recruit | Crew cap and companion death keep this bounded. Watch in playtests. |
| Run with 0 Elites killed | No companions; valid "pure gadget + structures" run. |

## 6. Dependencies

| System | Direction | Contract |
|---|---|---|
| Horde and Progression (`horde-and-progression.md`) | in | Provides Elite death events, HP multiplier `m`, Elite stat data. |
| Engineering (`engineering-system.md`) | both | Field Workshop heals companions; Brass is the scrap reward (this system calls `AddBrass(15)`). Structures can be defended by companions. |
| Enemy definitions (`enemy-definitions.md`, **not yet written**) | in | Per-type base HP, contact damage, companion visuals. |
| Movement and camera (`movement-system.md`, **not yet written**) | in | Player position; formation follow. |
| Meta progression (in `horde-and-progression.md`) | in | `crewCapacityLevel`. |
| HUD (`hud.md`, **not yet written**) | out | Ring progress, crew slots, scrap vs recruit indication. |

## 7. Tuning Knobs

| Knob | Category | Default | Safe range | Affects |
|---|---|---|---|---|
| `recruitTime` | Feel | 2.0 s | 1.0 - 4.0 | Risk of recruiting. Below 1.0 trivialises it; above 4.0 feels punishing on mobile. |
| `ringRadius` | Feel | 1.6 | 1.0 - 3.0 | How easily the player stays in the ring while dodging. |
| `remnantLifetime` | Gate | 12 s | 6 - 20 | Pressure to commit versus keep fighting. |
| `crewCapacityBase` | Gate | 3 | 2 - 4 | Power ceiling. |
| `crewCapacityMetaMax` | Curve | +3 (to 6) | 0 - 4 | Long-term growth. |
| `scrapBrass` | Curve | 15 | 5 - 40 | Value of the full-crew alternative. Should be roughly one cheap structure. |
| `companionHpFraction` | Curve | 0.5 | 0.3 - 1.0 | Companion durability. |
| `companionDamageMult` | Curve | 1.5 | 1.0 - 3.0 | Companion DPS relative to the Elite. |
| `companionAttackInterval` | Feel | 0.8 s | 0.4 - 1.5 | Attack rhythm. |
| `companionAttackRange` | Feel | 6 | 4 - 9 | Engagement radius. |
| `formationRadius` | Feel | 2.2 | 1.5 - 3.5 | Crowding and readability. |
| `workshopRegenPct` | Curve | 2%/s | 1 - 5 | Value of the Workshop for crews. |

All values live in `balance.json`, never in code.

## 8. Acceptance Criteria

Functional (automated, EditMode unless noted):
1. Standing in the ring for 2.0 s of simulated time (+/- one tick) yields one Companion when `crewCount < capacity`.
2. Standing in the ring for 1.0 s, leaving for 5 s, and returning: progress equals 0.5 on re-entry (no decay) and completes after 1.0 s more.
3. A Remnant never recruited disappears at 12.0 s (+/- one tick) with no reward.
4. With a full crew (3/3), completing a Remnant grants exactly 15 Brass and no Companion.
5. Companion max HP equals `0.5 * eliteMaxHp`; damage dealt per hit equals `eliteContactDamage * 1.5`; interval 0.8 s; target is the nearest enemy within 6 units; no hit when none are in range.
6. A companion within 5 units of a Workshop regains 2% max HP per second; outside, 0.
7. `crewCapacity` equals 3 at meta level 0 and 6 at meta level 3; never exceeds 6.
8. A companion that dies frees its slot; crew count decreases by one.
9. Completion on the same tick as expiry recruits.

Experiential (playtest):
10. In 5 test runs, at least 80% of testers describe recruiting as a "risky decision" rather than free or impossible.
11. Testers can tell, without help, whether a Remnant will recruit or scrap.
12. On a 6-inch phone with 3 companions and 2 structures on screen, testers can identify the player character within 1 s.
