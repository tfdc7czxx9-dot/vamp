using System;

namespace Vamp.Core.Meta
{
    /// <summary>Saved level of one research upgrade.</summary>
    [Serializable]
    public class UpgradeLevel
    {
        /// <summary>Upgrade id.</summary>
        public string id;
        /// <summary>Purchased level.</summary>
        public int level;
    }
}
