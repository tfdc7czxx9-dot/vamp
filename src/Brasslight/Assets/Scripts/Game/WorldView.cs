using System.Collections.Generic;
using UnityEngine;
using Vamp.Core.Config;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    /// <summary>
    /// The 3D diorama environment: infinite tiled ground, deterministic props per chunk (rocks, crystals,
    /// fungi) with a pooled set of real-time point lights, sun, lantern and fog. Pure presentation.
    /// </summary>
    public sealed class WorldView
    {
        private const float ChunkSize = 10f;
        private const int ChunkRadius = 3;
        private const float TileSize = 4f;
        private const float GroundSize = 240f;

        private readonly Transform _root;
        private readonly MaterialFactory _materials;
        private readonly GameObject _ground;
        private readonly Light _sun;
        private readonly Light _lantern;
        private readonly List<Light> _lights = new List<Light>();
        private readonly Dictionary<PrimitiveType, Stack<GameObject>> _propPool = new Dictionary<PrimitiveType, Stack<GameObject>>
        {
            { PrimitiveType.Cube, new Stack<GameObject>() },
            { PrimitiveType.Cylinder, new Stack<GameObject>() },
        };
        private readonly Dictionary<GameObject, PrimitiveType> _propType = new Dictionary<GameObject, PrimitiveType>();
        private readonly Dictionary<long, List<GameObject>> _chunks = new Dictionary<long, List<GameObject>>();
        private readonly List<long> _release = new List<long>();
        private readonly List<GameObject> _emissive = new List<GameObject>();
        private Material _groundMat, _rockMat, _crystalMat, _stemMat, _capMat;
        private int _lastChunkX = int.MinValue, _lastChunkY = int.MinValue;
        private uint _seed;
        private BuildConfig _build;
        private int _lightBudget = 6;
        private float _lightTimer;

        /// <summary>Creates sun, lantern and ground.</summary>
        public WorldView(Transform parent, MaterialFactory materials)
        {
            _root = new GameObject("World").transform;
            _root.SetParent(parent, false);
            _materials = materials;

            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(_root, false);
            sunGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            _sun = sunGo.AddComponent<Light>();
            _sun.type = LightType.Directional;
            _sun.shadows = LightShadows.Soft;

            var lanternGo = new GameObject("Lantern");
            lanternGo.transform.SetParent(_root, false);
            _lantern = lanternGo.AddComponent<Light>();
            _lantern.type = LightType.Point;
            _lantern.range = 9f;
            _lantern.intensity = 3f;
            _lantern.color = new Color(1f, 0.78f, 0.45f);

            _ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _ground.name = "Ground";
            Object.Destroy(_ground.GetComponent<Collider>());
            _ground.transform.SetParent(_root, false);
            _ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _ground.transform.localScale = new Vector3(GroundSize, GroundSize, 1f);
            var mr = _ground.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Switches palette and props to a world.</summary>
        public void SetWorld(WorldDef world, int worldIndex, uint seed, BuildConfig build)
        {
            _seed = seed;
            _build = build;
            Color g = VecExt.Hex(world.groundColor), a = VecExt.Hex(world.accentColor), f = VecExt.Hex(world.fogColor);
            DestroyWorldMaterials();

            _groundMat = _materials.Opaque(PixelArt.Ground(g, a, worldIndex), Color.white, Color.black, new Vector2(GroundSize / TileSize, GroundSize / TileSize));
            _ground.GetComponent<MeshRenderer>().sharedMaterial = _groundMat;
            _rockMat = _materials.Opaque(null, g * 0.75f, Color.black, Vector2.one);
            _stemMat = _materials.Opaque(null, Color.Lerp(g, Color.white, 0.35f), Color.black, Vector2.one);
            _crystalMat = _materials.Opaque(null, a, a * 2.2f, Vector2.one);
            _capMat = _materials.Opaque(null, Color.Lerp(a, Color.white, 0.2f), a * 1.2f, Vector2.one);

            _sun.color = Color.Lerp(Color.white, a, 0.25f);
            _sun.intensity = 0.55f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(f, g, 0.6f) * 1.6f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = f;

            foreach (var kv in _chunks) foreach (var p in kv.Value) Release(p);
            _chunks.Clear();
            _lastChunkX = int.MinValue;
        }

        private void DestroyWorldMaterials()
        {
            if (_groundMat != null && _groundMat.mainTexture != null) Object.Destroy(_groundMat.mainTexture);
            foreach (var m in new[] { _groundMat, _rockMat, _crystalMat, _stemMat, _capMat })
                if (m != null) Object.Destroy(m);
        }

        /// <summary>Applies the quality tier (shadows, light budget).</summary>
        public void ApplyQuality(QualityTier tier, float cameraDistance, Color fog)
        {
            _lightBudget = tier switch { QualityTier.Low => 2, QualityTier.Medium => 4, QualityTier.High => 8, _ => 12 };
            _sun.shadows = tier == QualityTier.Low ? LightShadows.None : LightShadows.Soft;
            RenderSettings.fogStartDistance = cameraDistance + 6f;
            RenderSettings.fogEndDistance = cameraDistance + 45f;
            RenderSettings.fogColor = fog;
            while (_lights.Count < _lightBudget)
            {
                var go = new GameObject("PropLight");
                go.transform.SetParent(_root, false);
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 6f;
                l.intensity = 2.2f;
                l.shadows = LightShadows.None;
                _lights.Add(l);
            }
            for (int i = 0; i < _lights.Count; i++) _lights[i].enabled = i < _lightBudget;
        }

        /// <summary>Moves ground and lantern with the player, streams prop chunks, assigns lights.</summary>
        public void Update(Vector3 player, float dt)
        {
            _ground.transform.position = new Vector3(Mathf.Round(player.x / TileSize) * TileSize, 0f, Mathf.Round(player.z / TileSize) * TileSize);
            _lantern.transform.position = player + new Vector3(0f, 2.2f, -0.5f);

            int cx = Mathf.FloorToInt(player.x / ChunkSize), cy = Mathf.FloorToInt(player.z / ChunkSize);
            if (cx != _lastChunkX || cy != _lastChunkY)
            {
                _lastChunkX = cx; _lastChunkY = cy;
                StreamChunks(cx, cy);
            }

            _lightTimer -= dt;
            if (_lightTimer <= 0f)
            {
                _lightTimer = 0.25f;
                AssignLights(player);
            }
        }

        private void StreamChunks(int cx, int cy)
        {
            _release.Clear();
            foreach (var key in _chunks.Keys)
            {
                int x = (int)(key >> 32), y = (int)(key & 0xFFFFFFFF);
                if (Mathf.Abs(x - cx) > ChunkRadius || Mathf.Abs(y - cy) > ChunkRadius) _release.Add(key);
            }
            foreach (var key in _release)
            {
                foreach (var p in _chunks[key]) Release(p);
                _chunks.Remove(key);
            }
            for (int x = cx - ChunkRadius; x <= cx + ChunkRadius; x++)
            for (int y = cy - ChunkRadius; y <= cy + ChunkRadius; y++)
            {
                long key = ((long)x << 32) | (uint)y;
                if (!_chunks.ContainsKey(key)) _chunks[key] = Populate(x, y);
            }
            _emissive.Clear();
            foreach (var list in _chunks.Values)
                foreach (var p in list)
                    if (p.name == "Crystal" || p.name == "Fungus") _emissive.Add(p);
        }

        private List<GameObject> Populate(int x, int y)
        {
            var list = new List<GameObject>(3);
            uint h = Rng.Hash(x, y, _seed ^ 0xBEEFu);
            var rng = new Rng(h);
            int count = rng.Range(0, 4);
            for (int i = 0; i < count; i++)
            {
                var pos = new Vec2((x + rng.NextFloat()) * ChunkSize, (y + rng.NextFloat()) * ChunkSize);
                if (pos.SqrMagnitude < 36f || NearPlot(pos)) continue;
                float roll = rng.NextFloat();
                float scale = rng.Range(0.6f, 1.6f);
                GameObject go;
                if (roll < 0.55f)
                {
                    go = Acquire(PrimitiveType.Cube, "Rock", _rockMat);
                    go.transform.localScale = new Vector3(scale * 1.4f, scale * 0.8f, scale * 1.1f);
                    go.transform.rotation = Quaternion.Euler(rng.Range(-8f, 8f), rng.Range(0f, 360f), rng.Range(-8f, 8f));
                    go.transform.position = pos.ToWorld(scale * 0.3f);
                }
                else if (roll < 0.82f)
                {
                    go = Acquire(PrimitiveType.Cube, "Crystal", _crystalMat);
                    go.transform.localScale = new Vector3(0.45f * scale, 2.2f * scale, 0.45f * scale);
                    go.transform.rotation = Quaternion.Euler(rng.Range(-20f, 20f), rng.Range(0f, 360f), 45f + rng.Range(-15f, 15f));
                    go.transform.position = pos.ToWorld(0.6f * scale);
                }
                else
                {
                    go = Acquire(PrimitiveType.Cylinder, "Fungus", _capMat);
                    go.transform.localScale = new Vector3(1.4f * scale, 0.12f * scale, 1.4f * scale);
                    go.transform.rotation = Quaternion.identity;
                    go.transform.position = pos.ToWorld(1.3f * scale);
                }
                list.Add(go);
            }
            return list;
        }

        private bool NearPlot(Vec2 pos)
        {
            if (_build == null) return false;
            BuildLattice.CellOf(_build, pos, out int cx, out int cy);
            if (!BuildLattice.HasPlot(cx, cy)) return false;
            return Vec2.SqrDistance(BuildLattice.PlotPosition(_build, cx, cy, _seed), pos) < 9f;
        }

        private GameObject Acquire(PrimitiveType type, string name, Material mat)
        {
            var pool = _propPool[type];
            GameObject go = pool.Count > 0 ? pool.Pop() : null;
            if (go == null)
            {
                go = GameObject.CreatePrimitive(type);
                Object.Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(_root, false);
                _propType[go] = type;
            }
            go.name = name;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.SetActive(true);
            return go;
        }

        private void Release(GameObject go)
        {
            go.SetActive(false);
            _propPool[_propType[go]].Push(go);
        }

        private void AssignLights(Vector3 player)
        {
            _emissive.Sort((a, b) => (a.transform.position - player).sqrMagnitude.CompareTo((b.transform.position - player).sqrMagnitude));
            for (int i = 0; i < _lights.Count; i++)
            {
                var l = _lights[i];
                if (i >= _lightBudget || i >= _emissive.Count) { l.enabled = false; continue; }
                l.enabled = true;
                l.color = _emissive[i].GetComponent<MeshRenderer>().sharedMaterial.GetColor("_BaseColor");
                l.transform.position = _emissive[i].transform.position + Vector3.up * 1.2f;
            }
        }
    }
}
