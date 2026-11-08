using NUnit.Framework;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    /// <summary>Evolutions, ranged enemies, boss patterns and crew/structure synergies.</summary>
    public class ContentFeaturesTest
    {
        private static void MaxGadgetAndPassive(GameSimulation sim, string passiveId)
        {
            var g = sim.Gadgets[0];
            g.Level = sim.Config.player.maxGadgetLevel;
            sim.Gadgets[0] = g;
            for (int i = 0; i < sim.Config.passives.Length; i++)
                if (sim.Config.passives[i].id == passiveId) sim.PassiveLevels[i] = 1;
        }

        [Test]
        public void test_max_gadget_with_partner_passive_is_offered_evolution()
        {
            var sim = TestHelpers.NewRun();
            var d = sim.Config.gadgets[sim.Gadgets[0].Def];
            MaxGadgetAndPassive(sim, d.evolveWithPassive);
            Assert.IsTrue(sim.CanEvolve(sim.Gadgets[0]));
            sim.AddXp(5);
            sim.Step(1f / 60f, Vec2.Zero);
            Assert.AreEqual(UpgradeOptionKind.Evolve, sim.LevelUpOffers[0].Kind);
        }

        [Test]
        public void test_without_partner_passive_no_evolution()
        {
            var sim = TestHelpers.NewRun();
            var g = sim.Gadgets[0];
            g.Level = sim.Config.player.maxGadgetLevel;
            sim.Gadgets[0] = g;
            Assert.IsFalse(sim.CanEvolve(sim.Gadgets[0]));
        }

        [Test]
        public void test_evolving_multiplies_damage_and_counts_stat()
        {
            var sim = TestHelpers.NewRun();
            var d = sim.Config.gadgets[sim.Gadgets[0].Def];
            MaxGadgetAndPassive(sim, d.evolveWithPassive);
            float before = sim.EffectiveDamage(sim.Gadgets[0]);
            sim.AddXp(5);
            sim.Step(1f / 60f, Vec2.Zero);
            sim.ChooseUpgrade(0);
            Assert.IsTrue(sim.Gadgets[0].Evolved);
            Assert.AreEqual(before * d.evolvedDamageMultiplier, sim.EffectiveDamage(sim.Gadgets[0]), 1e-3f);
            Assert.AreEqual(1, sim.Stats.Evolutions);
        }

        [Test]
        public void test_ranged_enemy_shot_damages_player()
        {
            var sim = TestHelpers.NewRun();
            int moth = sim.Config.EnemyIndex("glow_moth");
            int idx = sim.SpawnEnemy(moth, new Vec2(5f, 0f), false, false);
            sim.Enemies[idx].ShootTimer = 0f;
            sim.Enemies[idx].Speed = 0f;
            float hp = sim.Player.Hp;
            TestHelpers.RunFor(sim, 1.2f);
            Assert.Less(sim.Player.Hp, hp);
        }

        [Test]
        public void test_boss_fires_radial_volley_and_charges()
        {
            var sim = TestHelpers.NewRun();
            int boss = sim.Config.EnemyIndex(sim.World.bossId);
            int idx = sim.SpawnEnemy(boss, new Vec2(10f, 0f), false, true);
            sim.Enemies[idx].ShootTimer = 0f;
            sim.Enemies[idx].ChargeTimer = 0f;
            sim.Step(1f / 60f, Vec2.Zero);
            int shots = 0;
            foreach (var p in sim.Projectiles) if (p.Active && p.Kind == ProjectileKind.EnemyShot) shots++;
            Assert.AreEqual(sim.Config.enemies[boss].shotCount, shots);
            Assert.Greater(sim.Enemies[idx].Charging, 0f);
        }

        [Test]
        public void test_companion_near_lightning_mast_triggers_synergy()
        {
            var sim = TestHelpers.NewRun();
            sim.Player.Brass = 500;
            var plot = BuildLattice.PlotPosition(sim.Config.build, 1, 0, sim.Setup.Seed);
            sim.Player.Pos = plot;
            for (int i = 0; i < 200 && sim.Phase == RunPhase.Playing; i++) sim.Step(1f / 60f, Vec2.Zero);
            int mast = -1;
            for (int i = 0; i < sim.Config.structures.Length; i++) if (sim.Config.structures[i].Kind == Core.Config.StructureKind.LightningMast) mast = i;
            sim.BuildOffers[0] = mast;
            Assert.IsTrue(sim.ChooseStructure(0));

            TestHelpers.KillEliteAt(sim, plot + new Vec2(1f, 0f));
            sim.Player.Pos = plot + new Vec2(1f, 0f);
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            Assert.AreEqual(1, sim.CrewCount);
            sim.SpawnEnemy(sim.Config.EnemyIndex("primeval_giant"), plot + new Vec2(3f, 0f), false, false);
            TestHelpers.RunFor(sim, 2f);
            Assert.IsTrue(sim.Stats.SynergyTriggered);
        }

        [Test]
        public void test_new_leaders_apply_bonuses()
        {
            var c = TestHelpers.Config();
            int sefa = c.LeaderIndex("sefa_okoro");
            var mods = RunModifiers.Create(c, sefa, null);
            Assert.AreEqual(1, mods.CrewBonus);
            Assert.Greater(mods.Magnet, 0f);
            Assert.AreEqual(6, c.leaders.Length);
        }
    }
}
