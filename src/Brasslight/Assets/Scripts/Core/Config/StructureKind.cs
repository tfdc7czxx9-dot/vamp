namespace Vamp.Core.Config
{
    /// <summary>Type of structure that can be built on a build plot.</summary>
    public enum StructureKind
    {
        /// <summary>Blocks enemies and absorbs damage.</summary>
        Barricade,
        /// <summary>Shoots bolts at the nearest enemy.</summary>
        SteamTurret,
        /// <summary>Explodes when enemies step on it, then re-arms.</summary>
        MineField,
        /// <summary>Periodic chain lightning.</summary>
        LightningMast,
        /// <summary>Periodically pulls enemy attention away from the player.</summary>
        LureBeacon,
        /// <summary>Heals companions and the player nearby.</summary>
        FieldWorkshop,
    }
}
