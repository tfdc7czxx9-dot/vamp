using System;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    public sealed partial class GameSimulation
    {
        /// <summary>Spawn rate r(m) in enemies per second for minute m.</summary>
        public float SpawnRate(float minutes)
        {
            var r = Config.run;
            return r.spawnBase + r.spawnLinear * minutes + r.spawnQuadratic * minutes * minutes;
        }

        /// <summary>Enemy HP multiplier for minute m.</summary>
        public float HpMultiplier(float minutes) => 1f + Config.run.hpPerMinute * minutes;

        private int NormalEnemyCount()
        {
            int n = 0;
            for (int i = 0; i < Enemies.Length; i++)
                if (Enemies[i].Active && !Enemies[i].Elite && !Enemies[i].Boss) n++;
            return n;
        }

        private void UpdateSpawning(float dt)
        {
            float m = Time / 60f;
            _spawnAccumulator += SpawnRate(m) * dt;
            if (_spawnAccumulator >= 1f)
            {
                int normals = NormalEnemyCount();
                while (_spawnAccumulator >= 1f)
                {
                    _spawnAccumulator -= 1f;
                    if (normals >= EnemyCap) continue;
                    int def = PickEnemyType(m, false);
                    if (def >= 0 && SpawnEnemy(def, SpawnPoint(), false, false) >= 0) normals++;
                }
            }

            if (Time >= _nextEliteAt)
            {
                _nextEliteAt += Config.run.eliteInterval;
                int def = PickEnemyType(m, true);
                if (def >= 0)
                {
                    int idx = SpawnEnemy(def, SpawnPoint(), true, false);
                    if (idx >= 0) Emit(SimEventType.EliteSpawned, Enemies[idx].Pos, default, 0f, def);
                }
            }

            if (!_bossSpawned && Time >= Config.run.bossTimeSeconds)
            {
                _bossSpawned = true;
                int def = Config.EnemyIndex(World.bossId);
                int idx = SpawnEnemy(def, SpawnPoint(), false, true);
                if (idx >= 0) Emit(SimEventType.BossSpawned, Enemies[idx].Pos, default, 0f, def);
            }
        }

        private Vec2 SpawnPoint() => Player.Pos + Rng.OnRing(Config.run.spawnRingMin, Config.run.spawnRingMax);

        /// <summary>Picks a weighted enemy type available at minute m; elites only from eligible types.</summary>
        private int PickEnemyType(float m, bool elite)
        {
            float total = 0f;
            int fallback = -1;
            for (int pass = 0; pass < 2; pass++)
            {
                float roll = pass == 1 ? Rng.NextFloat() * total : 0f;
                foreach (var id in World.enemyIds)
                {
                    int i = Config.EnemyIndex(id);
                    var e = Config.enemies[i];
                    if (e.isBoss || (elite && !e.eliteEligible)) continue;
                    if (fallback < 0) fallback = i;
                    bool inWindow = e.minMinute <= m && (e.maxMinute <= 0f || m <= e.maxMinute);
                    if (!inWindow) continue;
                    float w = elite ? 1f : e.weight;
                    if (pass == 0) total += w;
                    else
                    {
                        roll -= w;
                        if (roll <= 0f) return i;
                    }
                }
                if (pass == 0 && total <= 0f) return fallback;
            }
            return fallback;
        }

        /// <summary>Spawns an enemy; returns its pool index or -1 if the pool is full.</summary>
        public int SpawnEnemy(int def, Vec2 pos, bool elite, bool boss)
        {
            if (def < 0 || _freeEnemies.Count == 0) return -1;
            int idx = _freeEnemies.Pop();
            var d = Config.enemies[def];
            float m = Time / 60f;
            float hp = boss ? d.hp : d.hp * HpMultiplier(m) * (elite ? Config.run.eliteHpMultiplier : 1f);
            Enemies[idx] = new Enemy
            {
                Active = true,
                Def = def,
                Pos = pos,
                Facing = new Vec2(1f, 0f),
                Hp = hp,
                MaxHp = hp,
                Speed = d.speed,
                Radius = d.radius * (elite ? Config.run.eliteSizeMultiplier : 1f),
                Damage = d.damage * (elite ? 1.5f : 1f),
                Xp = elite ? (int)MathF.Round(d.xp * Config.run.eliteXpMultiplier) : d.xp,
                Elite = elite,
                Boss = boss,
            };
            EnemyCount++;
            return idx;
        }

        private void CollectActiveLures()
        {
            _activeLureCount = 0;
            for (int i = 0; i < Structures.Length; i++)
            {
                ref var s = ref Structures[i];
                if (!s.Active) continue;
                var d = Config.structures[s.Def];
                if (d.Kind == StructureKind.LureBeacon && s.Cycle < d.activeSeconds) _activeLures[_activeLureCount++] = i;
            }
        }

        private void UpdateEnemies(float dt)
        {
            float despawnSq = Config.run.despawnDistance * Config.run.despawnDistance;
            float playerR = Config.player.radius;
            for (int i = 0; i < Enemies.Length; i++)
            {
                ref var e = ref Enemies[i];
                if (!e.Active) continue;
                if (e.Flash > 0f) e.Flash -= dt;
                if (e.Slow > 0f) e.Slow -= dt;
                if (e.BladeCooldown > 0f) e.BladeCooldown -= dt;

                // Relocate stragglers ahead of the player instead of letting them trail forever.
                if (!e.Boss && Vec2.SqrDistance(e.Pos, Player.Pos) > despawnSq)
                {
                    var ahead = Player.Facing * Config.run.spawnRingMin;
                    e.Pos = Player.Pos + ahead + Rng.OnRing(0f, 4f);
                    continue;
                }

                Vec2 target = Player.Pos;
                for (int l = 0; l < _activeLureCount; l++)
                {
                    ref var lure = ref Structures[_activeLures[l]];
                    float range = Config.structures[lure.Def].range;
                    if (Vec2.SqrDistance(e.Pos, lure.Pos) <= range * range) { target = lure.Pos; break; }
                }

                Vec2 dir = (target - e.Pos).Normalized;
                Vec2 push = Separation(i, ref e);
                float speed = e.Speed * (e.Slow > 0f ? 0.6f : 1f);
                Vec2 vel = dir * speed + push;
                e.Pos += vel * dt;
                if (dir.SqrMagnitude > 0f) e.Facing = dir;

                // Contact with the player.
                float rr = e.Radius + playerR;
                if (Vec2.SqrDistance(e.Pos, Player.Pos) < rr * rr) HitPlayer(e.Damage, e.Pos);

                // Contact with companions (damage over time).
                for (int c = 0; c < Companions.Length; c++)
                {
                    ref var comp = ref Companions[c];
                    if (!comp.Active) continue;
                    float cr = e.Radius + comp.Radius;
                    if (Vec2.SqrDistance(e.Pos, comp.Pos) < cr * cr) comp.Hp -= e.Damage * dt;
                }

                // Contact with structures.
                float sr = Config.build.structureRadius;
                for (int s = 0; s < Structures.Length; s++)
                {
                    ref var st = ref Structures[s];
                    if (!st.Active) continue;
                    float r = e.Radius + sr;
                    Vec2 delta = e.Pos - st.Pos;
                    float dsq = delta.SqrMagnitude;
                    if (dsq >= r * r) continue;
                    var sd = Config.structures[st.Def];
                    if (st.MaxHp > 0f) st.Hp -= e.Damage * Config.build.structureDamageFactor * dt;
                    if (sd.Kind == StructureKind.Barricade)
                    {
                        float d = MathF.Sqrt(dsq);
                        Vec2 n = d > 1e-4f ? delta / d : new Vec2(1f, 0f);
                        e.Pos = st.Pos + n * r;
                    }
                }
            }
        }

        private Vec2 Separation(int self, ref Enemy e)
        {
            int n = _grid.Query(e.Pos, e.Radius * 2f, _query);
            Vec2 push = Vec2.Zero;
            int checkedCount = 0;
            for (int k = 0; k < n && checkedCount < 8; k++)
            {
                int j = _query[k];
                if (j == self) continue;
                ref var o = ref Enemies[j];
                if (!o.Active) continue;
                checkedCount++;
                Vec2 d = e.Pos - o.Pos;
                float min = e.Radius + o.Radius;
                float sq = d.SqrMagnitude;
                if (sq >= min * min || sq < 1e-8f) continue;
                float dist = MathF.Sqrt(sq);
                push += d / dist * ((min - dist) / min) * 3f;
            }
            return push;
        }

        private void HitPlayer(float damage, Vec2 from)
        {
            if (Player.Invuln > 0f || IsOver) return;
            Player.Hp -= damage;
            Player.Invuln = Config.player.invulnSeconds;
            Emit(SimEventType.PlayerHit, Player.Pos, from, damage);
            if (Player.Hp > 0f) return;
            if (Player.RevivesLeft > 0)
            {
                Player.RevivesLeft--;
                Player.Hp = Player.MaxHp * 0.5f;
                Player.Invuln = 2f;
                // Clear the immediate surroundings so the revive is meaningful.
                for (int i = 0; i < Enemies.Length; i++)
                {
                    ref var e = ref Enemies[i];
                    if (e.Active && !e.Boss && Vec2.SqrDistance(e.Pos, Player.Pos) < 16f) KillEnemy(i, false);
                }
                Emit(SimEventType.PlayerRevived, Player.Pos);
                return;
            }
            Player.Hp = 0f;
            Phase = RunPhase.Defeat;
        }

        /// <summary>Deals damage to an enemy. Returns true if it died.</summary>
        public bool DamageEnemy(int index, float damage)
        {
            ref var e = ref Enemies[index];
            if (!e.Active || damage <= 0f) return false;
            e.Hp -= damage;
            e.Flash = 0.1f;
            Stats.DamageDealt += damage;
            Emit(SimEventType.EnemyHit, e.Pos, default, damage, index);
            if (e.Hp > 0f) return false;
            KillEnemy(index, true);
            return true;
        }

        private void KillEnemy(int index, bool drops)
        {
            ref var e = ref Enemies[index];
            if (!e.Active) return;
            e.Active = false;
            EnemyCount--;
            _freeEnemies.Push(index);
            Stats.Kills++;
            Emit(SimEventType.EnemyKilled, e.Pos, default, e.Elite ? 1f : 0f, e.Def);
            if (!drops) return;

            SpawnPickup(PickupKind.Xp, e.Pos, e.Xp);
            if (e.Boss)
            {
                Stats.BossDefeated = true;
                Phase = RunPhase.Victory;
                return;
            }
            if (e.Elite)
            {
                Stats.EliteKills++;
                SpawnPickup(PickupKind.Brass, e.Pos + new Vec2(0.4f, 0f), Config.run.eliteBrassDrop);
                SpawnRemnant(e);
                return;
            }
            if (Rng.Chance(Config.run.brassDropChance)) SpawnPickup(PickupKind.Brass, e.Pos + new Vec2(0.25f, 0.1f), 1);
            if (Rng.Chance(Config.run.healDropChance)) SpawnPickup(PickupKind.Heal, e.Pos + new Vec2(-0.25f, 0.1f), Config.run.healAmount);
        }

        /// <summary>Finds the nearest active enemy within range of a point, or -1.</summary>
        public int NearestEnemy(Vec2 from, float range, int exclude = -1)
        {
            int n = _grid.Query(from, range, _query);
            int best = -1;
            float bestSq = range * range;
            for (int k = 0; k < n; k++)
            {
                int i = _query[k];
                if (i == exclude) continue;
                ref var e = ref Enemies[i];
                if (!e.Active) continue;
                float sq = Vec2.SqrDistance(from, e.Pos);
                if (sq <= bestSq) { bestSq = sq; best = i; }
            }
            return best;
        }

        /// <summary>Damages every enemy within a radius. Returns the number hit.</summary>
        private int DamageArea(Vec2 center, float radius, float damage, float slow = 0f)
        {
            int n = _grid.Query(center, radius + 1f, _query);
            int hits = 0;
            for (int k = 0; k < n; k++)
            {
                int i = _query[k];
                ref var e = ref Enemies[i];
                if (!e.Active) continue;
                float r = radius + e.Radius;
                if (Vec2.SqrDistance(center, e.Pos) > r * r) continue;
                if (slow > 0f) e.Slow = slow;
                DamageEnemy(i, damage);
                hits++;
            }
            return hits;
        }

        /// <summary>Chain damage starting at the nearest enemy; emits lightning segments.</summary>
        private void ChainLightning(Vec2 origin, float range, float jump, int count, float damage)
        {
            int target = NearestEnemy(origin, range);
            Vec2 from = origin;
            int hits = 0;
            count = System.Math.Min(count, _chainHit.Length);
            while (target >= 0 && hits < count)
            {
                _chainHit[hits++] = target;
                Vec2 to = Enemies[target].Pos;
                Emit(SimEventType.Lightning, from, to, damage);
                DamageEnemy(target, damage);
                from = to;
                target = NearestUnhit(from, jump, hits);
            }
        }

        private int NearestUnhit(Vec2 from, float range, int hitCount)
        {
            int n = _grid.Query(from, range, _query);
            int best = -1;
            float bestSq = range * range;
            for (int k = 0; k < n; k++)
            {
                int i = _query[k];
                ref var e = ref Enemies[i];
                if (!e.Active) continue;
                bool already = false;
                for (int h = 0; h < hitCount; h++) if (_chainHit[h] == i) { already = true; break; }
                if (already) continue;
                float sq = Vec2.SqrDistance(from, e.Pos);
                if (sq <= bestSq) { bestSq = sq; best = i; }
            }
            return best;
        }
    }
}
