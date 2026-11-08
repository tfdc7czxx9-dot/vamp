using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one auto-firing gadget.</summary>
    [Serializable]
    public class GadgetDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Short description for the level-up card.</summary>
        public string description;
        /// <summary>Behaviour family, parsed into <see cref="Kind"/>.</summary>
        public string kind;
        /// <summary>Damage per hit at level 1.</summary>
        public float damage = 10f;
        /// <summary>Seconds between activations at level 1.</summary>
        public float cooldown = 1f;
        /// <summary>Targeting range.</summary>
        public float range = 8f;
        /// <summary>Area radius (AoE, blade size, aura).</summary>
        public float area = 1f;
        /// <summary>Projectile speed where applicable.</summary>
        public float projectileSpeed = 12f;
        /// <summary>Projectiles / blades / chain jumps at level 1.</summary>
        public int count = 1;
        /// <summary>Additional count every N levels (0 = never).</summary>
        public int countEveryLevels = 3;
        /// <summary>Enemies a projectile can pass through.</summary>
        public int pierce;
        /// <summary>Damage increase per level above 1.</summary>
        public float damagePerLevel = 0.2f;
        /// <summary>Cooldown reduction per level above 1.</summary>
        public float cooldownPerLevel = 0.04f;

        /// <summary>Resolved kind (set by <see cref="BalanceConfig.Resolve"/>).</summary>
        [NonSerialized] public GadgetKind Kind;
    }
}
