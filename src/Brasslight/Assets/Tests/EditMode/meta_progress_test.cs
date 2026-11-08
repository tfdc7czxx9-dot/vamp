using NUnit.Framework;
using Vamp.Core.Config;
using Vamp.Core.Meta;
using Vamp.Core.Simulation;

namespace Vamp.Tests
{
    public class MetaProgressTest
    {
        [Test]
        public void test_next_cost_grows_geometrically()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData { researchPoints = 1000 });
            var hull = c.metaUpgrades[0];
            Assert.AreEqual(hull.baseCost, meta.NextCost(hull));
            Assert.IsTrue(meta.Buy(hull));
            Assert.AreEqual((int)System.MathF.Round(hull.baseCost * hull.costGrowth), meta.NextCost(hull));
        }

        [Test]
        public void test_buy_fails_without_points()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData());
            Assert.IsFalse(meta.Buy(c.metaUpgrades[0]));
            Assert.AreEqual(0, meta.Level(c.metaUpgrades[0].id));
        }

        [Test]
        public void test_maxed_upgrade_has_no_cost()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData { researchPoints = 100000 });
            var u = c.metaUpgrades[0];
            for (int i = 0; i < u.maxLevel; i++) Assert.IsTrue(meta.Buy(u));
            Assert.AreEqual(-1, meta.NextCost(u));
            Assert.IsFalse(meta.Buy(u));
        }

        [Test]
        public void test_modifiers_reflect_purchases_and_leader()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData { researchPoints = 100000 });
            foreach (var u in c.metaUpgrades) if (u.Effect == MetaEffectKind.CrewCapacity) meta.Buy(u);
            var mods = meta.CreateModifiers(0);
            Assert.AreEqual(1, mods.CrewBonus);
            Assert.AreEqual(c.leaders[0].buildTimeBonus, mods.BuildTime, 1e-5f);
        }

        [Test]
        public void test_victory_unlocks_next_world_and_awards_points()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData());
            Assert.IsFalse(meta.IsWorldUnlocked(c.worlds[1]));
            var sim = TestHelpers.NewRun(c);
            int boss = c.EnemyIndex(sim.World.bossId);
            sim.DamageEnemy(sim.SpawnEnemy(boss, new Core.Math.Vec2(5f, 0f), false, true), 1e9f);
            int pts = meta.RecordRun(sim);
            Assert.GreaterOrEqual(pts, c.research.victoryBonus);
            Assert.IsTrue(meta.IsWorldUnlocked(c.worlds[1]));
            Assert.AreEqual(1, meta.Save.totalVictories);
        }

        [Test]
        public void test_migrate_repairs_null_collections()
        {
            var s = new SaveData { upgrades = null, wonWorlds = null, settings = null, researchPoints = -5 };
            MetaProgress.Migrate(s);
            Assert.IsNotNull(s.upgrades);
            Assert.IsNotNull(s.wonWorlds);
            Assert.IsNotNull(s.settings);
            Assert.AreEqual(0, s.researchPoints);
        }

        [Test]
        public void test_leader_unlock_costs_points()
        {
            var c = TestHelpers.Config();
            var meta = new MetaProgress(c, new SaveData { researchPoints = c.leaders[1].unlockCost });
            Assert.IsFalse(meta.IsLeaderUnlocked(c.leaders[1]));
            Assert.IsTrue(meta.UnlockLeader(c.leaders[1]));
            Assert.IsTrue(meta.IsLeaderUnlocked(c.leaders[1]));
            Assert.AreEqual(0, meta.Save.researchPoints);
        }
    }
}
