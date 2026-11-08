namespace Vamp.Core.Simulation
{
    /// <summary>A gadget carried by the player.</summary>
    public struct OwnedGadget
    {
        /// <summary>Index into <c>BalanceConfig.gadgets</c>.</summary>
        public int Def;
        /// <summary>Level 1..max.</summary>
        public int Level;
        /// <summary>Time until next activation.</summary>
        public float Timer;
        /// <summary>True once evolved.</summary>
        public bool Evolved;
        /// <summary>Orbit angle (blades), radians.</summary>
        public float Angle;
    }
}
