using UnityEngine;

namespace Vamp.Game
{
    /// <summary>Ensures a <see cref="GameRoot"/> exists after the first scene loads (the game runs from any scene).</summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (GameRoot.Instance != null) return;
            new GameObject("Brasslight").AddComponent<GameRoot>();
        }
    }
}
