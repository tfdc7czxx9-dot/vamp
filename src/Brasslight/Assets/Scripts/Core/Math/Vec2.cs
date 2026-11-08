using System;

namespace Vamp.Core.Math
{
    /// <summary>
    /// Minimal engine-independent 2D vector on the gameplay plane (X = world X, Y = world Z).
    /// </summary>
    [Serializable]
    public struct Vec2 : IEquatable<Vec2>
    {
        /// <summary>X component.</summary>
        public float X;
        /// <summary>Y component (maps to world Z in the Unity layer).</summary>
        public float Y;

        /// <summary>Creates a vector.</summary>
        public Vec2(float x, float y) { X = x; Y = y; }

        /// <summary>The zero vector.</summary>
        public static Vec2 Zero => new Vec2(0f, 0f);

        /// <summary>Squared length.</summary>
        public float SqrMagnitude => X * X + Y * Y;

        /// <summary>Length.</summary>
        public float Magnitude => MathF.Sqrt(X * X + Y * Y);

        /// <summary>Returns a unit-length copy, or zero if the vector is (near) zero.</summary>
        public Vec2 Normalized
        {
            get
            {
                float m = Magnitude;
                return m > 1e-6f ? new Vec2(X / m, Y / m) : Zero;
            }
        }

        /// <summary>Returns a copy whose length is at most <paramref name="max"/>.</summary>
        public Vec2 ClampMagnitude(float max)
        {
            float sq = SqrMagnitude;
            if (sq <= max * max) return this;
            float m = MathF.Sqrt(sq);
            return new Vec2(X / m * max, Y / m * max);
        }

        /// <summary>Squared distance between two points.</summary>
        public static float SqrDistance(Vec2 a, Vec2 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>Distance between two points.</summary>
        public static float Distance(Vec2 a, Vec2 b) => MathF.Sqrt(SqrDistance(a, b));

        /// <summary>Dot product.</summary>
        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>Unit vector for an angle in radians.</summary>
        public static Vec2 FromAngle(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator -(Vec2 a) => new Vec2(-a.X, -a.Y);
        public static Vec2 operator *(Vec2 a, float s) => new Vec2(a.X * s, a.Y * s);
        public static Vec2 operator *(float s, Vec2 a) => new Vec2(a.X * s, a.Y * s);
        public static Vec2 operator /(Vec2 a, float s) => new Vec2(a.X / s, a.Y / s);

        /// <inheritdoc/>
        public bool Equals(Vec2 other) => X == other.X && Y == other.Y;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Vec2 v && Equals(v);
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y);
        /// <inheritdoc/>
        public override string ToString() => $"({X:0.###}, {Y:0.###})";
    }
}
