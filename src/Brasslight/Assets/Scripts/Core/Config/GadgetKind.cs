namespace Vamp.Core.Config
{
    /// <summary>Behaviour family of an auto-firing gadget (weapon).</summary>
    public enum GadgetKind
    {
        /// <summary>Fires projectiles at the nearest enemy.</summary>
        SteamCannon,
        /// <summary>Chain lightning jumping between nearby enemies.</summary>
        ArcCoil,
        /// <summary>Blades orbiting the player.</summary>
        GyroBlades,
        /// <summary>Delayed area shells landing on enemies.</summary>
        Mortar,
        /// <summary>Short-range cone in the facing direction.</summary>
        FlameBellows,
        /// <summary>Damaging, slowing aura around the player.</summary>
        AetherPulse,
    }
}
