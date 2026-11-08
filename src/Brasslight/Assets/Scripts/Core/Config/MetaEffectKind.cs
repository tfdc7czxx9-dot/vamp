namespace Vamp.Core.Config
{
    /// <summary>Permanent (meta-progression) effect of a research upgrade.</summary>
    public enum MetaEffectKind
    {
        /// <summary>+flat max HP per level.</summary>
        MaxHp,
        /// <summary>+damage fraction per level.</summary>
        Might,
        /// <summary>+move speed fraction per level.</summary>
        Speed,
        /// <summary>+crew capacity per level.</summary>
        CrewCapacity,
        /// <summary>+brass at run start per level.</summary>
        StartingBrass,
        /// <summary>+magnet radius fraction per level.</summary>
        Magnet,
        /// <summary>Number of revives per run.</summary>
        Revival,
    }
}
