using NUnit.Framework;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    /// <summary>Plays complete simulated runs with a simple bot to catch crashes and runaway state.</summary>
    public class FullRunSmokeTest
    {
        private static void PlayBot(GameSimulation sim, float seconds)
        {
            const float dt = 1f / 60f;
            int steps = (int)(seconds / dt);
            var bot = new AutoPilot();
            for (int i = 0; i < steps && !sim.IsOver; i++)
            {
                for (int g = 0; g < 10 && (sim.Phase == RunPhase.LevelUpChoice || sim.Phase == RunPhase.BuildChoice); g++) bot.ResolveChoices(sim);
                sim.Step(dt, bot.Steer(sim));
                Assert.LessOrEqual(sim.EnemyCount, sim.Enemies.Length);
            }
        }

        private static void Report(GameSimulation sim)
        {
            TestContext.WriteLine($"t={sim.Time:0}s phase={sim.Phase} lvl={sim.Player.Level} kills={sim.Stats.Kills} elites={sim.Stats.EliteKills} " +
                $"recruited={sim.Stats.Recruited} scrapped={sim.Stats.Scrapped} built={sim.Stats.StructuresBuilt} upgraded={sim.Stats.StructuresUpgraded} " +
                $"brass={sim.Player.Brass}/{sim.Stats.BrassCollected} enemies={sim.EnemyCount} hp={sim.Player.Hp:0}/{sim.Player.MaxHp:0} gadgets={sim.Gadgets.Count} rp={sim.ResearchPoints}");
        }

        [Test]
        public void test_full_run_reaches_boss_or_ends_without_errors()
        {
            var c = TestHelpers.Config();
            c.player.maxHp = 100000f; // keep the bot alive to exercise the whole timeline
            var sim = TestHelpers.NewRun(c, 2024);
            PlayBot(sim, c.run.bossTimeSeconds + 5f);
            Report(sim);
            Assert.That(sim.Time, Is.GreaterThanOrEqualTo(c.run.bossTimeSeconds).Or.Property("IsOver").True);
            Assert.Greater(sim.Stats.Kills, 500);
            Assert.Greater(sim.Player.Level, 10);
            Assert.Greater(sim.Stats.EliteKills, 5);
            Assert.Greater(sim.Stats.Recruited, 0, "bot should exercise recruiting");
            Assert.Greater(sim.Stats.StructuresBuilt, 0, "bot should exercise building");
        }

        [Test]
        public void test_default_bot_run_ends_or_survives_cleanly()
        {
            var sim = TestHelpers.NewRun(seed: 7);
            PlayBot(sim, 300f);
            Report(sim);
            Assert.Greater(sim.Stats.Kills, 0);
            Assert.AreNotEqual(RunPhase.Defeat, sim.Phase, "a competent player should survive the first five minutes");
        }

        [TestCase(1u)]
        [TestCase(2u)]
        [TestCase(3u)]
        public void test_balance_autopilot_full_run_report(uint seed)
        {
            // Difficulty probe: the autopilot is weaker than a human; log how far it gets.
            var sim = TestHelpers.NewRun(seed: seed);
            PlayBot(sim, sim.Config.run.bossTimeSeconds + 120f);
            Report(sim);
            Assert.Greater(sim.Time, 180f, "difficulty spike: bot died before minute 3");
        }
    }
}
