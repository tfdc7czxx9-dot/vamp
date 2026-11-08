using System;
using System.Collections.Generic;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    public sealed partial class GameSimulation
    {
        private readonly Dictionary<long, int> _structureByCell;
        private long _plotKey = long.MinValue;
        private bool _plotLocked;
        private Vec2 _pendingPlotPos;
        private long _pendingPlotKey;

        /// <summary>Build/upgrade progress 0..1 on the plot the player stands on.</summary>
        public float PlotProgress { get; private set; }

        /// <summary>Position of the plot the player stands on (valid if <see cref="OnPlot"/>).</summary>
        public Vec2 CurrentPlotPos { get; private set; }

        /// <summary>True if the player stands on a build plot.</summary>
        public bool OnPlot => _plotKey != long.MinValue;

        /// <summary>Effective seconds to build/upgrade.</summary>
        public float BuildSeconds
        {
            get
            {
                float bonus = PassiveValue(PassiveKind.Engineering) + (Setup.Modifiers?.BuildTime ?? 0f);
                return Config.build.buildSeconds * MathF.Max(0.25f, 1f - bonus);
            }
        }

        /// <summary>Upgrade cost of a structure at its current level (cost * level).</summary>
        public int UpgradeCost(in Structure s) => Config.structures[s.Def].cost * s.Level;

        /// <summary>Level multiplier for damage/HP: 1 + scaling*(level-1).</summary>
        public float LevelMultiplier(int level) => 1f + Config.build.levelScaling * (level - 1);

        /// <summary>Structure index on a plot cell, or -1.</summary>
        public int StructureAt(long cellKey) => _structureByCell.TryGetValue(cellKey, out int i) ? i : -1;

        /// <summary>
        /// Writes plots (position + structure index or -1) within <paramref name="radius"/> of a point.
        /// Returns the number written.
        /// </summary>
        public int PlotsNear(Vec2 center, float radius, Vec2[] positions, int[] structures)
        {
            var b = Config.build;
            int r = (int)MathF.Ceiling(radius / b.latticeCellSize);
            BuildLattice.CellOf(b, center, out int ccx, out int ccy);
            int n = 0;
            for (int cx = ccx - r; cx <= ccx + r; cx++)
            for (int cy = ccy - r; cy <= ccy + r; cy++)
            {
                if (!BuildLattice.HasPlot(cx, cy) || n >= positions.Length) continue;
                Vec2 p = BuildLattice.PlotPosition(b, cx, cy, Setup.Seed);
                if (Vec2.SqrDistance(p, center) > radius * radius) continue;
                positions[n] = p;
                structures[n] = StructureAt(BuildLattice.Key(cx, cy));
                n++;
            }
            return n;
        }

        private bool FindPlotUnderPlayer(out long key, out Vec2 pos)
        {
            var b = Config.build;
            BuildLattice.CellOf(b, Player.Pos, out int ccx, out int ccy);
            float rSq = b.plotRadius * b.plotRadius;
            for (int cx = ccx - 1; cx <= ccx + 1; cx++)
            for (int cy = ccy - 1; cy <= ccy + 1; cy++)
            {
                if (!BuildLattice.HasPlot(cx, cy)) continue;
                Vec2 p = BuildLattice.PlotPosition(b, cx, cy, Setup.Seed);
                if (Vec2.SqrDistance(p, Player.Pos) > rSq) continue;
                key = BuildLattice.Key(cx, cy);
                pos = p;
                return true;
            }
            key = long.MinValue;
            pos = default;
            return false;
        }

        private void UpdateBuildPlots(float dt)
        {
            if (Phase != RunPhase.Playing) return;
            if (!FindPlotUnderPlayer(out long key, out Vec2 pos))
            {
                _plotKey = long.MinValue;
                _plotLocked = false;
                PlotProgress = 0f;
                return;
            }
            if (key != _plotKey)
            {
                _plotKey = key;
                _plotLocked = false;
                PlotProgress = 0f;
            }
            CurrentPlotPos = pos;
            if (_plotLocked) return;

            int existing = StructureAt(key);
            if (existing >= 0 && Structures[existing].Level >= Config.build.maxStructureLevel) return;

            PlotProgress += dt / BuildSeconds;
            if (PlotProgress < 1f) return;
            PlotProgress = 1f;
            _plotLocked = true; // the player must step off before the plot triggers again

            if (existing < 0)
            {
                OpenBuildChoice(key, pos);
                return;
            }

            ref var s = ref Structures[existing];
            int cost = UpgradeCost(s);
            if (Player.Brass < cost)
            {
                Emit(SimEventType.NotEnoughBrass, pos, default, cost);
                return;
            }
            Player.Brass -= cost;
            s.Level++;
            var d = Config.structures[s.Def];
            s.MaxHp = d.hp * LevelMultiplier(s.Level);
            s.Hp = s.MaxHp;
            Stats.StructuresUpgraded++;
            Emit(SimEventType.StructureUpgraded, pos, default, s.Level, s.Def);
        }

        private void OpenBuildChoice(long key, Vec2 pos)
        {
            BuildOffers.Clear();
            int want = System.Math.Min(Config.build.choiceCount, Config.structures.Length);
            while (BuildOffers.Count < want)
            {
                int pick = Rng.Range(0, Config.structures.Length);
                if (!BuildOffers.Contains(pick)) BuildOffers.Add(pick);
            }
            _pendingPlotKey = key;
            _pendingPlotPos = pos;
            Phase = RunPhase.BuildChoice;
        }

        /// <summary>True if the player can afford a structure def right now.</summary>
        public bool CanAfford(int structureDef) => Player.Brass >= Config.structures[structureDef].cost;

        /// <summary>
        /// Resolves the build choice. <paramref name="offerIndex"/> indexes <see cref="BuildOffers"/>;
        /// -1 cancels. Returns false (and stays in the choice) if the option is unaffordable.
        /// </summary>
        public bool ChooseStructure(int offerIndex)
        {
            if (Phase != RunPhase.BuildChoice) return false;
            if (offerIndex < 0 || offerIndex >= BuildOffers.Count)
            {
                Phase = RunPhase.Playing;
                return true;
            }
            int def = BuildOffers[offerIndex];
            if (!CanAfford(def)) return false;
            int slot = -1;
            for (int i = 0; i < Structures.Length; i++) if (!Structures[i].Active) { slot = i; break; }
            if (slot < 0) { Phase = RunPhase.Playing; return true; }

            var d = Config.structures[def];
            Player.Brass -= d.cost;
            Structures[slot] = new Structure
            {
                Active = true, Def = def, Level = 1, Pos = _pendingPlotPos, MaxHp = d.hp, Hp = d.hp,
                Armed = true, CellKey = _pendingPlotKey,
            };
            _structureByCell[_pendingPlotKey] = slot;
            Stats.StructuresBuilt++;
            Emit(SimEventType.StructureBuilt, _pendingPlotPos, default, 1f, def);
            Phase = RunPhase.Playing;
            return true;
        }

        private void UpdateStructures(float dt)
        {
            for (int i = 0; i < Structures.Length; i++)
            {
                ref var s = ref Structures[i];
                if (!s.Active) continue;
                var d = Config.structures[s.Def];
                if (s.MaxHp > 0f && s.Hp <= 0f)
                {
                    s.Active = false;
                    _structureByCell.Remove(s.CellKey);
                    Emit(SimEventType.StructureDestroyed, s.Pos, default, 0f, s.Def);
                    continue;
                }

                float mult = LevelMultiplier(s.Level);
                s.Timer -= dt;
                switch (d.Kind)
                {
                    case StructureKind.SteamTurret:
                        if (s.Timer <= 0f)
                        {
                            int t = NearestEnemy(s.Pos, d.range);
                            if (t < 0) break;
                            s.Timer = d.cooldown;
                            Vec2 dir = (Enemies[t].Pos - s.Pos).Normalized;
                            SpawnProjectile(ProjectileKind.TurretBolt, s.Pos, dir * 16f, d.damage * mult, 0.25f, 0, d.range / 16f + 0.2f);
                        }
                        break;

                    case StructureKind.MineField:
                        if (!s.Armed)
                        {
                            if (s.Timer <= 0f) s.Armed = true;
                            break;
                        }
                        if (NearestEnemy(s.Pos, d.range) >= 0)
                        {
                            s.Armed = false;
                            s.Timer = d.cooldown;
                            DamageArea(s.Pos, d.area, d.damage * mult);
                            Emit(SimEventType.Explosion, s.Pos, default, d.area, -1);
                        }
                        break;

                    case StructureKind.LightningMast:
                        if (s.Timer <= 0f && NearestEnemy(s.Pos, d.range) >= 0)
                        {
                            s.Timer = d.cooldown;
                            ChainLightning(s.Pos, d.range, 4f, d.chainCount, d.damage * mult);
                        }
                        break;

                    case StructureKind.LureBeacon:
                        s.Cycle += dt;
                        if (d.intervalSeconds > 0f && s.Cycle >= d.intervalSeconds) s.Cycle -= d.intervalSeconds;
                        break;

                    case StructureKind.FieldWorkshop:
                    {
                        float rangeSq = d.range * d.range;
                        for (int c = 0; c < Companions.Length; c++)
                        {
                            ref var comp = ref Companions[c];
                            if (!comp.Active || Vec2.SqrDistance(comp.Pos, s.Pos) > rangeSq) continue;
                            comp.Hp = MathF.Min(comp.MaxHp, comp.Hp + comp.MaxHp * d.healPercentPerSecond * mult * dt);
                        }
                        if (Vec2.SqrDistance(Player.Pos, s.Pos) <= rangeSq)
                            Player.Hp = MathF.Min(Player.MaxHp, Player.Hp + d.playerHealPerSecond * mult * dt);
                        break;
                    }
                }
                if (IsOver) return;
            }
        }
    }
}
