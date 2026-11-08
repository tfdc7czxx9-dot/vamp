namespace Vamp.Core.Simulation
{
    /// <summary>What a level-up option grants.</summary>
    public enum UpgradeOptionKind
    {
        /// <summary>A gadget the player does not own yet.</summary>
        NewGadget,
        /// <summary>+1 level for an owned gadget.</summary>
        GadgetLevel,
        /// <summary>+1 level for a passive.</summary>
        Passive,
        /// <summary>Evolve a max-level gadget whose partner passive is owned.</summary>
        Evolve,
        /// <summary>Fallback heal when everything is maxed.</summary>
        Heal,
    }
}
