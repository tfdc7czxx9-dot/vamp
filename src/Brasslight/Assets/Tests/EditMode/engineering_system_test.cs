using NUnit.Framework;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    public class EngineeringSystemTest
    {
        private static Vec2 PlotPos(GameSimulation sim, int cx = 1, int cy = 0) =>
            BuildLattice.PlotPosition(sim.Config.build, cx, cy, sim.Setup.Seed);

        private static int ActiveStructures(GameSimulation sim)
        {
            int n = 0;
            foreach (var s in sim.Structures) if (s.Active) n++;
            return n;
        }

        [Test]
        public void test_origin_cell_has_no_plot()
        {
            Assert.IsFalse(BuildLattice.HasPlot(0, 0));
            Assert.IsTrue(BuildLattice.HasPlot(1, 0));
        }

        [Test]
        public void test_plot_position_is_deterministic_and_inside_cell()
        {
            var sim = TestHelpers.NewRun();
            var b = sim.Config.build;
            for (int cx = -3; cx <= 3; cx++)
            for (int cy = -3; cy <= 3; cy++)
            {
                var p = BuildLattice.PlotPosition(b, cx, cy, 5);
                Assert.AreEqual(p, BuildLattice.PlotPosition(b, cx, cy, 5));
                BuildLattice.CellOf(b, p, out int x, out int y);
                Assert.AreEqual(cx, x);
                Assert.AreEqual(cy, y);
            }
        }

        [Test]
        public void test_standing_on_empty_plot_opens_build_choice()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Pos = PlotPos(sim);
            TestHelpers.ClearEnemies(sim);
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            Assert.AreEqual(RunPhase.BuildChoice, sim.Phase);
            Assert.AreEqual(3, sim.BuildOffers.Count);
        }

        [Test]
        public void test_build_choice_places_structure_and_costs_brass()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Brass = 100;
            sim.Player.Pos = PlotPos(sim);
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            int def = sim.BuildOffers[0];
            Assert.IsTrue(sim.ChooseStructure(0));
            Assert.AreEqual(1, ActiveStructures(sim));
            Assert.AreEqual(100 - sim.Config.structures[def].cost, sim.Player.Brass);
        }

        [Test]
        public void test_unaffordable_choice_is_rejected_and_cancel_resumes()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Brass = 0;
            sim.Player.Pos = PlotPos(sim);
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            Assert.IsFalse(sim.ChooseStructure(0));
            Assert.AreEqual(RunPhase.BuildChoice, sim.Phase);
            Assert.IsTrue(sim.ChooseStructure(-1));
            Assert.AreEqual(RunPhase.Playing, sim.Phase);
        }

        [Test]
        public void test_plot_does_not_retrigger_until_player_leaves()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Pos = PlotPos(sim);
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            sim.ChooseStructure(-1);
            TestHelpers.ClearEnemies(sim);
            for (int i = 0; i < 200; i++) sim.Step(1f / 60f, Vec2.Zero);
            Assert.AreNotEqual(RunPhase.BuildChoice, sim.Phase);
        }

        [Test]
        public void test_standing_on_own_structure_upgrades_it()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Brass = 500;
            var plot = PlotPos(sim);
            sim.Player.Pos = plot;
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            sim.ChooseStructure(0);
            int brassAfterBuild = sim.Player.Brass;

            sim.Player.Pos = plot + new Vec2(5f, 5f);
            TestHelpers.RunFor(sim, 0.1f);
            sim.Player.Pos = plot;
            TestHelpers.RunFor(sim, sim.BuildSeconds + 0.1f);

            Structure built = default;
            foreach (var s in sim.Structures) if (s.Active) built = s;
            Assert.AreEqual(2, built.Level);
            Assert.LessOrEqual(sim.Player.Brass, brassAfterBuild - sim.Config.structures[built.Def].cost + 50);
        }

        [Test]
        public void test_level_multiplier_formula()
        {
            var sim = TestHelpers.NewRun();
            Assert.AreEqual(1f, sim.LevelMultiplier(1), 1e-5f);
            Assert.AreEqual(1f + 4f * sim.Config.build.levelScaling, sim.LevelMultiplier(5), 1e-5f);
        }

        [Test]
        public void test_destroyed_structure_frees_plot()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Brass = 500;
            sim.Player.Pos = PlotPos(sim);
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            int pick = -1;
            for (int i = 0; i < sim.BuildOffers.Count; i++) if (sim.Config.structures[sim.BuildOffers[i]].hp > 0f) pick = i;
            if (pick < 0) Assert.Ignore("No destructible structure offered for this seed.");
            sim.ChooseStructure(pick);
            for (int i = 0; i < sim.Structures.Length; i++) if (sim.Structures[i].Active) sim.Structures[i].Hp = 0f;
            sim.Player.Pos = new Vec2(-50f, -50f);
            TestHelpers.RunFor(sim, 0.05f);
            Assert.AreEqual(0, ActiveStructures(sim));
        }
    }
}
