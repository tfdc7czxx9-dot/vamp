namespace Vamp.Core.Config
{
    /// <summary>Stat affected by a passive upgrade. Values are fractions per level unless noted.</summary>
    public enum PassiveKind
    {
        /// <summary>+damage multiplier.</summary>
        Might,
        /// <summary>-cooldown multiplier.</summary>
        Haste,
        /// <summary>+move speed multiplier.</summary>
        Speed,
        /// <summary>+magnet radius multiplier.</summary>
        Magnet,
        /// <summary>+flat max HP.</summary>
        Plating,
        /// <summary>+area multiplier.</summary>
        Area,
        /// <summary>-build time multiplier.</summary>
        Engineering,
        /// <summary>-recruit time multiplier.</summary>
        Charisma,
    }
}
