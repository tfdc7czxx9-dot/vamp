namespace Vamp.Core.Simulation
{
    /// <summary>One card offered on level-up.</summary>
    public struct UpgradeOption
    {
        /// <summary>Kind.</summary>
        public UpgradeOptionKind Kind;
        /// <summary>Gadget or passive def index.</summary>
        public int Index;
        /// <summary>Level the item will have after choosing.</summary>
        public int NextLevel;
    }
}
