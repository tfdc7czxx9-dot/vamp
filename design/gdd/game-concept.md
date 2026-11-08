# Game Concept: IMPOSSIBLE VOYAGES (working title, codename VAMP)

> **Status**: Draft v0.2 — owner chose setting and signature mechanics on 2026-11-08.
> Supersedes v0.1 (vampire setting, dropped). Validate with
> `/design-review design/gdd/game-concept.md`.
>
> **IP basis**: worlds and figures come from **public-domain** novels by Jules Verne
> (†1905) and H. G. Wells (†1946), and the title is borrowed from Georges Méliès'
> film *Le Voyage à travers l'impossible* (1904, also PD). Our protagonists, art,
> designs and all new content are **original and owned by us**. Rules for what
> may and may not be used are in `design/ip-guidelines.md`. A trademark search on
> the final title is still required (`legal-compliance-specialist`).

## Elevator Pitch

A brass-and-steam survivor-like across the impossible worlds of 19th-century
science fiction. Lead an expedition to the centre of the Earth, the ocean floor,
the Moon and a Martian-invaded London. Your machines fire automatically. You
**recruit beaten elite monsters into your crew** and **build fortifications on
the battlefield** to survive ever-larger swarms.

## Core Identity

| Field | Value |
|-------|-------|
| Genre | Survivor-like roguelite ("bullet heaven") with crew-building and light tower defense, top-down 2D |
| Setting | Victorian retro sci-fi / steampunk based on public-domain Verne & Wells novels |
| Platforms | Steam (Win/macOS/Linux/Steam Deck), Android, iOS |
| Business model | Premium, one-time purchase. No ads, no IAP, no energy systems |
| Target price | Steam €4.99 / mobile €3.99 (to validate with `store-marketing-specialist`) |
| Session length | 15–20 minute runs; first run playable in < 60 s from launch |
| Controls | Movement only. Attacking, recruiting and building all happen by *where you move and stand* |
| Rating target | PEGI 7–12 / USK 6–12 (creature and machine combat, no gore) |

## Core Fantasy

Being the daring expedition leader who arrives with one rattling machine and
leaves at the head of a roaring caravan: tamed dinosaurs, hijacked Martian
walkers and Selenite workers fighting at your side, behind a ring of steam
turrets you bolted together under fire.

## Unique Hook

Two signature mechanics the genre's best-known games do not have. Both use
movement-only input:

### 1. Recruit: monsters join your crew

- When an **Elite** enemy is defeated it leaves a **Remnant**: a stunned beast or a wrecked machine, marked by a glowing ring.
- Stand inside the ring for the **recruit time** (default 2.0 s) and it joins your crew as a **Companion**. Leaving the ring pauses progress. The Remnant expires after a timeout.
- Companions keep their enemy attack pattern, follow you in formation and level up with you.
- **Crew capacity** is limited (default 3, can be upgraded). With a full crew you choose: replace a companion, or **scrap** the Remnant for building resources.
- The tension: recruiting means standing still in the middle of a swarm.

### 2. Engineering: change the battlefield

- Every map has **Build Plots** (some fixed, some appearing over time).
- Enemies drop **Brass** (cogs), a second resource next to XP.
- Stand on a plot for the **build time** (default 1.5 s) to pick 1 of 3 structures, for example Barricade, Steam Turret, Mine Field, Lightning Mast, Lure Beacon (pulls enemies in) or Field Workshop (repairs companions).
- Coming back to a structure upgrades it. Structures have HP and can be destroyed.
- The tension: structures don't move, but you and your crew must keep moving. Each run becomes a choice between holding a fortified position and roaming for XP and Remnants.

### Synergy between the two

Companions defend structures, the Workshop repairs companions, and some
evolutions combine both. For example, a recruited Martian walker next to a
Lightning Mast becomes a lightning tripod.

## Worlds (Maps), each from a public-domain source

| # | Voyage | Source (PD) | Enemies / Elites | Scope |
|---|--------|-------------|------------------|-------|
| 1 | The Hollow Earth | Verne, *Journey to the Centre of the Earth* (1864) | cave beasts, giant fungi, sea saurians, prehistoric giants | **MVP** |
| 2 | Twenty Thousand Leagues Down | Verne, *Twenty Thousand Leagues Under the Seas* (1870) | deep-sea swarms, giant cephalopods, wreck automatons | 1.0 |
| 3 | Martian Invasion | Wells, *The War of the Worlds* (1898) | Martian machines, red weed, heat-ray walkers | 1.0 |
| 4 | Lunar Caverns | Wells, *The First Men in the Moon* (1901) / Verne, *From the Earth to the Moon* | Selenite castes, moon-calves | 1.0 |
| 5 | The Far Future | Wells, *The Time Machine* (1895) | subterranean Morlock-like dwellers | post-launch |
| 6 | Island of Beasts | Wells, *The Island of Doctor Moreau* (1896) | beast-folk (tone: adventure, no body horror) | post-launch |

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

1. **Fantasy**: leading an extraordinary expedition
2. **Challenge**: surviving escalating swarms
3. **Expression**: every run builds a different crew and fortress
4. **Discovery**: new worlds, recruitable creatures, evolutions

### Key Dynamics (Emergent player behaviors)

- "Fortress runs" vs. "caravan runs" (structures vs. a large crew)
- Luring swarms through mine fields and turret lines
- Deciding which Elite to recruit and which to scrap for Brass

### Core Mechanics (Systems we build)

1. Movement & camera
2. Auto-firing gadgets (weapons) with levels and evolutions
3. Horde spawner with wave/difficulty curve, Elite spawns
4. XP (Aether crystals) → level-up choice (1 of 3)
5. **Recruit / Companion system** (Remnants, crew capacity, companion AI and formation)
6. **Engineering system** (Build Plots, Brass, structures, upgrades)
7. Meta-progression (expedition upgrades, unlockable leaders, worlds)
8. Save system with cloud sync

## Player Motivation Profile

### Primary Psychological Needs Served

- **Competence**: power growth within a run
- **Autonomy**: crew composition and fortification layout
- **Relatedness**: attachment to your recruited companions; leaderboards/achievements

### Player Type Appeal (Bartle Taxonomy)

Explorers (worlds, creatures to recruit) and Achievers (unlocks), with Killers via score chase.

### Flow State Design

Elites arrive in rhythm, roughly every 60–90 s, so recruit decisions are regular
peaks. Build Plots give natural breathing points.

## Core Loop

### Moment-to-Moment (30 seconds)

Move, dodge, collect Aether and Brass, watch gadgets, crew and turrets fire.

### Short-Term (5-15 minutes)

Level up, recruit an Elite, build or upgrade a structure, push toward an evolution.
Each world has a boss at minute 15.

### Session-Level (30-120 minutes)

2–5 expeditions. Spend research points on permanent upgrades and unlock new leaders and worlds.

### Long-Term Progression

6 expedition leaders (original characters, each with a starting gadget and a
passive, e.g. the Engineer builds faster and the Naturalist recruits faster),
4 worlds at launch, a bestiary of recruitable creatures, evolutions and achievements.

### Retention Hooks

Bestiary completion ("recruit every species"), unlock list, "one more voyage".
No daily-login or FOMO systems (premium game).

## Game Pillars

### Pillar 1: One Thumb, Full Command

Everything, including attacking, recruiting and building, is done by moving and
standing. Same game on phone, Deck and PC.

### Pillar 2: Your Expedition, Your Story

Crew and fortifications make every run look and play differently.

### Pillar 3: Fair and Premium

No ads, no IAP, no dark patterns. Pay once, own everything, play offline.

### Anti-Pillars (What This Game Is NOT)

- Not a full RTS or base builder. Building is quick and happens in the field.
- Not a literary adaptation. The novels provide worlds and inspiration, not plot.
- Not multiplayer at launch
- Not gacha or live-service
- No gore or body horror

## MVP Scope (Vertical Slice)

- World 1 (The Hollow Earth), 1 leader, 8 gadgets, 2 evolutions
- 6 enemy types, 2 recruitable Elites, 1 boss
- 4 structure types, 4 Build Plots
- 5 permanent upgrades, local save
- Keyboard, gamepad and touch input

## Launch Scope (1.0)

- 4 worlds, 6 leaders, ~30 gadgets, ~10 evolutions, ~25 enemy types, ~10 recruitable Elites, 4 bosses, ~10 structures
- 40 Steam achievements (mirrored to Game Center / Play Games where enabled)
- Steam Cloud / iCloud / Play Games saved-game sync
- Languages: EN, DE, FR at launch (French fits the Verne setting); ES, PT-BR, JA, ZH-Hans, KO post-launch candidates

## Franchise Potential

Our own leaders, the expedition company and the art style are original IP and
can be protected as a brand. The public-domain "Extraordinary Voyages" world
offers dozens of further novels for sequels, DLC worlds or spin-offs without
any license fees.

## Technical Considerations

See ADR-0001 and `.claude/docs/technical-preferences.md`. Horde performance plus
companion AI is the top technical risk and is validated in the first prototype.

## Risks and Open Questions

- Survivor-like market is crowded. Recruit + Engineering must prove fun in the prototype.
- Companion and structure clutter on a phone screen needs strong readability (art-director, ux-designer).
- IP: avoid elements from protected adaptations (see `design/ip-guidelines.md`).
- Open: final title (trademark search), art style (pixel art vs. hand-drawn engraving style), music direction.

## Next Steps

1. Owner review of this concept
2. `/prototype recruit-and-build`: validate both mechanics and the 500-enemy performance
3. `/art-bible`, `/map-systems`, then GDDs per system (start with Recruit and Engineering)
4. Follow `production/roadmap-to-launch.md`
