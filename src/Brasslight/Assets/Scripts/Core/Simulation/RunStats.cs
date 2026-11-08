namespace Vamp.Core.Simulation
{
    /// <summary>Counters for a single run (results screen, achievements, research points).</summary>
    public class RunStats
    {
        /// <summary>Enemies killed.</summary>
        public int Kills;
        /// <summary>Elites killed.</summary>
        public int EliteKills;
        /// <summary>Elites recruited.</summary>
        public int Recruited;
        /// <summary>Remnants scrapped.</summary>
        public int Scrapped;
        /// <summary>Structures built.</summary>
        public int StructuresBuilt;
        /// <summary>Structure upgrades.</summary>
        public int StructuresUpgraded;
        /// <summary>Brass collected.</summary>
        public int BrassCollected;
        /// <summary>Damage dealt.</summary>
        public double DamageDealt;
        /// <summary>Gadget evolutions this run.</summary>
        public int Evolutions;
        /// <summary>A crew + structure synergy triggered this run.</summary>
        public bool SynergyTriggered;
        /// <summary>Boss defeated.</summary>
        public bool BossDefeated;
    }
}
