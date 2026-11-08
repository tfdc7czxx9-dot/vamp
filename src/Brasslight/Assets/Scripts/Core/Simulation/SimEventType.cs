namespace Vamp.Core.Simulation
{
    /// <summary>Kinds of presentation events emitted by the simulation each step.</summary>
    public enum SimEventType
    {
        EnemyHit, EnemyKilled, EliteSpawned, BossSpawned, PlayerHit, PlayerRevived, LevelUp,
        Recruited, Scrapped, CompanionAttack, CompanionLost, StructureBuilt, StructureUpgraded,
        StructureDestroyed, NotEnoughBrass, Lightning, Explosion, Flame, Pulse, PickupCollected,
        GadgetFired, Evolved, Synergy, EnemyShot, BossCharge,
    }
}
