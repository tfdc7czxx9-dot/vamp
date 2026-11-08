namespace Vamp.Platform
{
    /// <summary>Offline/editor backend: no achievements reporting, no cloud. Always available.</summary>
    public sealed class NullPlatformServices : IPlatformServices
    {
        /// <inheritdoc/>
        public string Name => "Offline";
        /// <inheritdoc/>
        public bool IsOverlayActive => false;
        /// <inheritdoc/>
        public bool Initialize() => true;
        /// <inheritdoc/>
        public void Tick() { }
        /// <inheritdoc/>
        public void Shutdown() { }
        /// <inheritdoc/>
        public void UnlockAchievement(string apiName) { }
        /// <inheritdoc/>
        public bool TryReadCloudSave(out string json) { json = null; return false; }
        /// <inheritdoc/>
        public void WriteCloudSave(string json) { }
    }
}
