using System.Collections.Generic;
using NUnit.Framework;
using Vamp.Core.Meta;

namespace Vamp.Tests
{
    public class AchievementRulesTest
    {
        [Test]
        public void test_lifetime_kill_thresholds_unlock_once()
        {
            var c = TestHelpers.Config();
            var save = new SaveData { totalKills = 1500 };
            var list = new List<string>();
            AchievementRules.EvaluateLifetime(c, save, list);
            CollectionAssert.Contains(list, "ACH_KILLS_100");
            CollectionAssert.Contains(list, "ACH_KILLS_1K");
            CollectionAssert.DoesNotContain(list, "ACH_KILLS_10K");
            list.Clear();
            AchievementRules.EvaluateLifetime(c, save, list);
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void test_world_win_unlocks_world_achievement()
        {
            var c = TestHelpers.Config();
            var save = new SaveData();
            save.wonWorlds.Add("hollow_earth");
            var list = new List<string>();
            AchievementRules.EvaluateLifetime(c, save, list);
            CollectionAssert.Contains(list, "ACH_WORLD_HOLLOW_EARTH");
            CollectionAssert.DoesNotContain(list, "ACH_WORLDS_ALL");
        }

        [Test]
        public void test_full_crew_unlocks_during_run()
        {
            var c = TestHelpers.Config();
            c.recruit.baseCrewCapacity = 1;
            var sim = TestHelpers.NewRun(c);
            TestHelpers.KillEliteAt(sim, new Core.Math.Vec2(1f, 0f));
            TestHelpers.RunFor(sim, sim.RecruitSeconds + 0.1f);
            var save = new SaveData();
            var list = new List<string>();
            AchievementRules.EvaluateDuringRun(sim, save, list);
            CollectionAssert.Contains(list, "ACH_RECRUIT_FULL_CREW");
        }
    }
}
