namespace Vamp.Core.Simulation
{
    /// <summary>Top-level state of a run.</summary>
    public enum RunPhase
    {
        /// <summary>Simulation advances.</summary>
        Playing,
        /// <summary>Paused, waiting for <see cref="GameSimulation.ChooseUpgrade"/>.</summary>
        LevelUpChoice,
        /// <summary>Paused, waiting for <see cref="GameSimulation.ChooseStructure"/>.</summary>
        BuildChoice,
        /// <summary>The boss was defeated.</summary>
        Victory,
        /// <summary>The leader fell.</summary>
        Defeat,
    }
}
