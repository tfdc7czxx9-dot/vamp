using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>One presentation event (sound, VFX, damage number). Purely informational.</summary>
    public struct SimEvent
    {
        /// <summary>Event kind.</summary>
        public SimEventType Type;
        /// <summary>Primary position.</summary>
        public Vec2 A;
        /// <summary>Secondary position (e.g. lightning target) or direction.</summary>
        public Vec2 B;
        /// <summary>Numeric payload (damage, radius, amount).</summary>
        public float Value;
        /// <summary>Index payload (def index, entity index).</summary>
        public int Index;
    }
}
