using System;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>
    /// Simple steering bot: avoids nearby enemies, claims remnants, visits build plots when it can
    /// afford a structure and resolves choices. Used by automated tests, balance simulations and the
    /// attract/demo mode. Deterministic.
    /// </summary>
    public sealed class AutoPilot
    {
        private readonly Vec2[] _plotPos = new Vec2[32];
        private readonly int[] _plotStruct = new int[32];
        private float _wanderAngle;

        /// <summary>Radius in which enemies repel the bot.</summary>
        public float AvoidRadius = 4.5f;

        /// <summary>Computes the movement input for the current state.</summary>
        public Vec2 Steer(GameSimulation sim)
        {
            var p = sim.Player.Pos;
            Vec2 avoid = Vec2.Zero;
            float avoidSq = AvoidRadius * AvoidRadius;
            int close = 0;
            for (int i = 0; i < sim.Enemies.Length; i++)
            {
                ref var e = ref sim.Enemies[i];
                if (!e.Active) continue;
                Vec2 d = p - e.Pos;
                float sq = d.SqrMagnitude;
                if (sq > avoidSq || sq < 1e-6f) continue;
                close++;
                float w = (e.Boss ? 3f : e.Elite ? 2f : 1f) / sq;
                avoid += d * w;
            }

            // Dodge hostile shots (a human reads the telegraph and sidesteps).
            for (int i = 0; i < sim.Projectiles.Length; i++)
            {
                ref var pr = ref sim.Projectiles[i];
                if (!pr.Active || pr.Kind != ProjectileKind.EnemyShot) continue;
                Vec2 d = p - pr.Pos;
                float sq = d.SqrMagnitude;
                if (sq > 9f || sq < 1e-6f) continue;
                // Move perpendicular to the shot's travel direction.
                Vec2 v = pr.Vel.Normalized;
                Vec2 side = new Vec2(-v.Y, v.X);
                if (Vec2.Dot(side, d) < 0f) side = -side;
                avoid += side * (2f / sq);
            }

            Vec2 goal = Goal(sim, out bool hold);
            Vec2 toGoal = goal - p;
            float goalDist = toGoal.Magnitude;
            Vec2 seek = goalDist > 0.3f ? toGoal / goalDist : Vec2.Zero;

            // Stand still on a goal (remnant/plot) unless enemies press in.
            bool healthy = sim.Player.Hp > sim.Player.MaxHp * 0.5f;
            if (hold && goalDist < 0.6f && (close < 4 || healthy)) return Vec2.Zero;

            _wanderAngle += 0.01f;
            Vec2 wander = Vec2.FromAngle(_wanderAngle) * 0.35f;
            Vec2 move = seek * 1.0f + avoid.Normalized * MathF.Min(2.5f, avoid.Magnitude * 2f) + wander;
            return move.ClampMagnitude(1f);
        }

        private Vec2 Goal(GameSimulation sim, out bool hold)
        {
            var p = sim.Player.Pos;
            hold = true;
            // 1) Nearest remnant within 10 units.
            int best = -1;
            float bestSq = 100f;
            for (int i = 0; i < sim.Remnants.Length; i++)
            {
                if (!sim.Remnants[i].Active) continue;
                float sq = Vec2.SqrDistance(p, sim.Remnants[i].Pos);
                if (sq < bestSq) { bestSq = sq; best = i; }
            }
            if (best >= 0) return sim.Remnants[best].Pos;

            // 2) A plot within 14 units if we can afford something there.
            int n = sim.PlotsNear(p, 14f, _plotPos, _plotStruct);
            int cheapest = int.MaxValue;
            foreach (var s in sim.Config.structures) cheapest = System.Math.Min(cheapest, s.cost);
            float bestPlot = float.MaxValue;
            Vec2 plot = default;
            for (int i = 0; i < n; i++)
            {
                int s = _plotStruct[i];
                bool useful = s < 0
                    ? sim.Player.Brass >= cheapest
                    : sim.Structures[s].Level < sim.Config.build.maxStructureLevel && sim.Player.Brass >= sim.UpgradeCost(sim.Structures[s]);
                if (!useful) continue;
                float sq = Vec2.SqrDistance(p, _plotPos[i]);
                if (sq < bestPlot) { bestPlot = sq; plot = _plotPos[i]; }
            }
            if (bestPlot < float.MaxValue) return plot;

            // 3) Otherwise drift toward the nearest XP pickup cluster.
            hold = false;
            float bestPick = 64f;
            Vec2 pick = p;
            for (int i = 0; i < sim.Pickups.Length; i++)
            {
                if (!sim.Pickups[i].Active) continue;
                float sq = Vec2.SqrDistance(p, sim.Pickups[i].Pos);
                if (sq < bestPick) { bestPick = sq; pick = sim.Pickups[i].Pos; }
            }
            return pick;
        }

        /// <summary>Resolves pending choices: prefers gadget upgrades, then the most expensive affordable structure.</summary>
        public void ResolveChoices(GameSimulation sim)
        {
            if (sim.Phase == RunPhase.LevelUpChoice)
            {
                int pick = 0;
                for (int i = 0; i < sim.LevelUpOffers.Count; i++)
                    if (sim.LevelUpOffers[i].Kind == UpgradeOptionKind.GadgetLevel || sim.LevelUpOffers[i].Kind == UpgradeOptionKind.NewGadget) { pick = i; break; }
                sim.ChooseUpgrade(pick);
            }
            else if (sim.Phase == RunPhase.BuildChoice)
            {
                int pick = -1, bestCost = -1;
                for (int i = 0; i < sim.BuildOffers.Count; i++)
                {
                    int def = sim.BuildOffers[i];
                    int cost = sim.Config.structures[def].cost;
                    if (sim.CanAfford(def) && cost > bestCost) { bestCost = cost; pick = i; }
                }
                sim.ChooseStructure(pick);
            }
        }
    }
}
