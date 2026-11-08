using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vamp.Game
{
    /// <summary>
    /// Draws camera-facing sprite billboards and flat ground decals with GPU instancing
    /// (Graphics.RenderMeshInstanced), one batch per material. Implements the swarm renderer of ADR-0002:
    /// hundreds of enemies cost a handful of draw calls and no GameObjects.
    /// </summary>
    public sealed class SpriteBatcher
    {
        private const int Chunk = 511;
        private readonly Mesh _quad;
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<bool> _castShadows = new List<bool>();
        private readonly List<List<Matrix4x4>> _batches = new List<List<Matrix4x4>>();
        private Quaternion _camRot = Quaternion.identity;
        private Vector3 _camUp = Vector3.up;
        private Vector3 _boundsCenter;
        private static readonly Quaternion FlatRot = Quaternion.Euler(90f, 0f, 0f);

        /// <summary>Creates the batcher with a unit quad (normal -Z, pivot centre).</summary>
        public SpriteBatcher()
        {
            _quad = new Mesh { name = "BillboardQuad" };
            _quad.SetVertices(new List<Vector3> { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) });
            _quad.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
            _quad.SetNormals(new List<Vector3> { Vector3.back, Vector3.back, Vector3.back, Vector3.back });
            _quad.SetTriangles(new[] { 0, 2, 1, 2, 3, 1 }, 0);
            _quad.RecalculateBounds();
        }

        /// <summary>Registers a material and returns its batch index.</summary>
        public int Register(Material material, bool castShadows = false)
        {
            material.enableInstancing = true;
            _materials.Add(material);
            _castShadows.Add(castShadows);
            _batches.Add(new List<Matrix4x4>(256));
            return _materials.Count - 1;
        }

        /// <summary>Material of a batch.</summary>
        public Material MaterialAt(int index) => _materials[index];

        /// <summary>Starts a frame; billboards will face <paramref name="cam"/>.</summary>
        public void Begin(Camera cam, Vector3 focus)
        {
            _camRot = cam.transform.rotation;
            _camUp = cam.transform.up;
            _boundsCenter = focus;
            foreach (var b in _batches) b.Clear();
        }

        /// <summary>Adds a billboard standing on <paramref name="ground"/>.</summary>
        public void Billboard(int material, Vector3 ground, float width, float height, bool flipX = false, float lift = 0f)
        {
            if (material < 0) return;
            Vector3 center = ground + _camUp * (height * 0.5f) + Vector3.up * lift;
            _batches[material].Add(Matrix4x4.TRS(center, _camRot, new Vector3(flipX ? -width : width, height, 1f)));
        }

        /// <summary>Adds a flat decal lying on the ground.</summary>
        public void Flat(int material, Vector3 center, float size, float yawDegrees = 0f, float height = 0.03f)
        {
            if (material < 0) return;
            var rot = Quaternion.Euler(0f, yawDegrees, 0f) * FlatRot;
            _batches[material].Add(Matrix4x4.TRS(center + Vector3.up * height, rot, new Vector3(size, size, 1f)));
        }

        /// <summary>Submits all batches for rendering this frame.</summary>
        public void End()
        {
            var bounds = new Bounds(_boundsCenter, new Vector3(400f, 100f, 400f));
            for (int m = 0; m < _materials.Count; m++)
            {
                var list = _batches[m];
                if (list.Count == 0) continue;
                var rp = new RenderParams(_materials[m])
                {
                    worldBounds = bounds,
                    shadowCastingMode = _castShadows[m] ? ShadowCastingMode.On : ShadowCastingMode.Off,
                    receiveShadows = true,
                };
                for (int start = 0; start < list.Count; start += Chunk)
                {
                    int count = Mathf.Min(Chunk, list.Count - start);
                    Graphics.RenderMeshInstanced(rp, _quad, 0, list, count, start);
                }
            }
        }
    }
}
