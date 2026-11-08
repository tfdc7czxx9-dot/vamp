using NUnit.Framework;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    public class HordeProgressionTest
    {
        [TestCase(1, 5)]
        [TestCase(2, 12)]
        [TestCase(3, 19)]
        [TestCase(4, 28)]
        [TestCase(5, 39)]
        [TestCase(10, 108)]
        public void test_xp_curve_matches_gdd(int level, int expected)
        {
            Assert.AreEqual(expected, TestHelpers.NewRun().XpForLevel(level));
        }

        [TestCase(0f, 1.0f)]
        [TestCase(15f, 12.25f)]
        public void test_spawn_rate_matches_gdd(float minute, float expected)
        {
            Assert.AreEqual(expected, TestHelpers.NewRun().SpawnRate(minute), 1e-4f);
        }

        [Test]
        public void test_enemies_spawn_over_time()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.RunFor(sim, 10f);
            Assert.Greater(sim.EnemyCount, 0);
        }

        [Test]
        public void test_add_xp_queues_level_up_and_opens_choice()
        {
            var sim = TestHelpers.NewRun();
            sim.AddXp(5);
            Assert.AreEqual(2, sim.Player.Level);
            sim.Step(1f / 60f, Vec2.Zero);
            Assert.AreEqual(RunPhase.LevelUpChoice, sim.Phase);
            Assert.AreEqual(3, sim.LevelUpOffers.Count);
        }

        [Test]
        public void test_level_up_offers_are_distinct()
        {
            var sim = TestHelpers.NewRun();
            sim.AddXp(5);
            sim.Step(1f / 60f, Vec2.Zero);
            var o = sim.LevelUpOffers;
            for (int i = 0; i < o.Count; i++)
            for (int j = i + 1; j < o.Count; j++)
                Assert.IsFalse(o[i].Kind == o[j].Kind && o[i].Index == o[j].Index);
        }

        [Test]
        public void test_choose_upgrade_resumes_play()
        {
            var sim = TestHelpers.NewRun();
            sim.AddXp(5);
            sim.Step(1f / 60f, Vec2.Zero);
            Assert.IsTrue(sim.ChooseUpgrade(0));
            Assert.AreEqual(RunPhase.Playing, sim.Phase);
        }

        [Test]
        public void test_gadget_damage_scales_per_level()
        {
            var sim = TestHelpers.NewRun();
            var d = sim.Config.gadgets[0];
            Assert.AreEqual(d.damage, sim.GadgetDamage(d, 1), 1e-4f);
            Assert.AreEqual(d.damage * (1f + 2f * d.damagePerLevel), sim.GadgetDamage(d, 3), 1e-4f);
        }

        [Test]
        public void test_killing_boss_wins_run()
        {
            var sim = TestHelpers.NewRun();
            int boss = sim.Config.EnemyIndex(sim.World.bossId);
            int idx = sim.SpawnEnemy(boss, new Vec2(10f, 0f), false, true);
            sim.DamageEnemy(idx, 1e9f);
            Assert.AreEqual(RunPhase.Victory, sim.Phase);
            Assert.IsTrue(sim.Stats.BossDefeated);
        }

        [Test]
        public void test_contact_damage_can_defeat_player()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Hp = 1f;
            sim.SpawnEnemy(0, sim.Player.Pos, false, false);
            TestHelpers.RunFor(sim, 0.2f);
            Assert.AreEqual(RunPhase.Defeat, sim.Phase);
        }

        [Test]
        public void test_revive_restores_half_hp_once()
        {
            var sim = TestHelpers.NewRun(mods: new RunModifiers { Revives = 1 });
            sim.Player.Hp = 1f;
            sim.SpawnEnemy(0, sim.Player.Pos, false, false);
            TestHelpers.RunFor(sim, 0.2f);
            Assert.AreNotEqual(RunPhase.Defeat, sim.Phase);
            Assert.AreEqual(0, sim.Player.RevivesLeft);
            Assert.Greater(sim.Player.Hp, sim.Player.MaxHp * 0.4f);
        }

        [Test]
        public void test_same_seed_and_input_is_deterministic()
        {
            var a = TestHelpers.NewRun(seed: 99);
            var b = TestHelpers.NewRun(seed: 99);
            var move = new Vec2(0.6f, 0.8f);
            TestHelpers.RunFor(a, 45f, move, true);
            TestHelpers.RunFor(b, 45f, move, true);
            Assert.AreEqual(a.Stats.Kills, b.Stats.Kills);
            Assert.AreEqual(a.Player.Pos, b.Player.Pos);
            Assert.AreEqual(a.Player.Level, b.Player.Level);
        }

        [Test]
        public void test_normal_enemies_never_exceed_cap()
        {
            var c = TestHelpers.Config();
            c.run.enemyCapMobile = 50;
            c.run.spawnBase = 100f;
            var sim = TestHelpers.NewRun(c);
            TestHelpers.RunFor(sim, 3f);
            int normals = 0;
            foreach (var e in sim.Enemies) if (e.Active && !e.Elite && !e.Boss) normals++;
            Assert.LessOrEqual(normals, 50);
        }
    }
}
