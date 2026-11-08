using System;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    public sealed partial class GameSimulation
    {
        /// <summary>Damage multiplier from passives and research.</summary>
        public float MightMultiplier => 1f + PassiveValue(PassiveKind.Might) + (Setup.Modifiers?.Might ?? 0f);

        /// <summary>Area multiplier from passives.</summary>
        public float AreaMultiplier => 1f + PassiveValue(PassiveKind.Area);

        /// <summary>Effective damage of a gadget at a level.</summary>
        public float GadgetDamage(GadgetDef d, int level) => d.damage * (1f + d.damagePerLevel * (level - 1)) * MightMultiplier;

        /// <summary>Effective cooldown of a gadget at a level (min 0.1 s).</summary>
        public float GadgetCooldown(GadgetDef d, int level)
        {
            float cd = d.cooldown * (1f - d.cooldownPerLevel * (level - 1)) * (1f - PassiveValue(PassiveKind.Haste));
            return MathF.Max(0.1f, cd);
        }

        /// <summary>Effective projectile/blade/chain count of a gadget at a level.</summary>
        public static int GadgetCount(GadgetDef d, int level) => d.count + (d.countEveryLevels > 0 ? (level - 1) / d.countEveryLevels : 0);

        /// <summary>Radius at which gyro blades orbit.</summary>
        public float BladeOrbitRadius(GadgetDef d) => d.area * AreaMultiplier;

        /// <summary>Hit radius of a single gyro blade.</summary>
        public float BladeHitRadius => 0.55f * AreaMultiplier;

        private void UpdateGadgets(float dt)
        {
            for (int gi = 0; gi < Gadgets.Count; gi++)
            {
                var g = Gadgets[gi];
                var d = Config.gadgets[g.Def];
                float dmg = GadgetDamage(d, g.Level);
                int count = GadgetCount(d, g.Level);
                g.Timer -= dt;

                switch (d.Kind)
                {
                    case GadgetKind.SteamCannon:
                        if (g.Timer <= 0f)
                        {
                            int t = NearestEnemy(Player.Pos, d.range);
                            if (t >= 0)
                            {
                                g.Timer = GadgetCooldown(d, g.Level);
                                Vec2 aim = (Enemies[t].Pos - Player.Pos).Normalized;
                                float baseAngle = MathF.Atan2(aim.Y, aim.X);
                                for (int k = 0; k < count; k++)
                                {
                                    float a = baseAngle + (k - (count - 1) * 0.5f) * 0.14f;
                                    SpawnProjectile(ProjectileKind.Bolt, Player.Pos, Vec2.FromAngle(a) * d.projectileSpeed, dmg,
                                        d.area * AreaMultiplier, d.pierce + g.Level / 4, d.range / d.projectileSpeed + 0.3f);
                                }
                                Emit(SimEventType.GadgetFired, Player.Pos, aim, 0f, g.Def);
                            }
                        }
                        break;

                    case GadgetKind.ArcCoil:
                        if (g.Timer <= 0f && NearestEnemy(Player.Pos, d.range) >= 0)
                        {
                            g.Timer = GadgetCooldown(d, g.Level);
                            ChainLightning(Player.Pos, d.range, d.area * AreaMultiplier, count, dmg);
                        }
                        break;

                    case GadgetKind.GyroBlades:
                    {
                        g.Angle += d.projectileSpeed * dt;
                        float orbit = BladeOrbitRadius(d);
                        float hitR = BladeHitRadius;
                        for (int k = 0; k < count; k++)
                        {
                            Vec2 bp = Player.Pos + Vec2.FromAngle(g.Angle + k * MathF.PI * 2f / count) * orbit;
                            int n = _grid.Query(bp, hitR + 1f, _query);
                            for (int q = 0; q < n; q++)
                            {
                                int i = _query[q];
                                ref var e = ref Enemies[i];
                                if (!e.Active || e.BladeCooldown > 0f) continue;
                                float r = hitR + e.Radius;
                                if (Vec2.SqrDistance(bp, e.Pos) > r * r) continue;
                                e.BladeCooldown = d.cooldown;
                                DamageEnemy(i, dmg);
                            }
                        }
                        break;
                    }

                    case GadgetKind.Mortar:
                        if (g.Timer <= 0f)
                        {
                            int n = _grid.Query(Player.Pos, d.range, _query);
                            int live = 0;
                            for (int q = 0; q < n; q++)
                                if (Enemies[_query[q]].Active && Vec2.SqrDistance(Enemies[_query[q]].Pos, Player.Pos) <= d.range * d.range)
                                    _query[live++] = _query[q];
                            if (live > 0)
                            {
                                g.Timer = GadgetCooldown(d, g.Level);
                                for (int k = 0; k < count; k++)
                                {
                                    Vec2 at = Enemies[_query[Rng.Range(0, live)]].Pos;
                                    SpawnProjectile(ProjectileKind.Shell, at, Vec2.Zero, dmg, d.area * AreaMultiplier, 0, 0.6f);
                                }
                                Emit(SimEventType.GadgetFired, Player.Pos, default, 0f, g.Def);
                            }
                        }
                        break;

                    case GadgetKind.FlameBellows:
                        if (g.Timer <= 0f)
                        {
                            g.Timer = GadgetCooldown(d, g.Level);
                            float range = d.range * AreaMultiplier * (1f + 0.05f * (g.Level - 1));
                            float cosHalf = MathF.Cos(d.area * 0.5f * MathF.PI / 180f);
                            int n = _grid.Query(Player.Pos, range + 1f, _query);
                            for (int q = 0; q < n; q++)
                            {
                                int i = _query[q];
                                ref var e = ref Enemies[i];
                                if (!e.Active) continue;
                                Vec2 to = e.Pos - Player.Pos;
                                float dist = to.Magnitude;
                                if (dist > range + e.Radius) continue;
                                if (dist > 0.5f && Vec2.Dot(to / dist, Player.Facing) < cosHalf) continue;
                                DamageEnemy(i, dmg);
                            }
                            Emit(SimEventType.Flame, Player.Pos, Player.Facing, range, g.Def);
                        }
                        break;

                    case GadgetKind.AetherPulse:
                        if (g.Timer <= 0f)
                        {
                            g.Timer = GadgetCooldown(d, g.Level);
                            float radius = d.area * AreaMultiplier * (1f + 0.08f * (g.Level - 1));
                            DamageArea(Player.Pos, radius, dmg, 0.6f);
                            Emit(SimEventType.Pulse, Player.Pos, default, radius, g.Def);
                        }
                        break;
                }
                Gadgets[gi] = g;
                if (IsOver) return;
            }
        }

        private void SpawnProjectile(ProjectileKind kind, Vec2 pos, Vec2 vel, float damage, float radius, int pierce, float life)
        {
            for (int i = 0; i < Projectiles.Length; i++)
            {
                if (Projectiles[i].Active) continue;
                Projectiles[i] = new Projectile
                {
                    Active = true, Kind = kind, Pos = pos, Vel = vel, Damage = damage, Radius = radius,
                    Pierce = pierce, Life = life, LastHit = -1,
                };
                return;
            }
        }

        private void UpdateProjectiles(float dt)
        {
            for (int p = 0; p < Projectiles.Length; p++)
            {
                ref var pr = ref Projectiles[p];
                if (!pr.Active) continue;
                pr.Life -= dt;

                if (pr.Kind == ProjectileKind.Shell)
                {
                    if (pr.Life > 0f) continue;
                    pr.Active = false;
                    DamageArea(pr.Pos, pr.Radius, pr.Damage);
                    Emit(SimEventType.Explosion, pr.Pos, default, pr.Radius);
                    if (IsOver) return;
                    continue;
                }

                if (pr.Life <= 0f) { pr.Active = false; continue; }
                pr.Pos += pr.Vel * dt;
                int n = _grid.Query(pr.Pos, pr.Radius + 1f, _query);
                for (int q = 0; q < n; q++)
                {
                    int i = _query[q];
                    if (i == pr.LastHit) continue;
                    ref var e = ref Enemies[i];
                    if (!e.Active) continue;
                    float r = pr.Radius + e.Radius;
                    if (Vec2.SqrDistance(pr.Pos, e.Pos) > r * r) continue;
                    pr.LastHit = i;
                    DamageEnemy(i, pr.Damage);
                    if (--pr.Pierce < 0) { pr.Active = false; break; }
                }
                if (IsOver) return;
            }
        }
    }
}
