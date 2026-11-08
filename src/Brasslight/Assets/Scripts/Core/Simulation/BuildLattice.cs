using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>
    /// Deterministic, infinite lattice of build plots: one plot per cell, except the origin cell.
    /// </summary>
    public static class BuildLattice
    {
        /// <summary>Packs a cell coordinate into a dictionary key.</summary>
        public static long Key(int cx, int cy) => ((long)cx << 32) ^ (uint)cy;

        /// <summary>Cell coordinate containing a world position.</summary>
        public static void CellOf(BuildConfig cfg, Vec2 pos, out int cx, out int cy)
        {
            cx = (int)System.MathF.Floor(pos.X / cfg.latticeCellSize);
            cy = (int)System.MathF.Floor(pos.Y / cfg.latticeCellSize);
        }

        /// <summary>True if the cell holds a plot.</summary>
        public static bool HasPlot(int cx, int cy) => !(cx == 0 && cy == 0);

        /// <summary>World position of the plot in a cell.</summary>
        public static Vec2 PlotPosition(BuildConfig cfg, int cx, int cy, uint seed)
        {
            uint h = Rng.Hash(cx, cy, seed);
            float u = (h & 0xFFFF) / 65535f;
            float v = (h >> 16) / 65535f;
            float j = cfg.plotJitter;
            float x = (cx + 0.5f + (u - 0.5f) * 2f * j) * cfg.latticeCellSize;
            float y = (cy + 0.5f + (v - 0.5f) * 2f * j) * cfg.latticeCellSize;
            return new Vec2(x, y);
        }
    }
}
