using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>A defeated elite that can be recruited or scrapped.</summary>
    public struct Remnant
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Enemy def of the elite.</summary>
        public int Def;
        /// <summary>Position (ring centre).</summary>
        public Vec2 Pos;
        /// <summary>Remaining lifetime.</summary>
        public float Life;
        /// <summary>Recruit/scrap progress 0..1.</summary>
        public float Progress;
        /// <summary>The elite's max HP (companion HP derives from it).</summary>
        public float EliteHp;
        /// <summary>The elite's contact damage (companion damage derives from it).</summary>
        public float EliteDamage;
    }
}
