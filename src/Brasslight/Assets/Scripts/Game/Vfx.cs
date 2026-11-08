using System.Collections.Generic;
using UnityEngine;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    /// <summary>Short-lived visual effects driven by simulation events: lightning, shockwaves, sparks, damage numbers.</summary>
    public sealed class Vfx
    {
        private struct Ring { public Vector3 Pos; public float From, To, Life, Max; public int Mat; }
        private struct Spark { public Vector3 Pos, Vel; public float Life, Size; public int Mat; }
        private struct Line { public LineRenderer R; public float Life; }

        /// <summary>A floating damage number in world space.</summary>
        public struct Number { public Vector3 Pos; public float Value, Life; public bool Big; }

        private readonly List<Ring> _rings = new List<Ring>(64);
        private readonly List<Spark> _sparks = new List<Spark>(256);
        private readonly List<Line> _lines = new List<Line>(24);
        private readonly int _ringMat, _sparkMat, _fireMat;
        private readonly Material _lightningMat, _crewMat;
        private readonly Transform _root;
        private readonly System.Random _rand = new System.Random(5);

        /// <summary>Active damage numbers (drawn by the HUD).</summary>
        public readonly List<Number> Numbers = new List<Number>(128);

        /// <summary>Show damage numbers.</summary>
        public bool ShowNumbers = true;

        /// <summary>Creates the effect pools.</summary>
        public Vfx(Transform parent, SpriteBatcher batcher, MaterialFactory mats)
        {
            _root = new GameObject("Vfx").transform;
            _root.SetParent(parent, false);
            _ringMat = batcher.Register(mats.Sprite(PixelArt.Ring(32, 2, Color.white), true, new Color(1f, 0.9f, 0.6f), new Color(2f, 1.6f, 0.8f)));
            _sparkMat = batcher.Register(mats.Sprite(PixelArt.Dot(6, new Color(1f, 0.85f, 0.4f)), true, Color.white, new Color(2.5f, 2f, 1f)));
            _fireMat = batcher.Register(mats.Sprite(PixelArt.Dot(6, new Color(1f, 0.45f, 0.1f)), true, Color.white, new Color(3f, 1.2f, 0.3f)));
            _lightningMat = mats.Glow(new Color(1.8f, 2.6f, 4f));
            _crewMat = mats.Glow(new Color(3f, 2.2f, 0.9f));
            for (int i = 0; i < 24; i++)
            {
                var go = new GameObject("Bolt");
                go.transform.SetParent(_root, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 6;
                lr.widthMultiplier = 0.09f;
                lr.useWorldSpace = true;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.enabled = false;
                _lines.Add(new Line { R = lr });
            }
        }

        /// <summary>Reacts to one simulation event. Returns a camera shake amount.</summary>
        public float Handle(in SimEvent e)
        {
            switch (e.Type)
            {
                case SimEventType.Lightning: Bolt(e.A.ToWorld(0.8f), e.B.ToWorld(0.8f), _lightningMat, 0.12f, true); return 0f;
                case SimEventType.CompanionAttack: Bolt(e.A.ToWorld(0.6f), e.B.ToWorld(0.6f), _crewMat, 0.08f, false); return 0f;
                case SimEventType.Explosion: AddRing(e.A, 0.3f, e.Value * 2f, 0.3f); Burst(e.A, 10, _fireMat, 6f); return 0.15f;
                case SimEventType.Pulse: AddRing(e.A, e.Value * 1.6f, e.Value * 2.1f, 0.2f); return 0f;
                case SimEventType.Flame: Cone(e.A, e.B, e.Value); return 0f;
                case SimEventType.EnemyKilled: Burst(e.A, e.Value > 0f ? 18 : 4, _sparkMat, 4f); return e.Value > 0f ? 0.25f : 0f;
                case SimEventType.EnemyHit:
                    if (ShowNumbers && Numbers.Count < 120) Numbers.Add(new Number { Pos = e.A.ToWorld(1.2f), Value = e.Value, Life = 0.6f, Big = e.Value >= 30f });
                    return 0f;
                case SimEventType.PlayerHit: Burst(e.A, 6, _fireMat, 3f); return 0.35f;
                case SimEventType.Recruited: AddRing(e.A, 0.5f, 5f, 0.6f); Burst(e.A, 24, _sparkMat, 5f); return 0.2f;
                case SimEventType.StructureBuilt:
                case SimEventType.StructureUpgraded: AddRing(e.A, 0.5f, 4f, 0.5f); Burst(e.A, 16, _sparkMat, 4f); return 0.15f;
                case SimEventType.StructureDestroyed: Burst(e.A, 20, _fireMat, 5f); return 0.3f;
                case SimEventType.LevelUp: AddRing(e.A, 0.5f, 7f, 0.5f); return 0f;
                case SimEventType.BossSpawned: return 0.8f;
                case SimEventType.BossCharge: AddRing(e.A, 1f, 4f, 0.4f); return 0f;
                case SimEventType.Evolved: AddRing(e.A, 0.5f, 10f, 0.9f); Burst(e.A, 40, _sparkMat, 7f); return 0.4f;
                case SimEventType.Synergy: AddRing(e.A, 0.5f, 5f, 0.6f); return 0.2f;
                case SimEventType.PlayerRevived: AddRing(e.A, 0.5f, 9f, 0.8f); return 0.6f;
            }
            return 0f;
        }

        private void AddRing(Vec2 p, float from, float to, float life)
        {
            if (_rings.Count < 64) _rings.Add(new Ring { Pos = p.ToWorld(), From = from, To = to, Life = life, Max = life, Mat = _ringMat });
        }

        private void Burst(Vec2 p, int count, int mat, float speed)
        {
            for (int i = 0; i < count && _sparks.Count < 256; i++)
            {
                var dir = new Vector3((float)_rand.NextDouble() * 2f - 1f, (float)_rand.NextDouble() * 1.5f, (float)_rand.NextDouble() * 2f - 1f);
                _sparks.Add(new Spark { Pos = p.ToWorld(0.5f), Vel = dir * speed, Life = 0.35f + (float)_rand.NextDouble() * 0.3f, Size = 0.18f, Mat = mat });
            }
        }

        private void Cone(Vec2 origin, Vec2 facing, float range)
        {
            var f = facing.ToWorld().normalized;
            for (int i = 0; i < 6 && _sparks.Count < 256; i++)
            {
                var jitter = Quaternion.Euler(0f, ((float)_rand.NextDouble() - 0.5f) * 60f, 0f) * f;
                _sparks.Add(new Spark { Pos = origin.ToWorld(0.6f), Vel = jitter * range * 3f, Life = 0.3f, Size = 0.3f, Mat = _fireMat });
            }
        }

        private void Bolt(Vector3 a, Vector3 b, Material mat, float life, bool jagged)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i].Life > 0f) continue;
                var lr = _lines[i].R;
                lr.sharedMaterial = mat;
                for (int k = 0; k < 6; k++)
                {
                    float t = k / 5f;
                    var p = Vector3.Lerp(a, b, t);
                    if (jagged && k > 0 && k < 5) p += new Vector3((float)_rand.NextDouble() - 0.5f, (float)_rand.NextDouble() * 0.5f, (float)_rand.NextDouble() - 0.5f) * 0.6f;
                    lr.SetPosition(k, p);
                }
                lr.enabled = true;
                _lines[i] = new Line { R = lr, Life = life };
                return;
            }
        }

        /// <summary>Advances all effects.</summary>
        public void Update(float dt)
        {
            for (int i = _rings.Count - 1; i >= 0; i--)
            {
                var r = _rings[i]; r.Life -= dt;
                if (r.Life <= 0f) _rings.RemoveAt(i); else _rings[i] = r;
            }
            for (int i = _sparks.Count - 1; i >= 0; i--)
            {
                var s = _sparks[i]; s.Life -= dt; s.Pos += s.Vel * dt; s.Vel.y -= 9f * dt;
                if (s.Pos.y < 0.05f) { s.Pos.y = 0.05f; s.Vel *= 0.5f; }
                if (s.Life <= 0f) _sparks.RemoveAt(i); else _sparks[i] = s;
            }
            for (int i = 0; i < _lines.Count; i++)
            {
                var l = _lines[i];
                if (l.Life <= 0f) continue;
                l.Life -= dt;
                if (l.Life <= 0f) l.R.enabled = false;
                _lines[i] = l;
            }
            for (int i = Numbers.Count - 1; i >= 0; i--)
            {
                var n = Numbers[i]; n.Life -= dt; n.Pos.y += dt * 1.5f;
                if (n.Life <= 0f) Numbers.RemoveAt(i); else Numbers[i] = n;
            }
        }

        /// <summary>Queues ring and spark sprites.</summary>
        public void Draw(SpriteBatcher b)
        {
            foreach (var r in _rings) b.Flat(r.Mat, r.Pos, Mathf.Lerp(r.To, r.From, r.Life / r.Max), 0f, 0.06f);
            foreach (var s in _sparks) b.Billboard(s.Mat, s.Pos, s.Size, s.Size);
        }

        /// <summary>Clears all effects (new run).</summary>
        public void Clear()
        {
            _rings.Clear(); _sparks.Clear(); Numbers.Clear();
            for (int i = 0; i < _lines.Count; i++) { _lines[i].R.enabled = false; _lines[i] = new Line { R = _lines[i].R }; }
        }
    }
}
