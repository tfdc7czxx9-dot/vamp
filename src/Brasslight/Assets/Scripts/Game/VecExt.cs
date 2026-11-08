using UnityEngine;
using Vamp.Core.Math;

namespace Vamp.Game
{
    /// <summary>Conversions between the simulation plane (X, Y) and Unity world space (X, Z).</summary>
    public static class VecExt
    {
        /// <summary>Simulation point to world position at height <paramref name="y"/>.</summary>
        public static Vector3 ToWorld(this Vec2 v, float y = 0f) => new Vector3(v.X, y, v.Y);

        /// <summary>Unity 2D vector to simulation vector.</summary>
        public static Vec2 ToSim(this Vector2 v) => new Vec2(v.x, v.y);

        /// <summary>Parses "#RRGGBB" (falls back to magenta so bad data is visible).</summary>
        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        }
    }
}
