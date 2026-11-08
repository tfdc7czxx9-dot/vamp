namespace Vamp.Platform
{
    /// <summary>
    /// Store/platform services. Gameplay never calls a store SDK directly (ADR-0001).
    /// Every implementation must degrade gracefully: if the platform is unavailable, the game keeps running.
    /// </summary>
    public interface IPlatformServices
    {
        /// <summary>Human-readable backend name (for logs and the options screen).</summary>
        string Name { get; }

        /// <summary>True while the platform's overlay (e.g. Steam overlay) is open; the game pauses.</summary>
        bool IsOverlayActive { get; }

        /// <summary>Initialises the backend. Returns false if unavailable (caller falls back to <see cref="NullPlatformServices"/>).</summary>
        bool Initialize();

        /// <summary>Called once per frame (callbacks).</summary>
        void Tick();

        /// <summary>Releases the backend.</summary>
        void Shutdown();

        /// <summary>Unlocks an achievement by API name (no-op if unsupported).</summary>
        void UnlockAchievement(string apiName);

        /// <summary>Reads the cloud copy of the save, if any.</summary>
        bool TryReadCloudSave(out string json);

        /// <summary>Writes the save to platform cloud storage (no-op if unsupported).</summary>
        void WriteCloudSave(string json);
    }
}
