using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one buildable structure.</summary>
    [Serializable]
    public class StructureDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Short description.</summary>
        public string description;
        /// <summary>Structure family, parsed into <see cref="Kind"/>.</summary>
        public string kind;
        /// <summary>Brass cost at level 1 (upgrade cost = cost * currentLevel).</summary>
        public int cost = 10;
        /// <summary>HP at level 1 (0 = indestructible).</summary>
        public float hp;
        /// <summary>Damage per activation at level 1.</summary>
        public float damage;
        /// <summary>Seconds between activations.</summary>
        public float cooldown = 1f;
        /// <summary>Targeting / effect range.</summary>
        public float range = 6f;
        /// <summary>Area radius for AoE effects.</summary>
        public float area = 2f;
        /// <summary>Chain targets for lightning.</summary>
        public int chainCount;
        /// <summary>Active duration for lure beacons (s).</summary>
        public float activeSeconds;
        /// <summary>Cycle length for lure beacons (s).</summary>
        public float intervalSeconds;
        /// <summary>Companion heal per second as a fraction of max HP.</summary>
        public float healPercentPerSecond;
        /// <summary>Player heal per second (HP).</summary>
        public float playerHealPerSecond;
        /// <summary>Sprite index in the structure atlas.</summary>
        public int sprite;

        /// <summary>Resolved kind.</summary>
        [NonSerialized] public StructureKind Kind;
    }
}
