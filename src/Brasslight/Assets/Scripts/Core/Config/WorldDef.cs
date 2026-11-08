using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one world (map / voyage).</summary>
    [Serializable]
    public class WorldDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Public-domain source credited in the world intro.</summary>
        public string source;
        /// <summary>Enemy ids spawning in this world.</summary>
        public string[] enemyIds = Array.Empty<string>();
        /// <summary>Boss enemy id.</summary>
        public string bossId;
        /// <summary>World that must be won to unlock this one (empty = unlocked).</summary>
        public string unlockedByWorld;
        /// <summary>Achievement API name unlocked by defeating this world's boss.</summary>
        public string achievement;
        /// <summary>Ground colour (hex RGB) used by the presentation layer.</summary>
        public string groundColor = "#2E4A4A";
        /// <summary>Accent / light colour (hex RGB).</summary>
        public string accentColor = "#FFB040";
        /// <summary>Fog colour (hex RGB).</summary>
        public string fogColor = "#10181C";
    }
}
