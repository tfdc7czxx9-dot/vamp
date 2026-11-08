using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Vamp.Game
{
    /// <summary>Reads text files from StreamingAssets on every platform (Android requires UnityWebRequest).</summary>
    public static class StreamingFile
    {
        /// <summary>Coroutine: reads <paramref name="relativePath"/> and calls <paramref name="done"/> with the text or null.</summary>
        public static IEnumerator Read(string relativePath, Action<string> done)
        {
            string path = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (!path.Contains("://")) path = "file://" + path;
            using (var req = UnityWebRequest.Get(path))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[StreamingFile] {relativePath}: {req.error}");
                    done(null);
                    yield break;
                }
                done(req.downloadHandler.text);
            }
        }
    }
}
