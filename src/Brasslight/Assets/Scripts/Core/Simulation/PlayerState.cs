using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>Mutable state of the expedition leader during a run.</summary>
    public class PlayerState
    {
        /// <summary>Position.</summary>
        public Vec2 Pos;
        /// <summary>Last non-zero movement direction.</summary>
        public Vec2 Facing;
        /// <summary>Current HP.</summary>
        public float Hp;
        /// <summary>Max HP.</summary>
        public float MaxHp;
        /// <summary>Remaining invulnerability.</summary>
        public float Invuln;
        /// <summary>Level.</summary>
        public int Level;
        /// <summary>XP within the current level.</summary>
        public int Xp;
        /// <summary>XP needed for the next level.</summary>
        public int XpToNext;
        /// <summary>Level-ups not yet chosen.</summary>
        public int PendingLevelUps;
        /// <summary>Brass (building resource).</summary>
        public int Brass;
        /// <summary>Revives left.</summary>
        public int RevivesLeft;
    }
}
