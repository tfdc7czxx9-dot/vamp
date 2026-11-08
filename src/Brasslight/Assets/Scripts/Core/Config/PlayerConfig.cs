using System;

namespace Vamp.Core.Config
{
    /// <summary>Player base stats and XP curve.</summary>
    [Serializable]
    public class PlayerConfig
    {
        /// <summary>Base max HP.</summary>
        public float maxHp = 100f;
        /// <summary>Base move speed (units/s).</summary>
        public float speed = 5f;
        /// <summary>Collision radius.</summary>
        public float radius = 0.4f;
        /// <summary>Radius in which pickups are collected.</summary>
        public float pickupRadius = 1.0f;
        /// <summary>Radius in which pickups fly toward the player.</summary>
        public float magnetRadius = 3.0f;
        /// <summary>Invulnerability after being hit (s).</summary>
        public float invulnSeconds = 0.5f;
        /// <summary>Maximum gadgets carried.</summary>
        public int maxGadgets = 6;
        /// <summary>Maximum gadget level.</summary>
        public int maxGadgetLevel = 8;
        /// <summary>xp(L) = round(base + linear*(L-1) + quadratic*(L-1)^2).</summary>
        public float xpBase = 5f;
        /// <summary>See <see cref="xpBase"/>.</summary>
        public float xpLinear = 6f;
        /// <summary>See <see cref="xpBase"/>.</summary>
        public float xpQuadratic = 0.6f;
        /// <summary>Number of options offered on level-up.</summary>
        public int levelUpChoices = 3;
        /// <summary>HP restored by the fallback level-up option when nothing else is available.</summary>
        public float fallbackHeal = 30f;
    }
}
