using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one enemy type.</summary>
    [Serializable]
    public class EnemyDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Localisation key or display name.</summary>
        public string displayName;
        /// <summary>Sprite index in the creature atlas.</summary>
        public int sprite;
        /// <summary>Base HP.</summary>
        public float hp = 10f;
        /// <summary>Move speed (units/s).</summary>
        public float speed = 2f;
        /// <summary>Contact damage per hit.</summary>
        public float damage = 5f;
        /// <summary>Collision radius.</summary>
        public float radius = 0.35f;
        /// <summary>XP dropped.</summary>
        public int xp = 1;
        /// <summary>Earliest minute this enemy appears.</summary>
        public float minMinute;
        /// <summary>Latest minute this enemy appears (inclusive; 0 = no limit).</summary>
        public float maxMinute;
        /// <summary>Relative spawn weight.</summary>
        public float weight = 1f;
        /// <summary>Can spawn as an elite (recruitable).</summary>
        public bool eliteEligible;
        /// <summary>Seconds between ranged attacks (0 = melee only).</summary>
        public float shootInterval;
        /// <summary>Damage per enemy shot.</summary>
        public float shotDamage;
        /// <summary>Enemy shot speed.</summary>
        public float shotSpeed = 7f;
        /// <summary>Shots per volley (spread; bosses fire radial rings when ≥ 8).</summary>
        public int shotCount = 1;
        /// <summary>Range within which the enemy starts shooting.</summary>
        public float shootRange = 9f;
        /// <summary>Seconds between charges (0 = never).</summary>
        public float chargeInterval;
        /// <summary>Speed multiplier while charging.</summary>
        public float chargeSpeedMultiplier = 3f;
        /// <summary>This is a world boss.</summary>
        public bool isBoss;
    }
}
