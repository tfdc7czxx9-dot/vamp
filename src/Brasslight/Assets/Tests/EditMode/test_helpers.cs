using Vamp.Core.Config;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    /// <summary>Factories and drivers shared by simulation tests.</summary>
    public static class TestHelpers
    {
        /// <summary>Default balance, freshly created per test (tests may mutate it).</summary>
        public static BalanceConfig Config() => BalanceConfig.CreateDefault();

        /// <summary>A run in world 0 with leader 0 and the given seed.</summary>
        public static GameSimulation NewRun(BalanceConfig config = null, uint seed = 42, RunModifiers mods = null, bool mobile = true)
        {
            config ??= Config();
            return new GameSimulation(config, new RunSetup { World = 0, Leader = 0, Seed = seed, Mobile = mobile, Modifiers = mods ?? new RunModifiers() });
        }

        /// <summary>
        /// Steps the simulation for <paramref name="seconds"/>, auto-resolving level-ups (first option)
        /// and build choices (cancel unless <paramref name="build"/> is true, then first affordable).
        /// </summary>
        public static void RunFor(GameSimulation sim, float seconds, Vec2 move = default, bool build = false)
        {
            const float dt = 1f / 60f;
            int steps = (int)System.MathF.Ceiling(seconds / dt);
            for (int i = 0; i < steps && !sim.IsOver; i++)
            {
                ResolveChoices(sim, build);
                sim.Step(dt, move);
            }
            ResolveChoices(sim, build);
        }

        /// <summary>Resolves pending choices deterministically.</summary>
        public static void ResolveChoices(GameSimulation sim, bool build)
        {
            int guard = 0;
            while (guard++ < 100)
            {
                if (sim.Phase == RunPhase.LevelUpChoice) { sim.ChooseUpgrade(0); continue; }
                if (sim.Phase == RunPhase.BuildChoice)
                {
                    int pick = -1;
                    if (build)
                        for (int i = 0; i < sim.BuildOffers.Count; i++)
                            if (sim.CanAfford(sim.BuildOffers[i])) { pick = i; break; }
                    sim.ChooseStructure(pick);
                    continue;
                }
                break;
            }
        }

        /// <summary>Removes every active enemy without drops (isolates a scenario from random spawns).</summary>
        public static void ClearEnemies(GameSimulation sim)
        {
            for (int i = 0; i < sim.Enemies.Length; i++)
                if (sim.Enemies[i].Active) sim.Enemies[i].Hp = 0.0001f;
            for (int i = 0; i < sim.Enemies.Length; i++)
                if (sim.Enemies[i].Active) sim.DamageEnemy(i, 1f);
        }

        /// <summary>Index of the first eligible elite type in world 0.</summary>
        public static int EliteDef(GameSimulation sim)
        {
            foreach (var id in sim.World.enemyIds)
            {
                int i = sim.Config.EnemyIndex(id);
                if (sim.Config.enemies[i].eliteEligible) return i;
            }
            return -1;
        }

        /// <summary>Spawns an elite at a position and kills it, leaving a remnant.</summary>
        public static void KillEliteAt(GameSimulation sim, Vec2 pos)
        {
            int idx = sim.SpawnEnemy(EliteDef(sim), pos, true, false);
            sim.DamageEnemy(idx, 1e9f);
        }
    }
}
