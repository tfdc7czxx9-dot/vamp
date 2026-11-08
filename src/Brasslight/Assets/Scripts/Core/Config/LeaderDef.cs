using System;

namespace Vamp.Core.Config
{
    /// <summary>Data for one playable expedition leader (original character).</summary>
    [Serializable]
    public class LeaderDef
    {
        /// <summary>Stable id.</summary>
        public string id;
        /// <summary>Display name.</summary>
        public string displayName;
        /// <summary>Short description of the leader's passive.</summary>
        public string description;
        /// <summary>Gadget id the leader starts with.</summary>
        public string startingGadget;
        /// <summary>Move speed bonus fraction.</summary>
        public float speedBonus;
        /// <summary>Build time reduction fraction.</summary>
        public float buildTimeBonus;
        /// <summary>Recruit time reduction fraction.</summary>
        public float recruitTimeBonus;
        /// <summary>Flat max HP bonus.</summary>
        public float maxHpBonus;
        /// <summary>Damage bonus fraction.</summary>
        public float mightBonus;
        /// <summary>Magnet radius bonus fraction.</summary>
        public float magnetBonus;
        /// <summary>Brass at run start.</summary>
        public int startingBrass;
        /// <summary>Extra crew capacity.</summary>
        public int crewBonus;
        /// <summary>Research points needed to unlock (0 = available from the start).</summary>
        public int unlockCost;
        /// <summary>Sprite index in the hero atlas.</summary>
        public int sprite;
    }
}
