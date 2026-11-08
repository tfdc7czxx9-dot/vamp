using NUnit.Framework;
using Vamp.Core.Math;

namespace Vamp.Tests
{
    public class CoreMathTest
    {
        [Test]
        public void test_rng_same_seed_same_sequence()
        {
            var a = new Rng(123);
            var b = new Rng(123);
            for (int i = 0; i < 100; i++) Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void test_rng_range_stays_in_bounds()
        {
            var r = new Rng(7);
            for (int i = 0; i < 10000; i++)
            {
                int v = r.Range(3, 9);
                Assert.That(v, Is.InRange(3, 8));
                float f = r.NextFloat();
                Assert.That(f, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
        }

        [Test]
        public void test_rng_zero_seed_is_valid()
        {
            var r = new Rng(0);
            Assert.AreNotEqual(0u, r.NextUInt());
        }

        [Test]
        public void test_vec2_normalized_zero_returns_zero()
        {
            Assert.AreEqual(Vec2.Zero, Vec2.Zero.Normalized);
            Assert.AreEqual(1f, new Vec2(3f, 4f).Normalized.Magnitude, 1e-5f);
        }

        [Test]
        public void test_vec2_clamp_magnitude_limits_length()
        {
            Assert.AreEqual(1f, new Vec2(10f, 0f).ClampMagnitude(1f).Magnitude, 1e-5f);
            Assert.AreEqual(new Vec2(0.5f, 0f), new Vec2(0.5f, 0f).ClampMagnitude(1f));
        }
    }
}
