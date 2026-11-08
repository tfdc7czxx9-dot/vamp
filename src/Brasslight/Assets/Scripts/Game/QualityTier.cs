using UnityEngine;

namespace Vamp.Game
{
    /// <summary>Quality tiers of ADR-0002 (Low / Medium / High / Ultra).</summary>
    public enum QualityTier
    {
        /// <summary>Old phones.</summary>
        Low,
        /// <summary>Mid phones, Steam Deck.</summary>
        Medium,
        /// <summary>PC.</summary>
        High,
        /// <summary>Strong PC.</summary>
        Ultra,
    }

    /// <summary>Picks a default tier from the hardware.</summary>
    public static class QualityTierDetector
    {
        /// <summary>Automatic tier.</summary>
        public static QualityTier Detect()
        {
            int mem = SystemInfo.systemMemorySize;
            int vram = SystemInfo.graphicsMemorySize;
            if (Application.isMobilePlatform)
                return mem >= 6000 ? QualityTier.Medium : QualityTier.Low;
            if (vram >= 6000 && mem >= 16000) return QualityTier.Ultra;
            if (vram >= 2000) return QualityTier.High;
            return QualityTier.Medium; // Steam Deck and integrated GPUs
        }

        /// <summary>Resolves the saved setting (-1 = automatic).</summary>
        public static QualityTier Resolve(int saved) => saved < 0 || saved > 3 ? Detect() : (QualityTier)saved;
    }
}
