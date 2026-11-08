using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>Kinds of pickups.</summary>
    public enum PickupKind
    {
        /// <summary>Aether crystal (XP).</summary>
        Xp,
        /// <summary>Brass cog (building resource).</summary>
        Brass,
        /// <summary>Tea tin (heal).</summary>
        Heal,
    }

    /// <summary>Pooled pickup state.</summary>
    public struct Pickup
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Kind.</summary>
        public PickupKind Kind;
        /// <summary>Position.</summary>
        public Vec2 Pos;
        /// <summary>Amount.</summary>
        public float Value;
        /// <summary>Flying toward the player.</summary>
        public bool Magnetized;
    }
}
