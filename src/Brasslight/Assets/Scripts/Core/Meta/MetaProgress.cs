using System;
using System.Collections.Generic;
using Vamp.Core.Config;
using Vamp.Core.Simulation;

namespace Vamp.Core.Meta
{
    /// <summary>Rules for spending research points, unlocks and recording run results.</summary>
    public sealed class MetaProgress
    {
        private readonly BalanceConfig _config;

        /// <summary>The save being modified.</summary>
        public SaveData Save { get; }

        /// <summary>Wraps a save (null creates a new one) and repairs missing fields.</summary>
        public MetaProgress(BalanceConfig config, SaveData save)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Save = save ?? new SaveData();
            Migrate(Save);
        }

        /// <summary>Repairs null collections and upgrades old formats in place.</summary>
        public static void Migrate(SaveData s)
        {
            s.upgrades ??= new List<UpgradeLevel>();
            s.wonWorlds ??= new List<string>();
            s.unlockedLeaders ??= new List<string>();
            s.recruitedSpecies ??= new List<string>();
            s.achievements ??= new List<string>();
            s.seenSpecies ??= new List<string>();
            s.settings ??= new SettingsData();
            s.selectedLeader ??= "";
            s.selectedWorld ??= "";
            if (s.researchPoints < 0) s.researchPoints = 0;
            s.version = SaveData.CurrentVersion;
        }

        /// <summary>Purchased level of a research upgrade.</summary>
        public int Level(string upgradeId)
        {
            foreach (var u in Save.upgrades) if (u.id == upgradeId) return u.level;
            return 0;
        }

        /// <summary>Levels aligned with <c>config.metaUpgrades</c>.</summary>
        public int[] LevelsArray()
        {
            var a = new int[_config.metaUpgrades.Length];
            for (int i = 0; i < a.Length; i++) a[i] = Level(_config.metaUpgrades[i].id);
            return a;
        }

        /// <summary>Cost of the next level: round(baseCost * growth^level), or -1 if maxed.</summary>
        public int NextCost(MetaUpgradeDef def)
        {
            int lvl = Level(def.id);
            if (lvl >= def.maxLevel) return -1;
            return (int)MathF.Round(def.baseCost * MathF.Pow(def.costGrowth, lvl));
        }

        /// <summary>Buys the next level if affordable. Returns success.</summary>
        public bool Buy(MetaUpgradeDef def)
        {
            int cost = NextCost(def);
            if (cost < 0 || Save.researchPoints < cost) return false;
            Save.researchPoints -= cost;
            foreach (var u in Save.upgrades)
            {
                if (u.id != def.id) continue;
                u.level++;
                return true;
            }
            Save.upgrades.Add(new UpgradeLevel { id = def.id, level = 1 });
            return true;
        }

        /// <summary>True if a world can be selected.</summary>
        public bool IsWorldUnlocked(WorldDef w) => string.IsNullOrEmpty(w.unlockedByWorld) || Save.wonWorlds.Contains(w.unlockedByWorld);

        /// <summary>True if a leader can be selected.</summary>
        public bool IsLeaderUnlocked(LeaderDef l) => l.unlockCost <= 0 || Save.unlockedLeaders.Contains(l.id);

        /// <summary>Unlocks a leader with research points. Returns success.</summary>
        public bool UnlockLeader(LeaderDef l)
        {
            if (IsLeaderUnlocked(l) || Save.researchPoints < l.unlockCost) return false;
            Save.researchPoints -= l.unlockCost;
            Save.unlockedLeaders.Add(l.id);
            return true;
        }

        /// <summary>Builds run modifiers for a leader using the purchased research.</summary>
        public RunModifiers CreateModifiers(int leaderIndex) => RunModifiers.Create(_config, leaderIndex, LevelsArray());

        /// <summary>Records a finished run and awards its research points. Returns the points awarded.</summary>
        public int RecordRun(GameSimulation sim, IEnumerable<int> recruitedDefs = null, IEnumerable<int> seenDefs = null)
        {
            int pts = sim.ResearchPoints;
            Save.researchPoints += pts;
            Save.totalRuns++;
            Save.totalKills += sim.Stats.Kills;
            Save.totalRecruits += sim.Stats.Recruited;
            Save.totalStructures += sim.Stats.StructuresBuilt;
            Save.totalScraps += sim.Stats.Scrapped;
            if (sim.Time > Save.bestTimeSeconds) Save.bestTimeSeconds = sim.Time;
            if (sim.Stats.BossDefeated)
            {
                Save.totalVictories++;
                if (!Save.wonWorlds.Contains(sim.World.id)) Save.wonWorlds.Add(sim.World.id);
            }
            if (seenDefs != null)
                foreach (int d in seenDefs)
                {
                    string id = _config.enemies[d].id;
                    if (!Save.seenSpecies.Contains(id)) Save.seenSpecies.Add(id);
                }
            if (recruitedDefs != null)
                foreach (int d in recruitedDefs)
                {
                    string id = _config.enemies[d].id;
                    if (!Save.recruitedSpecies.Contains(id)) Save.recruitedSpecies.Add(id);
                }
            return pts;
        }
    }
}
