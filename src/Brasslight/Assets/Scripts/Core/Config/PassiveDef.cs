using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one passive upgrade.</summary>
    [Serializable]
    public class PassiveDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Short description.</summary>
        public string description;
        /// <summary>Stat affected, parsed into <see cref="Kind"/>.</summary>
        public string kind;
        /// <summary>Effect per level.</summary>
        public float valuePerLevel = 0.1f;
        /// <summary>Maximum level.</summary>
        public int maxLevel = 5;

        /// <summary>Resolved kind.</summary>
        [NonSerialized] public PassiveKind Kind;
    }
}
