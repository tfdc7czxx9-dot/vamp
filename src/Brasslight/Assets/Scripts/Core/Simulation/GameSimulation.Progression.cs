using System;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    public sealed partial class GameSimulation
    {
        /// <summary>Effective magnet radius.</summary>
        public float MagnetRadius => Config.player.magnetRadius * (1f + PassiveValue(PassiveKind.Magnet) + (Setup.Modifiers?.Magnet ?? 0f));

        private void SpawnPickup(PickupKind kind, Vec2 pos, float value)
        {
            for (int i = 0; i < Pickups.Length; i++)
            {
                if (Pickups[i].Active) continue;
                Pickups[i] = new Pickup { Active = true, Kind = kind, Pos = pos, Value = value };
                return;
            }
            // Pool full: grant immediately so nothing is lost.
            Collect(kind, value, pos);
        }

        private void UpdatePickups(float dt)
        {
            float magnetSq = MagnetRadius * MagnetRadius;
            float pickSq = Config.player.pickupRadius * Config.player.pickupRadius;
            for (int i = 0; i < Pickups.Length; i++)
            {
                ref var p = ref Pickups[i];
                if (!p.Active) continue;
                float sq = Vec2.SqrDistance(p.Pos, Player.Pos);
                if (sq <= pickSq)
                {
                    p.Active = false;
                    Collect(p.Kind, p.Value, p.Pos);
                    continue;
                }
                if (!p.Magnetized && sq <= magnetSq) p.Magnetized = true;
                if (p.Magnetized)
                {
                    Vec2 dir = (Player.Pos - p.Pos).Normalized;
                    p.Pos += dir * MathF.Max(12f, Config.player.speed * 2.5f) * dt;
                }
            }
        }

        private void Collect(PickupKind kind, float value, Vec2 pos)
        {
            switch (kind)
            {
                case PickupKind.Xp: AddXp((int)value); break;
                case PickupKind.Brass:
                    Player.Brass += (int)value;
                    Stats.BrassCollected += (int)value;
                    break;
                case PickupKind.Heal: Player.Hp = MathF.Min(Player.MaxHp, Player.Hp + value); break;
            }
            Emit(SimEventType.PickupCollected, pos, default, value, (int)kind);
        }

        /// <summary>Adds XP and queues level-ups.</summary>
        public void AddXp(int amount)
        {
            Player.Xp += amount;
            while (Player.Xp >= Player.XpToNext)
            {
                Player.Xp -= Player.XpToNext;
                Player.Level++;
                Player.XpToNext = XpForLevel(Player.Level);
                Player.PendingLevelUps++;
                Emit(SimEventType.LevelUp, Player.Pos, default, Player.Level);
            }
        }

        /// <summary>True if a gadget is at max level, not evolved, and its partner passive is owned.</summary>
        public bool CanEvolve(in OwnedGadget g)
        {
            var d = Config.gadgets[g.Def];
            if (g.Evolved || g.Level < Config.player.maxGadgetLevel || string.IsNullOrEmpty(d.evolveWithPassive)) return false;
            for (int i = 0; i < Config.passives.Length; i++)
                if (Config.passives[i].id == d.evolveWithPassive) return PassiveLevels[i] > 0;
            return false;
        }

        /// <summary>Level of an owned gadget, or 0.</summary>
        public int GadgetLevel(int def)
        {
            foreach (var g in Gadgets) if (g.Def == def) return g.Level;
            return 0;
        }

        private void OpenLevelUp()
        {
            BuildLevelUpOffers();
            Phase = RunPhase.LevelUpChoice;
        }

        private void BuildLevelUpOffers()
        {
            _candidates.Clear();
            LevelUpOffers.Clear();
            var p = Config.player;
            // Evolutions are always offered when available (guaranteed slot).
            for (int i = 0; i < Gadgets.Count && LevelUpOffers.Count < System.Math.Max(1, p.levelUpChoices); i++)
                if (CanEvolve(Gadgets[i])) LevelUpOffers.Add(new UpgradeOption { Kind = UpgradeOptionKind.Evolve, Index = Gadgets[i].Def, NextLevel = Gadgets[i].Level });
            for (int i = 0; i < Config.gadgets.Length; i++)
            {
                int lvl = GadgetLevel(i);
                if (lvl == 0 && Gadgets.Count < p.maxGadgets)
                    _candidates.Add(new UpgradeOption { Kind = UpgradeOptionKind.NewGadget, Index = i, NextLevel = 1 });
                else if (lvl > 0 && lvl < p.maxGadgetLevel)
                    _candidates.Add(new UpgradeOption { Kind = UpgradeOptionKind.GadgetLevel, Index = i, NextLevel = lvl + 1 });
            }
            for (int i = 0; i < Config.passives.Length; i++)
                if (PassiveLevels[i] < Config.passives[i].maxLevel)
                    _candidates.Add(new UpgradeOption { Kind = UpgradeOptionKind.Passive, Index = i, NextLevel = PassiveLevels[i] + 1 });

            int want = System.Math.Max(1, p.levelUpChoices);
            while (LevelUpOffers.Count < want && _candidates.Count > 0)
            {
                int k = Rng.Range(0, _candidates.Count);
                LevelUpOffers.Add(_candidates[k]);
                _candidates[k] = _candidates[_candidates.Count - 1];
                _candidates.RemoveAt(_candidates.Count - 1);
            }
            if (LevelUpOffers.Count == 0)
                LevelUpOffers.Add(new UpgradeOption { Kind = UpgradeOptionKind.Heal, Index = -1, NextLevel = 0 });
        }

        /// <summary>Applies a level-up offer. Returns false if not in a level-up choice or the index is invalid.</summary>
        public bool ChooseUpgrade(int offerIndex)
        {
            if (Phase != RunPhase.LevelUpChoice || offerIndex < 0 || offerIndex >= LevelUpOffers.Count) return false;
            var o = LevelUpOffers[offerIndex];
            switch (o.Kind)
            {
                case UpgradeOptionKind.NewGadget:
                    Gadgets.Add(new OwnedGadget { Def = o.Index, Level = 1 });
                    break;
                case UpgradeOptionKind.GadgetLevel:
                    for (int i = 0; i < Gadgets.Count; i++)
                    {
                        if (Gadgets[i].Def != o.Index) continue;
                        var g = Gadgets[i];
                        g.Level++;
                        Gadgets[i] = g;
                        break;
                    }
                    break;
                case UpgradeOptionKind.Passive:
                    PassiveLevels[o.Index]++;
                    break;
                case UpgradeOptionKind.Evolve:
                    for (int i = 0; i < Gadgets.Count; i++)
                    {
                        if (Gadgets[i].Def != o.Index) continue;
                        var g = Gadgets[i];
                        g.Evolved = true;
                        Gadgets[i] = g;
                        Stats.Evolutions++;
                        Emit(SimEventType.Evolved, Player.Pos, default, 0f, o.Index);
                        break;
                    }
                    break;
                case UpgradeOptionKind.Heal:
                    Player.Hp = MathF.Min(Player.MaxHp, Player.Hp + Config.player.fallbackHeal);
                    break;
            }
            Player.PendingLevelUps--;
            if (Player.PendingLevelUps > 0) BuildLevelUpOffers();
            else Phase = RunPhase.Playing;
            return true;
        }
    }
}
