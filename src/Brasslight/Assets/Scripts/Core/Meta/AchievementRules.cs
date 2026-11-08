using System.Collections.Generic;
using Vamp.Core.Config;
using Vamp.Core.Simulation;

namespace Vamp.Core.Meta
{
    /// <summary>
    /// Evaluates achievement conditions (see production/platforms/steam/achievements.md).
    /// Newly earned API names are appended to the save and returned so the platform layer can report them.
    /// Achievements that depend on systems not built yet (evolutions, bestiary discovery) are not evaluated here.
    /// </summary>
    public static class AchievementRules
    {
        /// <summary>Checks conditions that can trigger during a run (call periodically).</summary>
        public static void EvaluateDuringRun(GameSimulation sim, SaveData save, List<string> newlyUnlocked)
        {
            if (sim.CrewCapacity > 0 && sim.CrewCount >= sim.CrewCapacity) Unlock("ACH_RECRUIT_FULL_CREW", save, newlyUnlocked);
            if (sim.Stats.Kills >= 1000) Unlock("ACH_RUN_KILLS_1000", save, newlyUnlocked);
            int standing = 0;
            foreach (var s in sim.Structures)
            {
                if (!s.Active) continue;
                standing++;
                if (s.Level >= sim.Config.build.maxStructureLevel) Unlock("ACH_STRUCTURE_MAX", save, newlyUnlocked);
            }
            if (standing >= 10 && sim.Config.leaders[sim.Setup.Leader].id == "ines_varga") Unlock("ACH_LEADER_ENGINEER", save, newlyUnlocked);
            if (sim.Time >= sim.Config.run.bossTimeSeconds && !sim.IsOver)
            {
                Unlock("ACH_RUN_SURVIVE_15", save, newlyUnlocked);
                if (standing >= 8) Unlock("ACH_FORTRESS_RUN", save, newlyUnlocked);
            }
            if (sim.Stats.SynergyTriggered) Unlock("ACH_SYNERGY_EVOLUTION", save, newlyUnlocked);
            if (sim.Stats.Evolutions > 0) Unlock("ACH_EVOLUTION_FIRST", save, newlyUnlocked);
            if (sim.Phase == RunPhase.Victory && sim.CrewCount >= sim.CrewCapacity && sim.CrewCapacity > 0)
                Unlock("ACH_BOSS_FULL_CREW", save, newlyUnlocked);
        }

        /// <summary>Checks lifetime conditions (call after a run is recorded and after purchases/unlocks).</summary>
        public static void EvaluateLifetime(BalanceConfig config, SaveData save, List<string> newlyUnlocked)
        {
            if (save.totalKills >= 100) Unlock("ACH_KILLS_100", save, newlyUnlocked);
            if (save.totalKills >= 1000) Unlock("ACH_KILLS_1K", save, newlyUnlocked);
            if (save.totalKills >= 10000) Unlock("ACH_KILLS_10K", save, newlyUnlocked);
            if (save.totalKills >= 100000) Unlock("ACH_KILLS_100K", save, newlyUnlocked);
            if (save.totalRecruits >= 1) Unlock("ACH_RECRUIT_FIRST", save, newlyUnlocked);
            if (save.totalRecruits >= 25) Unlock("ACH_RECRUIT_25", save, newlyUnlocked);
            if (save.totalScraps >= 10) Unlock("ACH_SCRAP_10", save, newlyUnlocked);
            if (save.totalStructures >= 1) Unlock("ACH_BUILD_FIRST", save, newlyUnlocked);
            if (save.totalRuns >= 50) Unlock("ACH_RUNS_50", save, newlyUnlocked);
            if (save.unlockedLeaders.Count >= 1) Unlock("ACH_LEADER_FIRST_UNLOCK", save, newlyUnlocked);
            if (save.upgrades.Count > 0) Unlock("ACH_RESEARCH_FIRST", save, newlyUnlocked);

            int lockedLeaders = 0;
            foreach (var l in config.leaders) if (l.unlockCost > 0) lockedLeaders++;
            if (lockedLeaders > 0 && save.unlockedLeaders.Count >= lockedLeaders) Unlock("ACH_LEADERS_UNLOCK_ALL", save, newlyUnlocked);
            if (save.seenSpecies.Count >= 10) Unlock("ACH_BESTIARY_10", save, newlyUnlocked);
            if (config.enemies.Length > 0 && save.seenSpecies.Count >= config.enemies.Length) Unlock("ACH_BESTIARY_ALL", save, newlyUnlocked);

            int won = 0;
            foreach (var w in config.worlds)
            {
                if (!save.wonWorlds.Contains(w.id)) continue;
                won++;
                if (!string.IsNullOrEmpty(w.achievement)) Unlock(w.achievement, save, newlyUnlocked);
            }
            if (config.worlds.Length > 0 && won == config.worlds.Length) Unlock("ACH_WORLDS_ALL", save, newlyUnlocked);

            int recruitable = 0, recruited = 0;
            foreach (var e in config.enemies)
            {
                if (!e.eliteEligible) continue;
                recruitable++;
                if (save.recruitedSpecies.Contains(e.id)) recruited++;
            }
            if (recruitable > 0 && recruited == recruitable) Unlock("ACH_BESTIARY_RECRUIT_ALL", save, newlyUnlocked);
        }

        private static void Unlock(string id, SaveData save, List<string> newlyUnlocked)
        {
            if (save.achievements.Contains(id)) return;
            save.achievements.Add(id);
            newlyUnlocked?.Add(id);
        }
    }
}
