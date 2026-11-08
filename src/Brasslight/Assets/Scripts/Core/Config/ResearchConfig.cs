using System;

namespace Vamp.Core.Config
{
    /// <summary>Meta currency earned at the end of a run.</summary>
    [Serializable]
    public class ResearchConfig
    {
        /// <summary>Kills needed for one research point.</summary>
        public int killsPerPoint = 20;
        /// <summary>Research points per full minute survived.</summary>
        public int pointsPerMinute = 2;
        /// <summary>Bonus for defeating the boss.</summary>
        public int victoryBonus = 50;
    }
}
