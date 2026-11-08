// Compiled only when Steamworks.NET is installed and BRASSLIGHT_STEAM is defined for Standalone builds
// (Player Settings > Scripting Define Symbols). See production/platforms/steam/checklist.md.
#if BRASSLIGHT_STEAM && UNITY_STANDALONE
using System.Text;
using Steamworks;
using UnityEngine;

namespace Vamp.Platform
{
    /// <summary>Steamworks.NET backend: achievements, Steam Cloud (Remote Storage), overlay pause.</summary>
    public sealed class SteamPlatformServices : IPlatformServices
    {
        private const string CloudFile = "brasslight_save.json";
        private Callback<GameOverlayActivated_t> _overlayCallback;
        private bool _initialized;

        /// <inheritdoc/>
        public string Name => "Steam";
        /// <inheritdoc/>
        public bool IsOverlayActive { get; private set; }

        /// <inheritdoc/>
        public bool Initialize()
        {
            try
            {
                if (!Packsize.Test() || !DllCheck.Test()) return false;
                _initialized = SteamAPI.Init();
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogWarning("[Steam] steam_api not found: " + e.Message);
                _initialized = false;
            }
            if (!_initialized) return false;
            _overlayCallback = Callback<GameOverlayActivated_t>.Create(OnOverlay);
            return true;
        }

        private void OnOverlay(GameOverlayActivated_t data) => IsOverlayActive = data.m_bActive != 0;

        /// <inheritdoc/>
        public void Tick()
        {
            if (_initialized) SteamAPI.RunCallbacks();
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_initialized) return;
            SteamAPI.Shutdown();
            _initialized = false;
        }

        /// <inheritdoc/>
        public void UnlockAchievement(string apiName)
        {
            if (!_initialized) return;
            if (SteamUserStats.SetAchievement(apiName)) SteamUserStats.StoreStats();
        }

        /// <inheritdoc/>
        public bool TryReadCloudSave(out string json)
        {
            json = null;
            if (!_initialized || !SteamRemoteStorage.FileExists(CloudFile)) return false;
            int size = SteamRemoteStorage.GetFileSize(CloudFile);
            if (size <= 0) return false;
            var data = new byte[size];
            int read = SteamRemoteStorage.FileRead(CloudFile, data, size);
            if (read <= 0) return false;
            json = Encoding.UTF8.GetString(data, 0, read);
            return true;
        }

        /// <inheritdoc/>
        public void WriteCloudSave(string json)
        {
            if (!_initialized || string.IsNullOrEmpty(json)) return;
            var data = Encoding.UTF8.GetBytes(json);
            SteamRemoteStorage.FileWrite(CloudFile, data, data.Length);
        }
    }
}
#endif
