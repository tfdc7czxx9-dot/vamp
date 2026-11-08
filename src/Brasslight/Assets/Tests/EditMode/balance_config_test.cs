using NUnit.Framework;
using Vamp.Core.Config;

namespace Vamp.Tests
{
    public class BalanceConfigTest
    {
        [Test]
        public void test_default_config_resolves_without_errors()
        {
            var c = TestHelpers.Config();
            CollectionAssert.IsEmpty(c.Resolve());
        }

        [Test]
        public void test_unknown_gadget_kind_reports_error()
        {
            var c = TestHelpers.Config();
            c.gadgets[0].kind = "Banana";
            Assert.That(c.Resolve(), Has.Some.Contains("unknown kind"));
        }

        [Test]
        public void test_world_with_unknown_enemy_reports_error()
        {
            var c = TestHelpers.Config();
            c.worlds[0].enemyIds = new[] { "nope" };
            Assert.That(c.Resolve(), Has.Some.Contains("unknown enemy"));
        }

        [Test]
        public void test_every_world_has_a_recruitable_elite()
        {
            var c = TestHelpers.Config();
            foreach (var w in c.worlds)
            {
                bool any = false;
                foreach (var id in w.enemyIds) any |= c.enemies[c.EnemyIndex(id)].eliteEligible;
                Assert.IsTrue(any, w.id);
            }
        }
    }
}
