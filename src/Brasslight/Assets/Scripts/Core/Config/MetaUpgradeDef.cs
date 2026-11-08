using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one permanent research upgrade.</summary>
    [Serializable]
    public class MetaUpgradeDef
    {
        /// <summary>Stable id (also the save-file key).</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Short description.</summary>
        public string description;
        /// <summary>Effect, parsed into <see cref="Effect"/>.</summary>
        public string effect;
        /// <summary>Effect per level.</summary>
        public float valuePerLevel;
        /// <summary>Maximum level.</summary>
        public int maxLevel = 5;
        /// <summary>cost(level) = round(baseCost * costGrowth^level), level = current level.</summary>
        public int baseCost = 20;
        /// <summary>See <see cref="baseCost"/>.</summary>
        public float costGrowth = 1.5f;

        /// <summary>Resolved effect.</summary>
        [NonSerialized] public MetaEffectKind Effect;
    }
}
