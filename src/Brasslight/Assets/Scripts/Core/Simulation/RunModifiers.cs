using Vamp.Core.Config;

namespace Vamp.Core.Simulation
{
    /// <summary>Run-start modifiers from the chosen leader and purchased research.</summary>
    public class RunModifiers
    {
        /// <summary>Flat max HP bonus.</summary>
        public float MaxHpBonus;
        /// <summary>Damage bonus fraction.</summary>
        public float Might;
        /// <summary>Speed bonus fraction.</summary>
        public float Speed;
        /// <summary>Extra crew capacity.</summary>
        public int CrewBonus;
        /// <summary>Brass at run start.</summary>
        public int StartingBrass;
        /// <summary>Magnet radius bonus fraction.</summary>
        public float Magnet;
        /// <summary>Revives available.</summary>
        public int Revives;
        /// <summary>Build time reduction fraction.</summary>
        public float BuildTime;
        /// <summary>Recruit time reduction fraction.</summary>
        public float RecruitTime;

        /// <summary>
        /// Combines a leader and research levels. <paramref name="metaLevels"/> is aligned with
        /// <c>config.metaUpgrades</c> (missing entries count as 0).
        /// </summary>
        public static RunModifiers Create(BalanceConfig config, int leaderIndex, int[] metaLevels)
        {
            var m = new RunModifiers();
            if (leaderIndex >= 0 && leaderIndex < config.leaders.Length)
            {
                var l = config.leaders[leaderIndex];
                m.MaxHpBonus += l.maxHpBonus;
                m.Speed += l.speedBonus;
                m.BuildTime += l.buildTimeBonus;
                m.RecruitTime += l.recruitTimeBonus;
                m.Might += l.mightBonus;
                m.Magnet += l.magnetBonus;
                m.StartingBrass += l.startingBrass;
                m.CrewBonus += l.crewBonus;
            }
            for (int i = 0; i < config.metaUpgrades.Length; i++)
            {
                int lvl = metaLevels != null && i < metaLevels.Length ? metaLevels[i] : 0;
                if (lvl <= 0) continue;
                var u = config.metaUpgrades[i];
                lvl = System.Math.Min(lvl, u.maxLevel);
                float v = u.valuePerLevel * lvl;
                switch (u.Effect)
                {
                    case MetaEffectKind.MaxHp: m.MaxHpBonus += v; break;
                    case MetaEffectKind.Might: m.Might += v; break;
                    case MetaEffectKind.Speed: m.Speed += v; break;
                    case MetaEffectKind.CrewCapacity: m.CrewBonus += (int)v; break;
                    case MetaEffectKind.StartingBrass: m.StartingBrass += (int)v; break;
                    case MetaEffectKind.Magnet: m.Magnet += v; break;
                    case MetaEffectKind.Revival: m.Revives += (int)v; break;
                }
            }
            return m;
        }
    }
}
