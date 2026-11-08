# Horde, XP, Level-Up, Run Flow and Meta-Progression

> **Status**: Drafted 2026-11-08 (pending `/design-review`). Defaults mirror `src/Brasslight/Assets/StreamingAssets/balance.json`.
> Items marked **[CONFIRM]** are assumptions where the supplied defaults were silent.

## 1. Overview

A 15-minute run in which enemies spawn on a ring around the player at a rate that grows quadratically with elapsed time, with enemy HP also growing over time. Every 75 s (first at 60 s) an **Elite** appears, which leaves a Remnant for recruiting. Killing enemies yields XP; filling the XP bar triggers a paused **level-up** where the player picks one of 3 options (new gadget, gadget level, or passive). At 15:00 the **boss** spawns; killing it is victory. At the end of every run, **research points** are awarded for permanent meta upgrades.

## 2. Player Fantasy

"The swarm keeps coming and I keep getting stronger, and I am always almost overwhelmed." Aesthetics: **Challenge** (escalating pressure), **Submission/Sensation** (screen-filling hordes and satisfying kills), **Expression** (build choices), **Discovery** (new options). SDT: Competence (visible power growth, a readable curve), Autonomy (3-way choices). Flow: a sawtooth: pressure rises, a level-up or Elite gives a release, then re-engages at a higher baseline.

## 3. Detailed Rules

### 3.1 Run flow
1. States: `Boot -> RunStart -> Playing <-> Paused (level-up, build menu, pause) -> Victory | Defeat -> Results -> Meta`.
2. At `RunStart`: time = 0, level = 1, XP = 0, Brass = 0, HP = 100, player has the leader's starting gadget, meta upgrades applied.
3. Run length target is 15:00. The **boss spawns at t = 900 s**. Killing the boss = **Victory**. Player HP reaching 0 = **Defeat**.
4. After 900 s the run continues until the boss dies or the player dies. Normal spawning continues **[CONFIRM: `m` keeps growing and is not clamped at 15]**.
5. Paused states stop the run clock, spawning, and all timers.
6. Mobile: the game auto-pauses on focus loss (technical preferences).
7. Results screen shows kills, time survived, level, research points earned.

### 3.2 Player baseline
HP 100, speed 5 units/s, pickup radius 1.0, magnet radius 3.0. After taking contact damage the player is invulnerable for 0.5 s. Pickups within 3.0 units are pulled toward the player; pickups within 1.0 are collected.

### 3.3 Enemy spawning
1. Let `m = t / 60` (elapsed minutes, continuous) **[CONFIRM: continuous rather than integer minute]**.
2. Spawn rate `r(m)` per second (formula 4.1). Each tick, `spawnAccumulator += r(m) * dt`; spawn `floor(accumulator)` enemies and keep the remainder (no rounding loss).
3. Spawn position: uniform random angle around the player, radius uniform in **[14, 18]**.
4. **Enemy cap**: 500 on mobile, 1000 on PC (data-driven). When `activeEnemies >= cap`, normal spawns are skipped (the accumulator is clamped so no burst occurs later). Elites and the boss ignore the cap **[CONFIRM]**.
5. Enemy type selection per minute is defined in the enemy data (not specified here); every spawn uses `hp = baseHP * (1 + 0.10 * m)` at spawn (fixed at spawn, not re-scaled afterwards).
6. Enemies that wander more than a despawn distance from the player are recycled to the ring **[CONFIRM]** to keep the cap useful (pooling required: zero per-frame allocations).

### 3.4 Elites
- First Elite at t = 60 s, then every 75 s: 60, 135, 210, ... up to t = 885 s (**12 Elites** per 15-minute run).
- Elite HP = `baseHP * 8 * (1 + 0.10*m)`; size scale x1.6; picks a type that is recruitable.
- Elite drops 10 Brass and leaves a Remnant (see `recruit-system.md`).
- The Elite schedule runs on the run clock (pauses with the game).

### 3.5 XP and level-up
1. Enemies drop XP (Aether crystals) as pickups. Amount per enemy type is defined in the enemy data **[CONFIRM]**.
2. XP required to go from level L to L+1: `xp(L)` (formula 4.3). Excess XP carries over.
3. Multiple level-ups from one pickup queue and resolve one at a time, each pausing the game.
4. On level-up, the game pauses and offers **3 distinct options** drawn from the eligible pool:
   - **New gadget**: only if gadgets owned < 6.
   - **Gadget level-up**: any owned gadget with level < 8.
   - **Passive**: any available passive (stacking rules per passive, in passive data).
5. Options are distinct (no duplicate entries). Selection is uniform from the eligible pool **[CONFIRM: weighting]**. If fewer than 3 are eligible, show what exists. If none, grant a fallback (heal 25% max HP) **[CONFIRM]**.
6. A reroll or skip is not part of the baseline.

### 3.6 Boss
The boss spawns at 900 s at a point on the spawn ring. Its stats come from the world's enemy data. Boss death triggers Victory immediately (all remaining enemies are cleared with a non-lethal effect, pickups auto-collected).

### 3.7 Meta-progression
1. At run end (victory, defeat or abandon), award research points (formula 4.4).
2. Research points are spent in the meta menu on permanent upgrades (e.g. crew capacity +1 per level up to +3, giving a max of 6; other upgrades are defined in their own data). Upgrade list and costs are out of scope for this GDD **[CONFIRM: separate meta-upgrades GDD]**.
3. Meta state is saved locally after the Results screen (save system, not yet written). Points are never lost on app kill after the Results screen is shown.

## 4. Formulas

### 4.1 Spawn rate
`r(m) = 1.0 + 0.45*m + 0.02*m^2` enemies/second, `m >= 0`.

| m (min) | r(m)/s | HP mult `1+0.1m` |
|---|---|---|
| 0 | 1.00 | 1.0 |
| 1 | 1.47 | 1.1 |
| 2 | 1.98 | 1.2 |
| 3 | 2.53 | 1.3 |
| 4 | 3.12 | 1.4 |
| 5 | 3.75 | 1.5 |
| 6 | 4.42 | 1.6 |
| 7 | 5.13 | 1.7 |
| 8 | 5.88 | 1.8 |
| 9 | 6.67 | 1.9 |
| 10 | 7.50 | 2.0 |
| 11 | 8.37 | 2.1 |
| 12 | 9.28 | 2.2 |
| 13 | 10.23 | 2.3 |
| 14 | 11.22 | 2.4 |
| 15 | 12.25 | 2.5 |

Cumulative spawns over 0-15 min (uncapped) = `60 * (15 + 0.225*15^2 + (0.02/3)*15^3) = 60 * 88.125 = 5,287` enemies. Combined with HP growth, total enemy HP grows faster than quadratically, which is intended to force the player to scale via level-ups, crew and structures.

### 4.2 Enemy HP
`hp_spawn = baseHP * (1 + 0.10*m)`; Elite: `baseHP * 8 * (1 + 0.10*m)`; Elite scale 1.6.

### 4.3 XP curve
`xp(L) = round(5 + 6*(L-1) + 0.6*(L-1)^2)` (XP to go from level L to L+1; L >= 1).
Because `0.6*k^2` has a fractional part that is a multiple of 0.2, `.5` ties never occur; rounding is unambiguous.

| L | xp(L) | Cumulative XP to reach L+1 |
|---|---|---|
| 1 | 5 | 5 |
| 2 | 12 | 17 |
| 3 | 19 | 36 |
| 4 | 28 | 64 |
| 5 | 39 | 103 |
| 6 | 50 | 153 |
| 7 | 63 | 216 |
| 8 | 76 | 292 |
| 9 | 91 | 383 |
| 10 | 108 | 491 |
| 20 | 336 | approx. 2,400 |

### 4.4 Research points
`RP = floor(kills / 20) + 2 * minutesSurvived + (victory ? 50 : 0)`, with `minutesSurvived = floor(runSeconds / 60)` **[CONFIRM: floor versus fractional]**.
Examples: defeat at 8:30 with 1,200 kills = 60 + 16 = 76. Victory at 15:20 with 4,000 kills = 200 + 30 + 50 = 280. Abandon at 0:30 with 10 kills = 0.

### 4.5 Elite schedule
`t_k = 60 + 75*k` for k = 0..11 (`t_k <= 885`).

## 5. Edge Cases

| Situation | Result |
|---|---|
| Enemy cap reached | Normal spawns skipped; accumulator clamped; no catch-up burst. Elites and boss still spawn. |
| `r(m) * dt` < 1 | Remainder accumulates, so no spawn is lost. |
| Long frame (dt spike, e.g. 0.5 s) | Dt is clamped to a max step (suggest 0.1 s); no spawn burst. |
| Level-up during a build menu or vice versa | Menus queue: only one modal at a time; second opens after the first closes. |
| Level-up when pool has < 3 options | Show available ones; 0 options gives fallback heal. |
| Player at max everything (6 gadgets at level 8, all passives) | XP continues, level-ups grant the fallback. |
| Player dies on the same tick as the boss | Defeat wins **[CONFIRM]**. |
| Boss alive for a long time | Spawning continues to rise; the run self-limits through pressure. |
| Kill count overflow | Use 32-bit int; 2^31 is unreachable. |
| Time near 0 | `r(0) = 1.0` per second, first 60 s has no Elite, tutorial-friendly. |
| Elite scheduled while the cap is hit | Spawns anyway. |
| Meta capacity applied twice (reload) | Capacity is recomputed from saved level, not incremented. |
| App backgrounded | Run is paused; no points are awarded until the run ends. |
| Degenerate: stalling to farm RP | RP gives only 2 per minute and 1 per 20 kills; defeat time-capped by pressure. Victory bonus (50) dominates. Fine. |
| Degenerate: abandon immediately | 0 RP. No exploit. |
| Degenerate: survive without killing | RP = 2*min; hard to do against swarm; acceptable. |

## 6. Dependencies

| System | Direction | Contract |
|---|---|---|
| Recruit (`recruit-system.md`) | out | Elite death events and Elite stats; `crewCapacityLevel` from meta. |
| Engineering (`engineering-system.md`) | out | Enemy-death events for Brass; `m`; enemy list. |
| Enemy definitions (**not yet written**) | in | Base HP, damage, XP value, type tables per minute. |
| Gadget system (**not yet written**) | both | Level-up offers gadget/passive entries; gadget levels 1-8, max 6 gadgets. |
| Movement and camera (**not yet written**) | in | Player position for the spawn ring. |
| Save system (**not yet written**) | out | Persist research points and meta upgrades. |
| HUD (**not yet written**) | out | Timer, XP bar, level, Elite warning, boss warning, results. |
| Meta upgrades (**not yet written**) | both | Spend RP; supply capacity and other modifiers. |

## 7. Tuning Knobs

| Knob | Category | Default | Safe range | Affects |
|---|---|---|---|---|
| `runDuration` / `bossTime` | Gate | 900 s | 600 - 1200 | Session length. |
| `spawn.base/linear/quad` | Curve | 1.0 / 0.45 / 0.02 | +/-30% | Pressure curve. |
| `enemyHpPerMinute` | Curve | 0.10 | 0.05 - 0.20 | Scaling versus player power. |
| `spawnRingMin/Max` | Feel | 14 / 18 | min above screen half-diagonal | Surprise versus readability. |
| `enemyCap` | Gate | 500 mobile / 1000 PC | perf-bound | Performance and density. |
| `eliteFirst` / `eliteInterval` | Gate | 60 s / 75 s | 40-90 / 50-100 | Recruit cadence. |
| `eliteHpMult` / `eliteScale` | Curve/Feel | 8 / 1.6 | 5-12 / 1.3-2.0 | Elite fight length. |
| `xp.base/linear/quad` | Curve | 5 / 6 / 0.6 | +/-30% | Level-up pace. |
| `levelUpOptions` | Feel | 3 | 3-4 | Choice width. |
| `maxGadgets` / `maxGadgetLevel` | Gate | 6 / 8 | 4-8 / 5-10 | Build depth. |
| `player.hp/speed/pickup/magnet/iframes` | Feel | 100 / 5 / 1.0 / 3.0 / 0.5 s | +/-30% | Survivability and feel. |
| `rp.perKills/perMinute/victoryBonus` | Curve | 20 / 2 / 50 | various | Meta pace. |

## 8. Acceptance Criteria

Functional (EditMode unless noted):
1. `r(m)` returns the table in 4.1 to 2 decimals at m = 0, 5, 10, 15.
2. `xp(L)` returns 5, 12, 19, 28, 39 for L = 1..5 and 108 for L = 10.
3. Over a simulated 60 s at m ~ 0 with dt = 1/60, total spawned = 61 +/- 2 (integral of r).
4. All spawned enemies lie at distance 14-18 from the player at spawn.
5. Active enemies never exceed 500 (mobile profile) or 1000 (PC profile) from normal spawns.
6. Elites spawn at 60, 135, ... 885 s (12 total in a run that reaches 900 s); none after 885 s; Elite HP = 8x base x (1 + 0.1m); scale 1.6.
7. The boss spawns at exactly t = 900 s; killing it ends the run in Victory; HP 0 ends in Defeat.
8. Each level-up presents 3 distinct options; "new gadget" never appears with 6 gadgets; gadget level-up never appears for level-8 gadgets.
9. Multiple queued level-ups resolve one at a time, game paused during each.
10. `RP(kills=1200, t=510 s, defeat) = 76`; `RP(4000, 920 s, victory) = 280`.
11. The run clock does not advance while any modal menu is open.

Experiential (playtest):
12. Median new-player first run lasts 6 - 12 min; at least 70% of experienced testers (5 runs) win a run within their first 10 attempts without meta upgrades being required beyond 2 purchases. [Tune via curve knobs.]
13. Testers report at least one clear "I'm overwhelmed but recovering" moment per run.
14. On Steam Deck at 1000 enemies the simulation stays within the 4 ms budget (`performance-analyst` to measure).
