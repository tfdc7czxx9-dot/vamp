using System;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    public sealed partial class GameSimulation
    {
        /// <summary>Index of the remnant the player is currently claiming, or -1.</summary>
        public int ActiveRemnant { get; private set; } = -1;

        /// <summary>True if completing the active remnant will scrap it (crew full).</summary>
        public bool ActiveRemnantWillScrap => CrewCount >= CrewCapacity;

        /// <summary>Effective seconds needed to recruit/scrap.</summary>
        public float RecruitSeconds
        {
            get
            {
                float bonus = PassiveValue(PassiveKind.Charisma) + (Setup.Modifiers?.RecruitTime ?? 0f);
                return Config.recruit.recruitSeconds * MathF.Max(0.25f, 1f - bonus);
            }
        }

        private void SpawnRemnant(in Enemy elite)
        {
            for (int i = 0; i < Remnants.Length; i++)
            {
                if (Remnants[i].Active) continue;
                Remnants[i] = new Remnant
                {
                    Active = true, Def = elite.Def, Pos = elite.Pos, Life = Config.recruit.remnantLifetime,
                    EliteHp = elite.MaxHp, EliteDamage = Config.enemies[elite.Def].damage,
                };
                return;
            }
        }

        private void UpdateRemnants(float dt)
        {
            float ringSq = Config.recruit.ringRadius * Config.recruit.ringRadius;
            int nearest = -1;
            float nearestSq = float.MaxValue;
            for (int i = 0; i < Remnants.Length; i++)
            {
                ref var r = ref Remnants[i];
                if (!r.Active) continue;
                r.Life -= dt;
                if (r.Life <= 0f) { r.Active = false; continue; }
                float sq = Vec2.SqrDistance(r.Pos, Player.Pos);
                if (sq <= ringSq && sq < nearestSq) { nearestSq = sq; nearest = i; }
            }

            ActiveRemnant = nearest;
            if (nearest < 0) return;

            // Only the nearest remnant progresses; leaving the ring pauses progress (no decay).
            ref var rem = ref Remnants[nearest];
            rem.Progress += dt / RecruitSeconds;
            if (rem.Progress < 1f) return;

            rem.Active = false;
            ActiveRemnant = -1;
            if (CrewCount < CrewCapacity) Recruit(rem);
            else
            {
                Player.Brass += Config.recruit.scrapBrass;
                Stats.BrassCollected += Config.recruit.scrapBrass;
                Stats.Scrapped++;
                Emit(SimEventType.Scrapped, rem.Pos, default, Config.recruit.scrapBrass, rem.Def);
            }
        }

        private void Recruit(in Remnant rem)
        {
            var rc = Config.recruit;
            for (int c = 0; c < Companions.Length; c++)
            {
                if (Companions[c].Active) continue;
                float hp = MathF.Max(rc.companionMinHp, rem.EliteHp * rc.companionHpFraction);
                Companions[c] = new Companion
                {
                    Active = true, Def = rem.Def, Pos = rem.Pos, Facing = new Vec2(1f, 0f), Hp = hp, MaxHp = hp,
                    Damage = rem.EliteDamage * rc.companionDamageMultiplier,
                    Radius = Config.enemies[rem.Def].radius * Config.run.eliteSizeMultiplier,
                };
                Stats.Recruited++;
                Emit(SimEventType.Recruited, rem.Pos, default, 0f, rem.Def);
                return;
            }
        }

        /// <summary>Current attack damage of a companion including the per-level bonus.</summary>
        public float CompanionDamage(in Companion c) =>
            c.Damage * (1f + Config.recruit.companionDamagePerPlayerLevel * (Player.Level - 1)) * MightMultiplier;

        private void UpdateCompanions(float dt)
        {
            var rc = Config.recruit;
            int n = CrewCount;
            int slot = 0;
            float speed = Config.player.speed * rc.companionSpeedMultiplier;
            for (int c = 0; c < Companions.Length; c++)
            {
                ref var comp = ref Companions[c];
                if (!comp.Active) continue;

                if (comp.Hp <= 0f)
                {
                    comp.Active = false;
                    Emit(SimEventType.CompanionLost, comp.Pos, default, 0f, comp.Def);
                    continue;
                }

                float angle = slot * MathF.PI * 2f / MathF.Max(1, n) + Time * 0.4f;
                slot++;
                Vec2 desired = Player.Pos + Vec2.FromAngle(angle) * rc.formationRadius;
                Vec2 to = desired - comp.Pos;
                float dist = to.Magnitude;
                if (dist > 0.05f)
                {
                    float step = MathF.Min(dist, speed * (dist > 6f ? 2f : 1f) * dt);
                    comp.Pos += to / dist * step;
                    comp.Facing = to / dist;
                }

                comp.AttackTimer -= dt;
                if (comp.AttackTimer > 0f) continue;
                int target = NearestEnemy(comp.Pos, rc.companionAttackRange);
                if (target < 0) continue;
                comp.AttackTimer = rc.companionAttackInterval;
                Emit(SimEventType.CompanionAttack, comp.Pos, Enemies[target].Pos, 0f, comp.Def);
                DamageEnemy(target, CompanionDamage(comp));
                if (IsOver) return;
            }
        }
    }
}
