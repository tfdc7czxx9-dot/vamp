using System.Collections.Generic;
using UnityEngine;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    /// <summary>
    /// Placeholder audio synthesised at startup (no external assets): short SFX and a looping
    /// original music-box theme. Final audio follows design/gdd/sound-bible.md.
    /// Voice-limited so 500 enemies never flood the mixer.
    /// </summary>
    public sealed class SoundBank
    {
        private const int Rate = 44100;
        private readonly Dictionary<SimEventType, AudioClip> _clips = new Dictionary<SimEventType, AudioClip>();
        private readonly Dictionary<SimEventType, float> _cooldown = new Dictionary<SimEventType, float>();
        private readonly AudioSource[] _voices;
        private readonly AudioSource _music;
        private int _next;
        private float _sfxVolume = 0.8f;

        /// <summary>Creates voices and synthesises all clips.</summary>
        public SoundBank(Transform parent)
        {
            var go = new GameObject("Audio");
            go.transform.SetParent(parent, false);
            _voices = new AudioSource[12];
            for (int i = 0; i < _voices.Length; i++)
            {
                _voices[i] = go.AddComponent<AudioSource>();
                _voices[i].playOnAwake = false;
                _voices[i].spatialBlend = 0f;
            }
            _music = go.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;

            _clips[SimEventType.GadgetFired] = Tone("shot", 0.07f, 520f, 300f, Wave.Square, 0.25f);
            _clips[SimEventType.EnemyHit] = Noise("hit", 0.04f, 0.18f);
            _clips[SimEventType.EnemyKilled] = Tone("pop", 0.09f, 300f, 120f, Wave.Triangle, 0.35f);
            _clips[SimEventType.Lightning] = Zap("zap", 0.18f);
            _clips[SimEventType.Explosion] = Boom("boom", 0.45f);
            _clips[SimEventType.PickupCollected] = Tone("pickup", 0.06f, 880f, 1320f, Wave.Sine, 0.25f);
            _clips[SimEventType.LevelUp] = Arp("levelup", new[] { 523f, 659f, 784f, 1046f }, 0.08f);
            _clips[SimEventType.Recruited] = Arp("recruit", new[] { 392f, 494f, 587f, 784f, 988f }, 0.09f);
            _clips[SimEventType.StructureBuilt] = Clank("build");
            _clips[SimEventType.StructureUpgraded] = Arp("upgrade", new[] { 587f, 880f }, 0.08f);
            _clips[SimEventType.PlayerHit] = Tone("hurt", 0.18f, 180f, 90f, Wave.Square, 0.4f);
            _clips[SimEventType.EliteSpawned] = Arp("elite", new[] { 220f, 207f, 196f }, 0.14f);
            _clips[SimEventType.BossSpawned] = Arp("boss", new[] { 110f, 104f, 98f, 92f }, 0.3f);
            _clips[SimEventType.NotEnoughBrass] = Tone("deny", 0.15f, 200f, 160f, Wave.Square, 0.3f);
            _clips[SimEventType.CompanionLost] = Arp("lost", new[] { 392f, 330f, 262f }, 0.12f);
            _clips[SimEventType.Evolved] = Arp("evolve", new[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.07f);
            _clips[SimEventType.Synergy] = Zap("synergy", 0.3f);
            _clips[SimEventType.EnemyShot] = Tone("enemyshot", 0.08f, 700f, 500f, Wave.Triangle, 0.12f);
            _clips[SimEventType.BossCharge] = Boom("charge", 0.35f);
            _music.clip = Theme();
        }

        /// <summary>Sets volumes (0..1).</summary>
        public void SetVolumes(float music, float sfx)
        {
            _music.volume = music * 0.45f;
            _sfxVolume = sfx;
        }

        /// <summary>Starts or stops the music loop.</summary>
        public void Music(bool on, float pitch = 1f)
        {
            _music.pitch = pitch;
            if (on && !_music.isPlaying) _music.Play();
            if (!on && _music.isPlaying) _music.Stop();
        }

        /// <summary>Plays the sound for an event, respecting a per-type cooldown.</summary>
        public void Play(SimEventType type)
        {
            if (!_clips.TryGetValue(type, out var clip)) return;
            float now = Time.unscaledTime;
            float minGap = type == SimEventType.EnemyHit || type == SimEventType.PickupCollected || type == SimEventType.EnemyKilled ? 0.05f : 0.02f;
            if (_cooldown.TryGetValue(type, out float last) && now - last < minGap) return;
            _cooldown[type] = now;
            var v = _voices[_next];
            _next = (_next + 1) % _voices.Length;
            v.pitch = Random.Range(0.93f, 1.07f);
            v.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>UI click.</summary>
        public void Click() => Play(SimEventType.PickupCollected);

        private enum Wave { Sine, Square, Triangle }

        private static float Osc(Wave w, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            return w switch
            {
                Wave.Square => p < 0.5f ? 1f : -1f,
                Wave.Triangle => 4f * Mathf.Abs(p - 0.5f) - 1f,
                _ => Mathf.Sin(p * Mathf.PI * 2f),
            };
        }

        private static AudioClip Make(string name, float[] data)
        {
            var c = AudioClip.Create(name, data.Length, 1, Rate, false);
            c.SetData(data, 0);
            return c;
        }

        private static AudioClip Tone(string name, float dur, float f0, float f1, Wave w, float amp)
        {
            int n = (int)(dur * Rate);
            var d = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                phase += Mathf.Lerp(f0, f1, t) / Rate;
                d[i] = Osc(w, phase) * amp * (1f - t) * Mathf.Min(1f, i / 80f);
            }
            return Make(name, d);
        }

        private static AudioClip Noise(string name, float dur, float amp)
        {
            int n = (int)(dur * Rate);
            var d = new float[n];
            var r = new System.Random(1);
            for (int i = 0; i < n; i++) d[i] = ((float)r.NextDouble() * 2f - 1f) * amp * (1f - i / (float)n);
            return Make(name, d);
        }

        private static AudioClip Zap(string name, float dur)
        {
            int n = (int)(dur * Rate);
            var d = new float[n];
            var r = new System.Random(2);
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                phase += (1800f - 1200f * t) / Rate;
                d[i] = (Osc(Wave.Square, phase) * 0.15f + ((float)r.NextDouble() * 2f - 1f) * 0.15f) * (1f - t);
            }
            return Make(name, d);
        }

        private static AudioClip Boom(string name, float dur)
        {
            int n = (int)(dur * Rate);
            var d = new float[n];
            var r = new System.Random(3);
            float lp = 0f, phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                lp += (((float)r.NextDouble() * 2f - 1f) - lp) * 0.08f;
                phase += Mathf.Lerp(90f, 35f, t) / Rate;
                d[i] = (lp * 0.9f + Osc(Wave.Sine, phase) * 0.5f) * Mathf.Pow(1f - t, 2f);
            }
            return Make(name, d);
        }

        private static AudioClip Clank(string name)
        {
            int n = (int)(0.25f * Rate);
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 18f);
                d[i] = (Mathf.Sin(t * 2f * Mathf.PI * 740f) + 0.6f * Mathf.Sin(t * 2f * Mathf.PI * 1130f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 1690f)) * 0.18f * env;
            }
            return Make(name, d);
        }

        private static AudioClip Arp(string name, float[] notes, float step)
        {
            int per = (int)(step * Rate);
            var d = new float[per * notes.Length + Rate / 10];
            for (int k = 0; k < notes.Length; k++)
            {
                float phase = 0f;
                for (int i = 0; i < per + Rate / 10 && k * per + i < d.Length; i++)
                {
                    phase += notes[k] / Rate;
                    float env = Mathf.Exp(-i / (float)Rate * 9f) * Mathf.Min(1f, i / 60f);
                    d[k * per + i] += Osc(Wave.Triangle, phase) * 0.25f * env;
                }
            }
            return Make(name, d);
        }

        /// <summary>
        /// Original placeholder theme "The Expedition" (music-box lead over a brass-like bass), 8 bars in D minor, 104 BPM.
        /// </summary>
        private static AudioClip Theme()
        {
            // MIDI note numbers; 0 = rest. Eighth notes.
            int[] lead =
            {
                62, 0, 65, 69, 67, 65, 64, 65,   62, 0, 60, 62, 64, 65, 67, 0,
                69, 0, 70, 69, 67, 65, 67, 69,   65, 0, 64, 62, 64, 0, 0, 0,
                62, 0, 65, 69, 74, 72, 70, 69,   67, 0, 65, 67, 69, 70, 72, 0,
                74, 72, 70, 69, 67, 65, 64, 62,  61, 0, 64, 0, 62, 0, 0, 0,
            };
            int[] bass = { 38, 38, 41, 41, 46, 46, 45, 45, 38, 38, 43, 43, 45, 45, 45, 45 }; // half notes
            float eighth = 60f / 104f / 2f;
            int samplesPerEighth = (int)(eighth * Rate);
            int total = samplesPerEighth * lead.Length;
            var d = new float[total];
            for (int k = 0; k < lead.Length; k++)
            {
                if (lead[k] == 0) continue;
                float f = 440f * Mathf.Pow(2f, (lead[k] - 69) / 12f);
                float phase = 0f;
                for (int i = 0; i < samplesPerEighth * 2 && k * samplesPerEighth + i < total; i++)
                {
                    phase += f / Rate;
                    float env = Mathf.Exp(-i / (float)Rate * 5f) * Mathf.Min(1f, i / 40f);
                    d[k * samplesPerEighth + i] += (Osc(Wave.Sine, phase) * 0.6f + Osc(Wave.Sine, phase * 2f) * 0.2f) * 0.22f * env;
                }
            }
            int samplesPerHalf = samplesPerEighth * 4;
            for (int k = 0; k < bass.Length; k++)
            {
                float f = 440f * Mathf.Pow(2f, (bass[k] - 69) / 12f);
                float phase = 0f;
                for (int i = 0; i < samplesPerHalf && k * samplesPerHalf + i < total; i++)
                {
                    phase += f / Rate;
                    float env = Mathf.Min(1f, i / 300f) * (1f - i / (float)samplesPerHalf * 0.5f);
                    d[k * samplesPerHalf + i] += Osc(Wave.Triangle, phase) * 0.16f * env;
                }
            }
            return Make("Theme_Expedition", d);
        }
    }
}
