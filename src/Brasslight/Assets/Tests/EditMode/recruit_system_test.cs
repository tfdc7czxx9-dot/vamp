using NUnit.Framework;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    public class RecruitSystemTest
    {
        private static int ActiveRemnants(GameSimulation sim)
        {
            int n = 0;
            foreach (var r in sim.Remnants) if (r.Active) n++;
            return n;
        }

        [Test]
        public void test_elite_death_leaves_remnant()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.KillEliteAt(sim, new Vec2(8f, 0f));
            Assert.AreEqual(1, ActiveRemnants(sim));
        }

        [Test]
        public void test_standing_in_ring_for_recruit_time_creates_companion()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.KillEliteAt(sim, new Vec2(1f, 0f));
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            Assert.AreEqual(1, sim.CrewCount);
            Assert.AreEqual(1, sim.Stats.Recruited);
        }

        [Test]
        public void test_leaving_ring_pauses_progress()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.KillEliteAt(sim, new Vec2(1f, 0f));
            TestHelpers.RunFor(sim, sim.RecruitSeconds * 0.5f);
            sim.Player.Pos = new Vec2(10f, 10f);
            TestHelpers.RunFor(sim, 1f);
            Assert.AreEqual(0, sim.CrewCount);
            float progress = 0f;
            foreach (var r in sim.Remnants) if (r.Active) progress = r.Progress;
            Assert.That(progress, Is.InRange(0.4f, 0.6f));
        }

        [Test]
        public void test_unclaimed_remnant_expires()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.KillEliteAt(sim, new Vec2(8f, 0f));
            TestHelpers.RunFor(sim, sim.Config.recruit.remnantLifetime + 0.2f);
            Assert.AreEqual(0, ActiveRemnants(sim));
            Assert.AreEqual(0, sim.CrewCount);
        }

        [Test]
        public void test_full_crew_scraps_remnant_for_brass()
        {
            var c = TestHelpers.Config();
            c.recruit.baseCrewCapacity = 0;
            var sim = TestHelpers.NewRun(c);
            TestHelpers.KillEliteAt(sim, new Vec2(1f, 0f));
            int before = sim.Player.Brass;
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            Assert.AreEqual(0, sim.CrewCount);
            Assert.AreEqual(1, sim.Stats.Scrapped);
            Assert.GreaterOrEqual(sim.Player.Brass - before, c.recruit.scrapBrass);
        }

        [Test]
        public void test_crew_bonus_raises_capacity_up_to_max()
        {
            var sim = TestHelpers.NewRun(mods: new RunModifiers { CrewBonus = 10 });
            Assert.AreEqual(sim.Config.recruit.maxCrewCapacity, sim.CrewCapacity);
        }

        [Test]
        public void test_companion_hp_is_fraction_of_elite_hp()
        {
            var sim = TestHelpers.NewRun();
            int def = TestHelpers.EliteDef(sim);
            int idx = sim.SpawnEnemy(def, new Vec2(1f, 0f), true, false);
            float eliteHp = sim.Enemies[idx].MaxHp;
            sim.DamageEnemy(idx, 1e9f);
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            foreach (var comp in sim.Companions)
                if (comp.Active)
                    Assert.AreEqual(System.MathF.Max(sim.Config.recruit.companionMinHp, eliteHp * sim.Config.recruit.companionHpFraction), comp.MaxHp, 1e-3f);
        }

        [Test]
        public void test_companions_attack_enemies()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.KillEliteAt(sim, new Vec2(1f, 0f));
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            Assert.AreEqual(1, sim.CrewCount);
            double before = sim.Stats.DamageDealt;
            int target = sim.SpawnEnemy(sim.Config.EnemyIndex("primeval_giant"), sim.Player.Pos + new Vec2(3f, 0f), false, false);
            Assert.GreaterOrEqual(target, 0);
            TestHelpers.RunFor(sim, 2f);
            Assert.Greater(sim.Stats.DamageDealt, before);
        }
    }
}
