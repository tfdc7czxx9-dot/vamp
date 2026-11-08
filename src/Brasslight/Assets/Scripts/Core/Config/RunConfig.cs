using System;

namespace Vamp.Core.Config
{
    /// <summary>Run pacing, spawning and drop tuning. See design/gdd/horde-and-progression.md.</summary>
    [Serializable]
    public class RunConfig
    {
        /// <summary>Time at which the boss spawns (seconds).</summary>
        public float bossTimeSeconds = 900f;
        /// <summary>Maximum simultaneous enemies on mobile.</summary>
        public int enemyCapMobile = 500;
        /// <summary>Maximum simultaneous enemies on PC.</summary>
        public int enemyCapPc = 1000;
        /// <summary>Inner radius of the spawn ring around the player.</summary>
        public float spawnRingMin = 14f;
        /// <summary>Outer radius of the spawn ring around the player.</summary>
        public float spawnRingMax = 18f;
        /// <summary>Enemies farther than this are relocated to the spawn ring.</summary>
        public float despawnDistance = 30f;
        /// <summary>Spawn rate r(m) = base + linear*m + quadratic*m^2 enemies per second.</summary>
        public float spawnBase = 1.0f;
        /// <summary>See <see cref="spawnBase"/>.</summary>
        public float spawnLinear = 0.45f;
        /// <summary>See <see cref="spawnBase"/>.</summary>
        public float spawnQuadratic = 0.02f;
        /// <summary>Enemy HP multiplier = 1 + hpPerMinute * m.</summary>
        public float hpPerMinute = 0.10f;
        /// <summary>First elite spawn time (seconds).</summary>
        public float eliteFirstAt = 60f;
        /// <summary>Seconds between elites.</summary>
        public float eliteInterval = 75f;
        /// <summary>Elite HP multiplier relative to base type.</summary>
        public float eliteHpMultiplier = 8f;
        /// <summary>Elite size multiplier relative to base type.</summary>
        public float eliteSizeMultiplier = 1.6f;
        /// <summary>Chance a normal enemy drops 1 brass.</summary>
        public float brassDropChance = 0.25f;
        /// <summary>Brass dropped by an elite.</summary>
        public int eliteBrassDrop = 10;
        /// <summary>Chance a normal enemy drops a healing tea tin.</summary>
        public float healDropChance = 0.005f;
        /// <summary>HP restored by a tea tin.</summary>
        public float healAmount = 25f;
        /// <summary>XP multiplier for elites.</summary>
        public float eliteXpMultiplier = 5f;
    }
}
