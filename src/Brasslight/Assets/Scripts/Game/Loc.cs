using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vamp.Game
{
    /// <summary>
    /// Minimal string table (StreamingAssets/loc/&lt;lang&gt;.json). Missing keys fall back to English, then to the key.
    /// To be replaced by the Unity Localization package once the string set stabilises.
    /// </summary>
    public sealed class Loc
    {
        [Serializable] private class Entry { public string k; public string v; }
        [Serializable] private class Table { public Entry[] entries; }

        private readonly Dictionary<string, string> _active = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _fallback = new Dictionary<string, string>();

        /// <summary>Supported language codes.</summary>
        public static readonly string[] Languages = { "en", "de" };

        /// <summary>Active language code.</summary>
        public string Language { get; private set; } = "en";

        /// <summary>Loads tables from JSON text (null-safe).</summary>
        public void Load(string language, string activeJson, string englishJson)
        {
            Language = language;
            Fill(_fallback, englishJson);
            Fill(_active, activeJson);
        }

        private static void Fill(Dictionary<string, string> dict, string json)
        {
            dict.Clear();
            if (string.IsNullOrEmpty(json)) return;
            var t = JsonUtility.FromJson<Table>(json);
            if (t?.entries == null) return;
            foreach (var e in t.entries) if (!string.IsNullOrEmpty(e.k)) dict[e.k] = e.v;
        }

        /// <summary>Looks up a key.</summary>
        public string this[string key]
        {
            get
            {
                if (_active.TryGetValue(key, out var v)) return v;
                if (_fallback.TryGetValue(key, out v)) return v;
                return key;
            }
        }

        /// <summary>Looks up a key, returning <paramref name="fallback"/> if no table has it.</summary>
        public string Get(string key, string fallback)
        {
            if (_active.TryGetValue(key, out var v)) return v;
            if (_fallback.TryGetValue(key, out v)) return v;
            return fallback;
        }

        /// <summary>Looks up a key and formats it.</summary>
        public string F(string key, params object[] args) => string.Format(this[key], args);

        /// <summary>Picks a language: saved choice, else system language, else English.</summary>
        public static string Resolve(string saved)
        {
            if (!string.IsNullOrEmpty(saved) && Array.IndexOf(Languages, saved) >= 0) return saved;
            return Application.systemLanguage == SystemLanguage.German ? "de" : "en";
        }
    }
}
