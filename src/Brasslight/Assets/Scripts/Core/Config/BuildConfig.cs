using System;

namespace Vamp.Core.Config
{
    /// <summary>Engineering / build plot tuning. See design/gdd/engineering-system.md.</summary>
    [Serializable]
    public class BuildConfig
    {
        /// <summary>Size of one lattice cell; each cell (except the origin) holds one plot.</summary>
        public float latticeCellSize = 18f;
        /// <summary>Plot jitter as a fraction of the cell size (0 = centred).</summary>
        public float plotJitter = 0.3f;
        /// <summary>Radius in which the player counts as standing on the plot.</summary>
        public float plotRadius = 1.2f;
        /// <summary>Seconds of standing to build or upgrade.</summary>
        public float buildSeconds = 1.5f;
        /// <summary>Maximum structure level.</summary>
        public int maxStructureLevel = 5;
        /// <summary>Damage/HP bonus per level above 1.</summary>
        public float levelScaling = 0.35f;
        /// <summary>Number of structure options offered on an empty plot.</summary>
        public int choiceCount = 3;
        /// <summary>Collision radius of structures that block (barricade) or can be hit.</summary>
        public float structureRadius = 0.9f;
        /// <summary>Damage per second an enemy deals to a structure it touches, as a multiple of its contact damage.</summary>
        public float structureDamageFactor = 1.0f;
    }
}
