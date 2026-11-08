using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>A recruited elite fighting for the player.</summary>
    public struct Companion
    {
        /// <summary>Slot in use.</summary>
        public bool Active;
        /// <summary>Enemy def it was recruited from.</summary>
        public int Def;
        /// <summary>Position.</summary>
        public Vec2 Pos;
        /// <summary>Facing direction.</summary>
        public Vec2 Facing;
        /// <summary>HP.</summary>
        public float Hp;
        /// <summary>Max HP.</summary>
        public float MaxHp;
        /// <summary>Base attack damage.</summary>
        public float Damage;
        /// <summary>Time until next attack.</summary>
        public float AttackTimer;
        /// <summary>Collision radius.</summary>
        public float Radius;
    }
}
