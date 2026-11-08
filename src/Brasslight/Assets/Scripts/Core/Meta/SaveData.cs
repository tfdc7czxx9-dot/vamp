using System;
using System.Collections.Generic;

namespace Vamp.Core.Meta
{
    /// <summary>Everything persisted between sessions. Serialised as JSON by the platform layer.</summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Current save format version.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Format version of this save.</summary>
        public int version = CurrentVersion;
        /// <summary>Unspent research points.</summary>
        public int researchPoints;
        /// <summary>Purchased research upgrades.</summary>
        public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
        /// <summary>World ids whose boss was defeated.</summary>
        public List<string> wonWorlds = new List<string>();
        /// <summary>Leader ids unlocked with research points.</summary>
        public List<string> unlockedLeaders = new List<string>();
        /// <summary>Enemy ids ever recruited (bestiary).</summary>
        public List<string> recruitedSpecies = new List<string>();
        /// <summary>Enemy ids ever defeated (bestiary discovery).</summary>
        public List<string> seenSpecies = new List<string>();
        /// <summary>Selected leader id.</summary>
        public string selectedLeader = "";
        /// <summary>Selected world id.</summary>
        public string selectedWorld = "";
        /// <summary>Lifetime runs.</summary>
        public int totalRuns;
        /// <summary>Lifetime victories.</summary>
        public int totalVictories;
        /// <summary>Lifetime kills.</summary>
        public long totalKills;
        /// <summary>Lifetime recruits.</summary>
        public int totalRecruits;
        /// <summary>Lifetime remnants scrapped.</summary>
        public int totalScraps;
        /// <summary>Achievement API names already unlocked (mirrors platform state, also for platforms without achievements).</summary>
        public List<string> achievements = new List<string>();
        /// <summary>Lifetime structures built.</summary>
        public int totalStructures;
        /// <summary>Longest survival (s).</summary>
        public float bestTimeSeconds;
        /// <summary>Options.</summary>
        public SettingsData settings = new SettingsData();
    }
}
