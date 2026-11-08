using UnityEngine;

namespace Vamp.Platform
{
    /// <summary>Creates the best available platform backend for the current build.</summary>
    public static class PlatformFactory
    {
        /// <summary>Returns an initialised backend; falls back to offline mode if the store SDK is unavailable.</summary>
        public static IPlatformServices Create()
        {
#if BRASSLIGHT_STEAM && UNITY_STANDALONE
            var steam = new SteamPlatformServices();
            if (steam.Initialize()) return steam;
            Debug.LogWarning("[Platform] Steam unavailable, running offline.");
#endif
            var offline = new NullPlatformServices();
            offline.Initialize();
            return offline;
        }
    }
}
