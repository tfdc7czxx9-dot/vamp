using System;

namespace Vamp.Core.Meta
{
    /// <summary>Player options persisted with the save.</summary>
    [Serializable]
    public class SettingsData
    {
        /// <summary>Music volume 0..1.</summary>
        public float musicVolume = 0.8f;
        /// <summary>SFX volume 0..1.</summary>
        public float sfxVolume = 0.8f;
        /// <summary>Quality tier (-1 = automatic, 0 Low .. 3 Ultra).</summary>
        public int qualityTier = -1;
        /// <summary>Screen shake enabled.</summary>
        public bool screenShake = true;
        /// <summary>Depth of field / tilt-shift enabled.</summary>
        public bool depthOfField = true;
        /// <summary>High-contrast outlines on gameplay sprites.</summary>
        public bool highContrast;
        /// <summary>Show damage numbers.</summary>
        public bool damageNumbers = true;
        /// <summary>UI language code ("" = system).</summary>
        public string language = "";
        /// <summary>Touch joystick on the left (true) or anywhere (false).</summary>
        public bool fixedJoystick;
    }
}
