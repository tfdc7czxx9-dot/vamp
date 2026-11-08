using System;

namespace Vamp.Core.Math
{
    /// <summary>
    /// Deterministic, allocation-free xorshift32 random number generator.
    /// The same seed always yields the same sequence on every platform.
    /// </summary>
    public sealed class Rng
    {
        private uint _state;

        /// <summary>Creates a generator. A seed of 0 is remapped to a fixed non-zero value.</summary>
        public Rng(uint seed) { _state = seed == 0 ? 0x9E3779B9u : seed; }

        /// <summary>Next raw 32-bit value.</summary>
        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>Uniform float in [0, 1).</summary>
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);

        /// <summary>Uniform float in [min, max).</summary>
        public float Range(float min, float max) => min + (max - min) * NextFloat();

        /// <summary>Uniform int in [min, maxExclusive). Returns min if the range is empty.</summary>
        public int Range(int min, int maxExclusive)
        {
            if (maxExclusive <= min) return min;
            return min + (int)(NextUInt() % (uint)(maxExclusive - min));
        }

        /// <summary>True with probability <paramref name="p"/>.</summary>
        public bool Chance(float p) => NextFloat() < p;

        /// <summary>Random point on a ring around the origin.</summary>
        public Vec2 OnRing(float minRadius, float maxRadius)
        {
            float a = NextFloat() * MathF.PI * 2f;
            return Vec2.FromAngle(a) * Range(minRadius, maxRadius);
        }

        /// <summary>Stateless hash of two ints, used for deterministic world features (e.g. build plots).</summary>
        public static uint Hash(int a, int b, uint seed)
        {
            unchecked
            {
                uint h = seed ^ 0x85EBCA6Bu;
                h ^= (uint)a * 0xCC9E2D51u; h = (h << 15) | (h >> 17); h *= 0x1B873593u;
                h ^= (uint)b * 0xE6546B64u; h = (h << 13) | (h >> 19); h *= 0x85EBCA6Bu;
                h ^= h >> 16; h *= 0x7FEB352Du; h ^= h >> 15; h *= 0x846CA68Bu; h ^= h >> 16;
                return h;
            }
        }
    }
}
