using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>Pooled enemy state (struct-of-fields for cache-friendly iteration).</summary>
    public struct Enemy
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Index into <c>BalanceConfig.enemies</c>.</summary>
        public int Def;
        /// <summary>Position.</summary>
        public Vec2 Pos;
        /// <summary>Last movement direction (for sprite facing).</summary>
        public Vec2 Facing;
        /// <summary>Current HP.</summary>
        public float Hp;
        /// <summary>Max HP.</summary>
        public float MaxHp;
        /// <summary>Move speed.</summary>
        public float Speed;
        /// <summary>Collision radius.</summary>
        public float Radius;
        /// <summary>Contact damage.</summary>
        public float Damage;
        /// <summary>XP dropped.</summary>
        public int Xp;
        /// <summary>Recruitable elite.</summary>
        public bool Elite;
        /// <summary>World boss.</summary>
        public bool Boss;
        /// <summary>Hit flash timer for presentation.</summary>
        public float Flash;
        /// <summary>Remaining slow duration.</summary>
        public float Slow;
        /// <summary>Per-enemy cooldown for orbiting blade hits.</summary>
        public float BladeCooldown;
    }
}
