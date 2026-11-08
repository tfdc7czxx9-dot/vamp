using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>A structure built on a build plot.</summary>
    public struct Structure
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Index into <c>BalanceConfig.structures</c>.</summary>
        public int Def;
        /// <summary>Level 1..max.</summary>
        public int Level;
        /// <summary>Position (plot position).</summary>
        public Vec2 Pos;
        /// <summary>HP (ignored if MaxHp is 0).</summary>
        public float Hp;
        /// <summary>Max HP (0 = indestructible).</summary>
        public float MaxHp;
        /// <summary>Activation timer.</summary>
        public float Timer;
        /// <summary>Secondary timer (lure cycle).</summary>
        public float Cycle;
        /// <summary>Mine armed state.</summary>
        public bool Armed;
        /// <summary>Lattice cell key of its plot.</summary>
        public long CellKey;
    }
}
