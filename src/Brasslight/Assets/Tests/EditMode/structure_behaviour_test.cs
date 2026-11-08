using NUnit.Framework;
using Vamp.Core.Config;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    /// <summary>Per-structure behaviour and timing rules from the Engineering and Horde GDDs.</summary>
    public class StructureBehaviourTest
    {
        private static int Def(GameSimulation sim, StructureKind kind)
        {
            for (int i = 0; i < sim.Config.structures.Length; i++) if (sim.Config.structures[i].Kind == kind) return i;
            return -1;
        }

        /// <summary>Builds a structure of the given kind on plot (1,0) and moves the player far away.</summary>
        private static Vec2 Build(GameSimulation sim, StructureKind kind)
        {
            sim.Player.Brass = 1000;
            var plot = BuildLattice.PlotPosition(sim.Config.build, 1, 0, sim.Setup.Seed);
            sim.Player.Pos = plot;
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            sim.BuildOffers[0] = Def(sim, kind);
            Assert.IsTrue(sim.ChooseStructure(0));
            sim.Player.Pos = plot + new Vec2(0f, -14f); // outside structure ranges, inside the despawn radius
            TestHelpers.ClearEnemies(sim);
            return plot;
        }

        [Test]
        public void test_mine_field_explodes_then_rearms_after_cooldown()
        {
            var sim = TestHelpers.NewRun();
            var plot = Build(sim, StructureKind.MineField);
            int e = sim.SpawnEnemy(0, plot, false, false);
            sim.Enemies[e].Speed = 0f;
            sim.Step(1f / 60f, Vec2.Zero);
            Structure s = default;
            foreach (var st in sim.Structures) if (st.Active) s = st;
            Assert.IsFalse(s.Armed, "mine should have fired");
            TestHelpers.RunFor(sim, sim.Config.structures[s.Def].cooldown + 0.2f);
            foreach (var st in sim.Structures) if (st.Active) s = st;
            Assert.IsTrue(s.Armed, "mine should re-arm after its cooldown");
        }

        [Test]
        public void test_lightning_mast_hits_at_most_chain_count_targets()
        {
            var sim = TestHelpers.NewRun();
            var plot = Build(sim, StructureKind.LightningMast);
            int giant = sim.Config.EnemyIndex("primeval_giant");
            for (int k = 0; k < 8; k++)
            {
                int i = sim.SpawnEnemy(giant, plot + new Vec2(1.5f + k * 0.5f, 0f), false, false);
                sim.Enemies[i].Speed = 0f;
            }
            sim.Step(1f / 60f, Vec2.Zero);
            int bolts = 0;
            foreach (var ev in sim.Events) if (ev.Type == SimEventType.Lightning) bolts++;
            Assert.AreEqual(sim.Config.structures[Def(sim, StructureKind.LightningMast)].chainCount, bolts);
        }

        [Test]
        public void test_lure_beacon_is_active_only_during_its_duty_cycle()
        {
            var sim = TestHelpers.NewRun();
            Build(sim, StructureKind.LureBeacon);
            var d = sim.Config.structures[Def(sim, StructureKind.LureBeacon)];
            TestHelpers.RunFor(sim, d.activeSeconds + 0.5f);
            Structure s = default;
            foreach (var st in sim.Structures) if (st.Active) s = st;
            Assert.GreaterOrEqual(s.Cycle, d.activeSeconds, "beacon should be resting after its active window");
            TestHelpers.RunFor(sim, d.intervalSeconds - d.activeSeconds);
            foreach (var st in sim.Structures) if (st.Active) s = st;
            Assert.Less(s.Cycle, d.activeSeconds, "beacon should be active again in the next cycle");
        }

        [Test]
        public void test_field_workshop_heals_player_in_range()
        {
            var sim = TestHelpers.NewRun();
            var plot = Build(sim, StructureKind.FieldWorkshop);
            sim.Player.Pos = plot + new Vec2(2f, 0f);
            sim.Player.Hp = 50f;
            TestHelpers.ClearEnemies(sim);
            for (int i = 0; i < 60; i++) { sim.Step(1f / 60f, Vec2.Zero); TestHelpers.ResolveChoices(sim, false); }
            Assert.Greater(sim.Player.Hp, 50.5f);
        }

        [Test]
        public void test_first_elite_spawns_at_configured_time()
        {
            var sim = TestHelpers.NewRun();
            var c = sim.Config;
            bool seen = false;
            float at = -1f;
            const float dt = 1f / 60f;
            while (sim.Time < c.run.eliteFirstAt + 1f && !sim.IsOver)
            {
                TestHelpers.ResolveChoices(sim, false);
                sim.Player.Hp = sim.Player.MaxHp;
                sim.Step(dt, Vec2.Zero);
                foreach (var e in sim.Events)
                    if (e.Type == SimEventType.EliteSpawned && !seen) { seen = true; at = sim.Time; }
            }
            Assert.IsTrue(seen);
            Assert.AreEqual(c.run.eliteFirstAt, at, 0.05f);
        }

        [Test]
        public void test_research_points_formula()
        {
            var sim = TestHelpers.NewRun();
            sim.Stats.Kills = 410;
            // RP = floor(410/20) + 2*0 + 0 = 20 at t=0
            Assert.AreEqual(20, sim.ResearchPoints);
            sim.Stats.BossDefeated = true;
            Assert.AreEqual(20 + sim.Config.research.victoryBonus, sim.ResearchPoints);
        }

        [Test]
        public void test_spawned_enemies_appear_on_spawn_ring()
        {
            var sim = TestHelpers.NewRun();
            TestHelpers.RunFor(sim, 2f);
            var r = sim.Config.run;
            foreach (var e in sim.Enemies)
            {
                if (!e.Active) continue;
                float d = Vec2.Distance(e.Pos, sim.Player.Pos);
                Assert.That(d, Is.InRange(r.spawnRingMin - 3f * 2f, r.spawnRingMax + 0.5f));
            }
        }
    }
}
