using System;
using System.Collections.Generic;

namespace Vamp.Core.Config
{
    /// <summary>
    /// Root of all tunable game data. The shipped values live in
    /// <c>Assets/StreamingAssets/balance.json</c>; <see cref="CreateDefault"/> mirrors them
    /// for tests and as a fallback. Call <see cref="Resolve"/> after loading.
    /// </summary>
    [Serializable]
    public class BalanceConfig
    {
        /// <summary>Data format version.</summary>
        public int version = 1;
        /// <summary>Run pacing and drops.</summary>
        public RunConfig run = new RunConfig();
        /// <summary>Player stats.</summary>
        public PlayerConfig player = new PlayerConfig();
        /// <summary>Recruit system.</summary>
        public RecruitConfig recruit = new RecruitConfig();
        /// <summary>Engineering system.</summary>
        public BuildConfig build = new BuildConfig();
        /// <summary>Meta currency.</summary>
        public ResearchConfig research = new ResearchConfig();
        /// <summary>All enemy types.</summary>
        public EnemyDef[] enemies = Array.Empty<EnemyDef>();
        /// <summary>All gadgets.</summary>
        public GadgetDef[] gadgets = Array.Empty<GadgetDef>();
        /// <summary>All passives.</summary>
        public PassiveDef[] passives = Array.Empty<PassiveDef>();
        /// <summary>All structures.</summary>
        public StructureDef[] structures = Array.Empty<StructureDef>();
        /// <summary>All worlds in unlock order.</summary>
        public WorldDef[] worlds = Array.Empty<WorldDef>();
        /// <summary>All leaders.</summary>
        public LeaderDef[] leaders = Array.Empty<LeaderDef>();
        /// <summary>All research upgrades.</summary>
        public MetaUpgradeDef[] metaUpgrades = Array.Empty<MetaUpgradeDef>();

        [NonSerialized] private Dictionary<string, int> _enemyIndex;
        [NonSerialized] private Dictionary<string, int> _gadgetIndex;

        /// <summary>Index of an enemy id, or -1.</summary>
        public int EnemyIndex(string id) => id != null && _enemyIndex != null && _enemyIndex.TryGetValue(id, out int i) ? i : -1;

        /// <summary>Index of a gadget id, or -1.</summary>
        public int GadgetIndex(string id) => id != null && _gadgetIndex != null && _gadgetIndex.TryGetValue(id, out int i) ? i : -1;

        /// <summary>Index of a world id, or -1.</summary>
        public int WorldIndex(string id)
        {
            for (int i = 0; i < worlds.Length; i++) if (worlds[i].id == id) return i;
            return -1;
        }

        /// <summary>Index of a leader id, or -1.</summary>
        public int LeaderIndex(string id)
        {
            for (int i = 0; i < leaders.Length; i++) if (leaders[i].id == id) return i;
            return -1;
        }

        /// <summary>
        /// Parses string enums, builds lookups and validates references.
        /// Returns a list of problems; an empty list means the config is usable.
        /// </summary>
        public List<string> Resolve()
        {
            var errors = new List<string>();
            _enemyIndex = new Dictionary<string, int>();
            _gadgetIndex = new Dictionary<string, int>();
            enemies ??= Array.Empty<EnemyDef>();
            gadgets ??= Array.Empty<GadgetDef>();
            passives ??= Array.Empty<PassiveDef>();
            structures ??= Array.Empty<StructureDef>();
            worlds ??= Array.Empty<WorldDef>();
            leaders ??= Array.Empty<LeaderDef>();
            metaUpgrades ??= Array.Empty<MetaUpgradeDef>();

            for (int i = 0; i < enemies.Length; i++)
            {
                var e = enemies[i];
                if (string.IsNullOrEmpty(e.id) || _enemyIndex.ContainsKey(e.id)) errors.Add($"enemy[{i}] missing or duplicate id");
                else _enemyIndex[e.id] = i;
                if (e.hp <= 0f) errors.Add($"enemy {e.id}: hp must be > 0");
            }
            for (int i = 0; i < gadgets.Length; i++)
            {
                var g = gadgets[i];
                if (string.IsNullOrEmpty(g.id) || _gadgetIndex.ContainsKey(g.id)) errors.Add($"gadget[{i}] missing or duplicate id");
                else _gadgetIndex[g.id] = i;
                if (!Enum.TryParse(g.kind, out g.Kind)) errors.Add($"gadget {g.id}: unknown kind '{g.kind}'");
                if (g.cooldown <= 0f) errors.Add($"gadget {g.id}: cooldown must be > 0");
            }
            foreach (var p in passives)
                if (!Enum.TryParse(p.kind, out p.Kind)) errors.Add($"passive {p.id}: unknown kind '{p.kind}'");
            foreach (var s in structures)
                if (!Enum.TryParse(s.kind, out s.Kind)) errors.Add($"structure {s.id}: unknown kind '{s.kind}'");
            foreach (var m in metaUpgrades)
                if (!Enum.TryParse(m.effect, out m.Effect)) errors.Add($"meta {m.id}: unknown effect '{m.effect}'");
            foreach (var w in worlds)
            {
                if (w.enemyIds == null || w.enemyIds.Length == 0) errors.Add($"world {w.id}: no enemies");
                else foreach (var id in w.enemyIds) if (EnemyIndex(id) < 0) errors.Add($"world {w.id}: unknown enemy '{id}'");
                if (EnemyIndex(w.bossId) < 0) errors.Add($"world {w.id}: unknown boss '{w.bossId}'");
            }
            foreach (var l in leaders)
                if (GadgetIndex(l.startingGadget) < 0) errors.Add($"leader {l.id}: unknown gadget '{l.startingGadget}'");
            if (worlds.Length == 0) errors.Add("no worlds");
            if (leaders.Length == 0) errors.Add("no leaders");
            if (structures.Length < build.choiceCount) errors.Add("fewer structures than build.choiceCount");
            return errors;
        }

        /// <summary>Creates the default balance (mirrors balance.json) and resolves it.</summary>
        public static BalanceConfig CreateDefault()
        {
            var c = new BalanceConfig
            {
                enemies = new[]
                {
                    // World 1 — The Hollow Earth
                    E("cave_crawler", "Cave Crawler", 0, 8, 2.2f, 5, 0.35f, 1, 0, 0, 5, true),
                    E("spore_puff", "Spore Puff", 1, 5, 3.0f, 3, 0.30f, 1, 0, 0, 3, false),
                    E("armoured_trilobite", "Armoured Trilobite", 2, 18, 1.6f, 8, 0.45f, 2, 2, 0, 3, true),
                    E("glow_moth", "Glow Moth", 4, 6, 4.0f, 4, 0.30f, 1, 3, 0, 3, false),
                    E("sea_saurian", "Sea Saurian", 3, 40, 2.0f, 12, 0.60f, 4, 5, 0, 2, true),
                    E("primeval_giant", "Primeval Giant", 5, 120, 1.4f, 20, 0.80f, 10, 9, 0, 0.6f, true),
                    Boss("magma_leviathan", "Magma Leviathan", 6, 6000, 1.8f, 30, 1.6f, 200),
                    // World 2 — Twenty Thousand Leagues Down
                    E("abyssal_eel", "Abyssal Eel", 7, 9, 2.8f, 5, 0.35f, 1, 0, 0, 5, false),
                    E("lantern_fish", "Lantern Fish", 8, 6, 3.4f, 4, 0.30f, 1, 0, 0, 3, false),
                    E("crab_automaton", "Crab Automaton", 9, 24, 1.8f, 9, 0.50f, 2, 2, 0, 3, true),
                    E("giant_cephalopod", "Giant Cephalopod", 10, 60, 1.9f, 14, 0.70f, 5, 5, 0, 1.5f, true),
                    E("wreck_diver", "Wreck Diver Automaton", 11, 140, 1.3f, 22, 0.80f, 12, 9, 0, 0.6f, true),
                    Boss("maelstrom_matriarch", "Maelstrom Matriarch", 12, 8000, 1.9f, 34, 1.8f, 220),
                    // World 3 — The Martian Invasion
                    E("red_weed_creeper", "Red Weed Creeper", 13, 12, 2.0f, 6, 0.35f, 1, 0, 0, 5, false),
                    E("martian_drone", "Martian Drone", 14, 8, 3.6f, 5, 0.30f, 1, 0, 0, 3, false),
                    E("handling_machine", "Handling Machine", 15, 30, 1.9f, 10, 0.55f, 3, 2, 0, 3, true),
                    E("heat_ray_walker", "Heat-Ray Walker", 16, 80, 1.6f, 16, 0.75f, 6, 5, 0, 1.5f, true),
                    E("smoke_canister", "Black Smoke Canister", 17, 20, 2.6f, 12, 0.40f, 2, 7, 0, 2, false),
                    Boss("great_engine", "The Great Engine of Mars", 18, 10000, 1.6f, 38, 2.0f, 250),
                    // World 4 — Lunar Caverns
                    E("selenite_worker", "Selenite Worker", 19, 14, 2.4f, 6, 0.35f, 1, 0, 0, 5, false),
                    E("crystal_mite", "Crystal Mite", 22, 9, 3.8f, 5, 0.30f, 1, 0, 0, 3, false),
                    E("moon_calf", "Moon-Calf", 20, 50, 1.4f, 12, 0.70f, 4, 2, 0, 2, true),
                    E("selenite_soldier", "Selenite Soldier", 21, 70, 2.2f, 15, 0.50f, 5, 5, 0, 2, true),
                    Boss("grand_lunar", "The Grand Lunar", 23, 12000, 1.5f, 40, 2.0f, 300),
                },
                gadgets = new[]
                {
                    G("steam_cannon", "Steam Cannon", "Fires brass shot at the nearest enemy.", "SteamCannon", 10, 0.9f, 10, 0.25f, 14, 1, 3, 0),
                    G("arc_coil", "Arc Coil", "Lightning leaps between nearby enemies.", "ArcCoil", 9, 1.4f, 7, 4f, 0, 3, 2, 0),
                    G("gyro_blades", "Gyro Blades", "Spinning blades orbit the expedition leader.", "GyroBlades", 7, 0.5f, 0, 2.0f, 2.4f, 2, 3, 0),
                    G("mortar", "Field Mortar", "Lobs explosive shells onto the swarm.", "Mortar", 22, 2.2f, 9, 2.0f, 0, 1, 3, 0),
                    G("flame_bellows", "Flame Bellows", "Blasts a cone of fire where you are heading.", "FlameBellows", 6, 0.3f, 3.5f, 60f, 0, 1, 0, 0),
                    G("aether_pulse", "Aether Pulse", "A crackling aura that burns and slows.", "AetherPulse", 4, 0.5f, 0, 2.5f, 0, 1, 0, 0),
                },
                passives = new[]
                {
                    P("might", "Tempered Steel", "+10% damage.", "Might", 0.10f),
                    P("haste", "Oiled Gears", "-8% gadget cooldowns.", "Haste", 0.08f),
                    P("speed", "Spring Boots", "+8% move speed.", "Speed", 0.08f),
                    P("magnet", "Lodestone", "+30% pickup magnet radius.", "Magnet", 0.30f),
                    P("plating", "Boiler Plating", "+15 max HP.", "Plating", 15f),
                    P("area", "Wide Bore", "+10% area.", "Area", 0.10f),
                    P("engineering", "Pocket Manual", "-10% build time.", "Engineering", 0.10f),
                    P("charisma", "Silver Tongue", "-10% recruit time.", "Charisma", 0.10f),
                },
                structures = new[]
                {
                    S("barricade", "Barricade", "Blocks the swarm.", "Barricade", 10, 300, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0),
                    S("steam_turret", "Steam Turret", "Shoots the nearest enemy.", "SteamTurret", 20, 150, 12, 0.6f, 7, 0, 0, 0, 0, 0, 0, 1),
                    S("mine_field", "Mine Field", "Explodes under enemies, re-arms.", "MineField", 15, 0, 40, 3f, 1.0f, 2.0f, 0, 0, 0, 0, 0, 2),
                    S("lightning_mast", "Lightning Mast", "Chain lightning to 4 targets.", "LightningMast", 25, 150, 18, 1.2f, 6, 0, 4, 0, 0, 0, 0, 3),
                    S("lure_beacon", "Lure Beacon", "Pulls the swarm toward itself.", "LureBeacon", 15, 200, 0, 1, 10, 0, 0, 6, 10, 0, 0, 4),
                    S("field_workshop", "Field Workshop", "Repairs companions and the leader.", "FieldWorkshop", 20, 150, 0, 1, 5, 0, 0, 0, 0, 0.02f, 1f, 5),
                },
                worlds = new[]
                {
                    new WorldDef { id = "hollow_earth", achievement = "ACH_WORLD_HOLLOW_EARTH", displayName = "The Hollow Earth", source = "Jules Verne, Journey to the Centre of the Earth (1864)",
                        enemyIds = new[] { "cave_crawler", "spore_puff", "armoured_trilobite", "glow_moth", "sea_saurian", "primeval_giant" },
                        bossId = "magma_leviathan", groundColor = "#2B4A47", accentColor = "#FF8A3D", fogColor = "#0E1A1C" },
                    new WorldDef { id = "ocean_depths", achievement = "ACH_WORLD_DEEP_SEA", displayName = "Twenty Thousand Leagues Down", source = "Jules Verne, Twenty Thousand Leagues Under the Seas (1870)",
                        enemyIds = new[] { "abyssal_eel", "lantern_fish", "crab_automaton", "giant_cephalopod", "wreck_diver" },
                        bossId = "maelstrom_matriarch", unlockedByWorld = "hollow_earth", groundColor = "#1D3550", accentColor = "#5FE1FF", fogColor = "#06121F" },
                    new WorldDef { id = "martian_invasion", achievement = "ACH_WORLD_MARS", displayName = "The Martian Invasion", source = "H. G. Wells, The War of the Worlds (1898)",
                        enemyIds = new[] { "red_weed_creeper", "martian_drone", "handling_machine", "heat_ray_walker", "smoke_canister" },
                        bossId = "great_engine", unlockedByWorld = "ocean_depths", groundColor = "#4A3B36", accentColor = "#FF3B30", fogColor = "#1A1210" },
                    new WorldDef { id = "lunar_caverns", achievement = "ACH_WORLD_MOON", displayName = "The Lunar Caverns", source = "H. G. Wells, The First Men in the Moon (1901)",
                        enemyIds = new[] { "selenite_worker", "crystal_mite", "moon_calf", "selenite_soldier" },
                        bossId = "grand_lunar", unlockedByWorld = "martian_invasion", groundColor = "#3B3A55", accentColor = "#B9A8FF", fogColor = "#0D0C1A" },
                },
                leaders = new[]
                {
                    new LeaderDef { id = "ines_varga", displayName = "Ines Varga, Engineer", description = "Builds 25% faster.", startingGadget = "steam_cannon", buildTimeBonus = 0.25f, sprite = 0 },
                    new LeaderDef { id = "otto_brenner", displayName = "Dr. Otto Brenner, Naturalist", description = "Recruits 25% faster.", startingGadget = "arc_coil", recruitTimeBonus = 0.25f, unlockCost = 40, sprite = 1 },
                    new LeaderDef { id = "mara_holt", displayName = "Captain Mara Holt, Aeronaut", description = "Moves 10% faster, +20 max HP.", startingGadget = "gyro_blades", speedBonus = 0.10f, maxHpBonus = 20f, unlockCost = 80, sprite = 2 },
                },
                metaUpgrades = new[]
                {
                    M("hull", "Reinforced Hull", "+10 max HP per level.", "MaxHp", 10, 5, 20, 1.5f),
                    M("munitions", "Better Munitions", "+5% damage per level.", "Might", 0.05f, 5, 25, 1.6f),
                    M("gearing", "Fine Gearing", "+4% move speed per level.", "Speed", 0.04f, 3, 20, 1.6f),
                    M("berths", "Extra Berths", "+1 crew capacity per level.", "CrewCapacity", 1, 3, 60, 2.0f),
                    M("supply_crate", "Supply Crate", "+10 brass at run start per level.", "StartingBrass", 10, 3, 15, 1.5f),
                    M("lodestone", "Lodestone Array", "+10% magnet radius per level.", "Magnet", 0.10f, 3, 15, 1.5f),
                    M("second_wind", "Second Wind", "Revive once per run at 50% HP.", "Revival", 1, 1, 120, 1f),
                },
            };
            c.Resolve();
            return c;
        }

        private static EnemyDef E(string id, string name, int sprite, float hp, float speed, float dmg, float radius, int xp,
            float minMinute, float maxMinute, float weight, bool elite) => new EnemyDef
        {
            id = id, displayName = name, sprite = sprite, hp = hp, speed = speed, damage = dmg, radius = radius, xp = xp,
            minMinute = minMinute, maxMinute = maxMinute, weight = weight, eliteEligible = elite,
        };

        private static EnemyDef Boss(string id, string name, int sprite, float hp, float speed, float dmg, float radius, int xp) => new EnemyDef
        {
            id = id, displayName = name, sprite = sprite, hp = hp, speed = speed, damage = dmg, radius = radius, xp = xp, weight = 0f, isBoss = true,
        };

        private static GadgetDef G(string id, string name, string desc, string kind, float dmg, float cd, float range, float area,
            float projSpeed, int count, int countEvery, int pierce) => new GadgetDef
        {
            id = id, displayName = name, description = desc, kind = kind, damage = dmg, cooldown = cd, range = range, area = area,
            projectileSpeed = projSpeed, count = count, countEveryLevels = countEvery, pierce = pierce,
        };

        private static PassiveDef P(string id, string name, string desc, string kind, float value) => new PassiveDef
        {
            id = id, displayName = name, description = desc, kind = kind, valuePerLevel = value, maxLevel = 5,
        };

        private static StructureDef S(string id, string name, string desc, string kind, int cost, float hp, float dmg, float cd,
            float range, float area, int chain, float active, float interval, float healPct, float playerHeal, int sprite) => new StructureDef
        {
            id = id, displayName = name, description = desc, kind = kind, cost = cost, hp = hp, damage = dmg, cooldown = cd,
            range = range, area = area, chainCount = chain, activeSeconds = active, intervalSeconds = interval,
            healPercentPerSecond = healPct, playerHealPerSecond = playerHeal, sprite = sprite,
        };

        private static MetaUpgradeDef M(string id, string name, string desc, string effect, float value, int max, int cost, float growth) => new MetaUpgradeDef
        {
            id = id, displayName = name, description = desc, effect = effect, valuePerLevel = value, maxLevel = max, baseCost = cost, costGrowth = growth,
        };
    }
}
