using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>Kinds of projectiles.</summary>
    public enum ProjectileKind
    {
        /// <summary>Straight shot from a gadget.</summary>
        Bolt,
        /// <summary>Delayed area shell (mortar).</summary>
        Shell,
        /// <summary>Straight shot from a turret structure.</summary>
        TurretBolt,
    }

    /// <summary>Pooled projectile state.</summary>
    public struct Projectile
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Kind.</summary>
        public ProjectileKind Kind;
        /// <summary>Position (for shells: impact point).</summary>
        public Vec2 Pos;
        /// <summary>Velocity.</summary>
        public Vec2 Vel;
        /// <summary>Damage per hit.</summary>
        public float Damage;
        /// <summary>Hit radius (shell: explosion radius).</summary>
        public float Radius;
        /// <summary>Remaining enemies it may pass through.</summary>
        public int Pierce;
        /// <summary>Remaining lifetime / shell fuse.</summary>
        public float Life;
        /// <summary>Last enemy hit (avoid double hits).</summary>
        public int LastHit;
    }
}
