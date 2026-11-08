using System;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>
    /// Allocation-free spatial hash for enemy queries. Rebuilt every step.
    /// Buckets are hashed cells; collisions are filtered by the caller's distance check.
    /// </summary>
    public sealed class SpatialGrid
    {
        private readonly float _cellSize;
        private readonly float _inv;
        private readonly int[] _head;
        private readonly int[] _next;
        private readonly int _mask;

        /// <summary>Creates a grid for up to <paramref name="capacity"/> items.</summary>
        public SpatialGrid(float cellSize, int capacity, int bucketPow2 = 4096)
        {
            if ((bucketPow2 & (bucketPow2 - 1)) != 0) throw new ArgumentException("bucket count must be a power of two");
            _cellSize = cellSize;
            _inv = 1f / cellSize;
            _head = new int[bucketPow2];
            _next = new int[capacity];
            _mask = bucketPow2 - 1;
        }

        /// <summary>Cell size in world units.</summary>
        public float CellSize => _cellSize;

        /// <summary>Removes all items.</summary>
        public void Clear() => Array.Fill(_head, -1);

        /// <summary>Inserts item <paramref name="index"/> at <paramref name="pos"/>.</summary>
        public void Insert(int index, Vec2 pos)
        {
            int b = Bucket(Cell(pos.X), Cell(pos.Y));
            _next[index] = _head[b];
            _head[b] = index;
        }

        /// <summary>
        /// Writes candidate indices whose cell overlaps the circle into <paramref name="results"/>.
        /// Candidates may lie outside the circle; callers must distance-check. Returns the count written.
        /// </summary>
        public int Query(Vec2 center, float radius, int[] results)
        {
            int minX = Cell(center.X - radius), maxX = Cell(center.X + radius);
            int minY = Cell(center.Y - radius), maxY = Cell(center.Y + radius);
            int n = 0;
            // Guard against enormous radii visiting the same bucket repeatedly.
            if ((maxX - minX + 1) * (maxY - minY + 1) > _head.Length) return QueryAll(results);
            for (int cx = minX; cx <= maxX; cx++)
            for (int cy = minY; cy <= maxY; cy++)
            {
                for (int i = _head[Bucket(cx, cy)]; i >= 0; i = _next[i])
                {
                    if (n >= results.Length) return n;
                    results[n++] = i;
                }
            }
            return n;
        }

        private int QueryAll(int[] results)
        {
            int n = 0;
            for (int b = 0; b < _head.Length; b++)
                for (int i = _head[b]; i >= 0; i = _next[i])
                {
                    if (n >= results.Length) return n;
                    results[n++] = i;
                }
            return n;
        }

        private int Cell(float v) => (int)MathF.Floor(v * _inv);

        private int Bucket(int cx, int cy)
        {
            unchecked { return ((cx * 73856093) ^ (cy * 19349663)) & _mask; }
        }
    }
}
