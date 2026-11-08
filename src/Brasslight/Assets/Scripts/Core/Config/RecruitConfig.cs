using System;

namespace Vamp.Core.Config
{
    /// <summary>Recruit / companion tuning. See design/gdd/recruit-system.md.</summary>
    [Serializable]
    public class RecruitConfig
    {
        /// <summary>Radius of the remnant ring the player must stand in.</summary>
        public float ringRadius = 1.6f;
        /// <summary>Seconds of standing required to recruit or scrap.</summary>
        public float recruitSeconds = 2.0f;
        /// <summary>Seconds before an unclaimed remnant disappears.</summary>
        public float remnantLifetime = 12f;
        /// <summary>Crew capacity without meta upgrades.</summary>
        public int baseCrewCapacity = 3;
        /// <summary>Absolute crew capacity cap.</summary>
        public int maxCrewCapacity = 6;
        /// <summary>Brass gained by scrapping a remnant while the crew is full.</summary>
        public int scrapBrass = 15;
        /// <summary>Companion HP as a fraction of the elite's HP.</summary>
        public float companionHpFraction = 0.5f;
        /// <summary>Companion attack damage = elite contact damage * this.</summary>
        public float companionDamageMultiplier = 1.5f;
        /// <summary>Seconds between companion attacks.</summary>
        public float companionAttackInterval = 0.8f;
        /// <summary>Companion attack range.</summary>
        public float companionAttackRange = 6f;
        /// <summary>Radius of the follow formation around the player.</summary>
        public float formationRadius = 2.2f;
        /// <summary>Companion speed relative to the player's speed.</summary>
        public float companionSpeedMultiplier = 1.25f;
        /// <summary>Companion damage bonus per player level.</summary>
        public float companionDamagePerPlayerLevel = 0.05f;
        /// <summary>Base HP used for companion HP when the elite HP would be tiny.</summary>
        public float companionMinHp = 40f;
    }
}
