using UnityEngine;
using Vamp.Core.Config;
using Vamp.Core.Math;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    public sealed partial class GameRoot
    {
        private int[] _enemyMat, _enemyFlash, _eliteMat, _leaderMat, _structureMat, _pickupMat;
        private int _shadowMat, _plotMat, _plotActiveMat, _remnantRingMat, _progressRingMat, _crewRingMat;
        private int _boltMat, _turretBoltMat, _shellWarnMat, _bladeMat;
        private readonly Vec2[] _plotPos = new Vec2[64];
        private readonly int[] _plotStruct = new int[64];

        /// <summary>Creates placeholder sprites and registers their instanced materials.</summary>
        private void BuildSprites()
        {
            int n = Config.enemies.Length;
            _enemyMat = new int[n]; _enemyFlash = new int[n]; _eliteMat = new int[n];
            for (int i = 0; i < n; i++)
            {
                var e = Config.enemies[i];
                var w = WorldOf(e.id);
                Color ground = VecExt.Hex(w.groundColor), accent = VecExt.Hex(w.accentColor);
                var rng = new Rng((uint)(i * 131 + 5));
                Color body = Color.Lerp(Color.Lerp(ground, Color.white, 0.35f), accent, rng.Range(0.2f, 0.7f));
                var tex = PixelArt.Creature(e.sprite, body, accent, e.isBoss);
                _enemyMat[i] = Batcher.Register(Materials.Sprite(tex, true, Color.white, Color.black), e.isBoss);
                _enemyFlash[i] = Batcher.Register(Materials.Sprite(tex, true, Color.white, new Color(1.6f, 1.6f, 1.6f)));
                _eliteMat[i] = Batcher.Register(Materials.Sprite(tex, true, new Color(1f, 0.95f, 0.8f), accent * 0.7f), true);
            }
            _leaderMat = new int[Config.leaders.Length];
            for (int i = 0; i < _leaderMat.Length; i++)
                _leaderMat[i] = Batcher.Register(Materials.Sprite(PixelArt.Leader(Config.leaders[i].sprite), true, Color.white, Color.black), true);
            _structureMat = new int[6];
            for (int i = 0; i < 6; i++)
                _structureMat[i] = Batcher.Register(Materials.Sprite(PixelArt.Structure(i), true, Color.white, i == 3 || i == 4 ? new Color(0.6f, 0.5f, 0.2f) : Color.black), true);
            _pickupMat = new int[3];
            _pickupMat[0] = Batcher.Register(Materials.Sprite(PixelArt.Pickup(0), true, Color.white, new Color(0.6f, 1.6f, 2f)));
            _pickupMat[1] = Batcher.Register(Materials.Sprite(PixelArt.Pickup(1), true, Color.white, new Color(0.8f, 0.6f, 0.2f)));
            _pickupMat[2] = Batcher.Register(Materials.Sprite(PixelArt.Pickup(2), true, Color.white, new Color(0.5f, 0.2f, 0.2f)));
            _shadowMat = Batcher.Register(Materials.Sprite(PixelArt.BlobShadow(), false, new Color(0f, 0f, 0f, 1f), Color.black));
            _plotMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(48, 3, new Color(0.85f, 0.65f, 0.25f), true), true, Color.white, new Color(0.5f, 0.35f, 0.1f)));
            _plotActiveMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(48, 4, new Color(1f, 0.85f, 0.4f)), true, Color.white, new Color(2f, 1.5f, 0.6f)));
            _remnantRingMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(48, 3, new Color(0.4f, 0.95f, 1f), true), true, Color.white, new Color(0.6f, 1.6f, 2f)));
            _progressRingMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(48, 5, new Color(0.6f, 1f, 1f)), true, Color.white, new Color(1f, 2.5f, 3f)));
            _crewRingMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(32, 2, new Color(0.95f, 0.75f, 0.3f)), true, Color.white, new Color(1f, 0.7f, 0.2f)));
            _boltMat = Batcher.Register(Materials.Sprite(PixelArt.Dot(8, new Color(1f, 0.8f, 0.35f)), true, Color.white, new Color(2.5f, 1.8f, 0.6f)));
            _turretBoltMat = Batcher.Register(Materials.Sprite(PixelArt.Dot(6, new Color(0.6f, 0.9f, 1f)), true, Color.white, new Color(1f, 2f, 3f)));
            _shellWarnMat = Batcher.Register(Materials.Sprite(PixelArt.Ring(32, 2, new Color(1f, 0.3f, 0.2f), true), true, Color.white, new Color(2f, 0.4f, 0.2f)));
            _bladeMat = Batcher.Register(Materials.Sprite(PixelArt.Pickup(1), true, Color.white, new Color(1.5f, 1.1f, 0.4f)));
        }

        private WorldDef WorldOf(string enemyId)
        {
            foreach (var w in Config.worlds)
            {
                if (w.bossId == enemyId) return w;
                foreach (var id in w.enemyIds) if (id == enemyId) return w;
            }
            return Config.worlds[0];
        }

        private void DrawWorld()
        {
            var sim = Sim;
            var cfg = Config;
            Vector3 focus = sim.Player.Pos.ToWorld();
            Batcher.Begin(Rig.Cam, focus);
            float t = Time.time;

            // Build plots and structures.
            int plots = sim.PlotsNear(sim.Player.Pos, 32f, _plotPos, _plotStruct);
            for (int i = 0; i < plots; i++)
            {
                var p = _plotPos[i].ToWorld();
                int si = _plotStruct[i];
                bool current = sim.OnPlot && Vec2.SqrDistance(sim.CurrentPlotPos, _plotPos[i]) < 0.01f;
                if (si < 0)
                {
                    Batcher.Flat(_plotMat, p, cfg.build.plotRadius * 2f, t * 20f);
                    if (current) Batcher.Flat(_plotActiveMat, p, cfg.build.plotRadius * 2f * Mathf.Max(0.15f, sim.PlotProgress), 0f, 0.05f);
                    continue;
                }
                ref var s = ref sim.Structures[si];
                var def = cfg.structures[s.Def];
                float size = 1.7f + 0.12f * (s.Level - 1);
                Batcher.Flat(_shadowMat, p, size * 0.9f);
                Batcher.Billboard(_structureMat[(int)def.Kind], p, size, size);
                if (current) Batcher.Flat(_plotActiveMat, p, cfg.build.plotRadius * 2f * Mathf.Max(0.15f, sim.PlotProgress), 0f, 0.05f);
                if (def.Kind == StructureKind.LureBeacon && s.Cycle < def.activeSeconds) Batcher.Flat(_shellWarnMat, p, def.range * 0.6f + Mathf.Sin(t * 6f) * 0.3f, t * 40f);
            }

            // Remnants: stunned elites lying on the ground inside their ring.
            for (int i = 0; i < sim.Remnants.Length; i++)
            {
                ref var r = ref sim.Remnants[i];
                if (!r.Active) continue;
                var p = r.Pos.ToWorld();
                float blink = r.Life < 3f && Mathf.Repeat(t * 6f, 1f) < 0.3f ? 0f : 1f;
                if (blink > 0f) Batcher.Flat(_remnantRingMat, p, cfg.recruit.ringRadius * 2f, t * -30f);
                if (r.Progress > 0f) Batcher.Flat(_progressRingMat, p, cfg.recruit.ringRadius * 2f * r.Progress, 0f, 0.05f);
                Batcher.Flat(_enemyMat[r.Def], p, cfg.enemies[r.Def].radius * cfg.run.eliteSizeMultiplier * 3f, 90f + Mathf.Sin(t * 3f) * 8f, 0.08f);
            }

            // Pickups.
            for (int i = 0; i < sim.Pickups.Length; i++)
            {
                ref var pk = ref sim.Pickups[i];
                if (!pk.Active) continue;
                float bob = 0.15f + Mathf.Sin(t * 5f + i) * 0.08f;
                float sz = pk.Kind == PickupKind.Xp ? 0.42f + Mathf.Min(0.3f, pk.Value * 0.03f) : 0.5f;
                Batcher.Billboard(_pickupMat[(int)pk.Kind], pk.Pos.ToWorld(), sz, sz, false, bob);
            }

            // Enemies.
            for (int i = 0; i < sim.Enemies.Length; i++)
            {
                ref var e = ref sim.Enemies[i];
                if (!e.Active) continue;
                var p = e.Pos.ToWorld();
                float size = e.Radius * 3.2f;
                float squash = 1f + Mathf.Sin(t * 10f + i * 1.7f) * 0.06f;
                int mat = e.Flash > 0f ? _enemyFlash[e.Def] : e.Elite || e.Boss ? _eliteMat[e.Def] : _enemyMat[e.Def];
                Batcher.Flat(_shadowMat, p, size * 0.7f);
                Batcher.Billboard(mat, p, size / squash, size * squash, e.Facing.X < 0f);
            }

            // Companions.
            for (int i = 0; i < sim.Companions.Length; i++)
            {
                ref var c = ref sim.Companions[i];
                if (!c.Active) continue;
                var p = c.Pos.ToWorld();
                float size = c.Radius * 3.2f;
                Batcher.Flat(_crewRingMat, p, size * 0.9f, t * 50f);
                Batcher.Flat(_shadowMat, p, size * 0.7f);
                Batcher.Billboard(_eliteMat[c.Def], p, size, size * (1f + Mathf.Sin(t * 8f + i) * 0.05f), c.Facing.X < 0f);
            }

            // Player.
            {
                var p = sim.Player.Pos.ToWorld();
                bool visible = sim.Player.Invuln <= 0f || Mathf.Repeat(t * 12f, 1f) < 0.6f;
                Batcher.Flat(_shadowMat, p, 0.9f);
                float walk = Input.Move.sqrMagnitude > 0.01f ? Mathf.Abs(Mathf.Sin(t * 12f)) * 0.08f : 0f;
                if (visible) Batcher.Billboard(_leaderMat[sim.Setup.Leader], p, 1.1f, 1.25f, sim.Player.Facing.X < 0f, walk);
            }

            // Gyro blades (positions mirror the simulation).
            foreach (var g in sim.Gadgets)
            {
                var d = cfg.gadgets[g.Def];
                if (d.Kind != GadgetKind.GyroBlades) continue;
                int count = GameSimulation.GadgetCount(d, g.Level);
                float orbit = sim.BladeOrbitRadius(d);
                for (int k = 0; k < count; k++)
                {
                    var bp = sim.Player.Pos + Vec2.FromAngle(g.Angle + k * Mathf.PI * 2f / count) * orbit;
                    Batcher.Billboard(_bladeMat, bp.ToWorld(), 0.55f, 0.55f, false, 0.4f);
                }
            }

            // Projectiles.
            for (int i = 0; i < sim.Projectiles.Length; i++)
            {
                ref var pr = ref sim.Projectiles[i];
                if (!pr.Active) continue;
                switch (pr.Kind)
                {
                    case ProjectileKind.Shell: Batcher.Flat(_shellWarnMat, pr.Pos.ToWorld(), pr.Radius * 2f * (1f - pr.Life), t * 90f); break;
                    case ProjectileKind.TurretBolt: Batcher.Billboard(_turretBoltMat, pr.Pos.ToWorld(), 0.3f, 0.3f, false, 0.5f); break;
                    default: Batcher.Billboard(_boltMat, pr.Pos.ToWorld(), 0.35f, 0.35f, false, 0.5f); break;
                }
            }

            Fx.Draw(Batcher);
            Batcher.End();
        }
    }
}
