namespace Vamp.Core.Simulation
{
    /// <summary>Everything needed to start a run.</summary>
    public class RunSetup
    {
        /// <summary>World index.</summary>
        public int World;
        /// <summary>Leader index.</summary>
        public int Leader;
        /// <summary>RNG seed (same seed + same inputs = same run).</summary>
        public uint Seed = 1;
        /// <summary>Use the mobile enemy cap.</summary>
        public bool Mobile;
        /// <summary>Permanent modifiers from research and leader.</summary>
        public RunModifiers Modifiers = new RunModifiers();
    }
}
