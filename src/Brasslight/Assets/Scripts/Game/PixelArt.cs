using UnityEngine;
using Vamp.Core.Math;

namespace Vamp.Game
{
    /// <summary>
    /// Generates placeholder pixel art at runtime (point-filtered textures) so the game is fully playable
    /// before final art exists. Final sprites replace these via the art pipeline (design/art/art-bible.md).
    /// </summary>
    public static class PixelArt
    {
        private static readonly Color Clear = new Color(0, 0, 0, 0);
        private static readonly Color Outline = new Color(0.08f, 0.06f, 0.06f, 1f);

        /// <summary>Creates a point-filtered, clamped texture.</summary>
        public static Texture2D NewTexture(int w, int h, string name)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = name,
            };
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = Clear;
            t.SetPixels(px);
            return t;
        }

        /// <summary>
        /// Procedural creature: mirrored cellular blob with shading, outline, eyes and a body plan
        /// derived from the sprite index (crawler, flyer, biped, heavy).
        /// </summary>
        public static Texture2D Creature(int index, Color body, Color accent, bool boss = false)
        {
            int size = boss ? 32 : 24;
            var rng = new Rng((uint)(index * 7919 + 17));
            int half = size / 2;
            bool[,] m = new bool[size, size];
            int plan = index % 4;
            for (int y = 2; y < size - 2; y++)
            for (int x = 1; x < half; x++)
            {
                float cx = (half - x) / (float)half;          // 0 at centre
                float cy = Mathf.Abs(y - size * (plan == 1 ? 0.55f : 0.45f)) / (size * 0.5f);
                float p = 0.95f - cx * (plan == 3 ? 0.55f : 0.85f) - cy * (plan == 2 ? 0.6f : 0.9f);
                m[x, y] = rng.NextFloat() < p;
            }
            // Smooth twice (cellular automaton) for organic shapes.
            for (int it = 0; it < 2; it++)
            {
                var n = new bool[size, size];
                for (int y = 1; y < size - 1; y++)
                for (int x = 1; x < half; x++)
                {
                    int c = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if ((dx != 0 || dy != 0) && m[Mathf.Min(x + dx, half - 1), y + dy]) c++;
                    n[x, y] = m[x, y] ? c >= 3 : c >= 5;
                }
                m = n;
            }
            // Mirror.
            for (int y = 0; y < size; y++)
            for (int x = 0; x < half; x++)
                m[size - 1 - x, y] = m[x, y];
            // Body-plan extras.
            if (plan == 0 || plan == 3) // legs
                for (int k = 0; k < 3; k++)
                {
                    int lx = half - 6 + k * 5;
                    for (int y = 1; y < 5; y++) if (lx >= 0 && lx < size) m[lx, y] = true;
                }
            if (plan == 1) // wings
                for (int y = size / 2; y < size / 2 + 6; y++)
                for (int x = 0; x < 4; x++) { m[x, y] = true; m[size - 1 - x, y] = true; }

            var tex = NewTexture(size, size, "Creature_" + index);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (!m[x, y]) continue;
                float shade = Mathf.Lerp(0.7f, 1.15f, y / (float)size);
                Color c = body * shade;
                if (((x + y * 3) % 7 == 0) && rng.NextFloat() < 0.5f) c = Color.Lerp(c, accent, 0.6f);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
            // Outline.
            var outline = new System.Collections.Generic.List<Vector2Int>();
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (m[x, y]) continue;
                bool edge = (x > 0 && m[x - 1, y]) || (x < size - 1 && m[x + 1, y]) || (y > 0 && m[x, y - 1]) || (y < size - 1 && m[x, y + 1]);
                if (edge) outline.Add(new Vector2Int(x, y));
            }
            foreach (var p in outline) tex.SetPixel(p.x, p.y, Outline);
            // Eyes.
            int ey = Mathf.RoundToInt(size * (plan == 1 ? 0.62f : 0.6f));
            int ex = half - 3;
            for (int s = -1; s <= 1; s += 2)
            {
                int x = s < 0 ? ex : size - 1 - ex;
                if (m[x, ey]) { tex.SetPixel(x, ey, accent * 1.6f); tex.SetPixel(x, ey + 1, Color.white); }
            }
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Draws an ASCII template; '.' is transparent, other chars map through the palette.</summary>
        public static Texture2D FromTemplate(string name, string[] rows, System.Func<char, Color> palette)
        {
            int h = rows.Length, w = rows[0].Length;
            var tex = NewTexture(w, h, name);
            for (int r = 0; r < h; r++)
            for (int x = 0; x < w && x < rows[r].Length; x++)
            {
                char ch = rows[r][x];
                if (ch == '.' || ch == ' ') continue;
                tex.SetPixel(x, h - 1 - r, palette(ch));
            }
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Expedition leader sprite (coat, goggles, hat) tinted per leader.</summary>
        public static Texture2D Leader(int index)
        {
            Color coat = index switch
            {
                0 => new Color(0.55f, 0.27f, 0.15f),
                1 => new Color(0.2f, 0.4f, 0.28f),
                2 => new Color(0.18f, 0.25f, 0.5f),
                3 => new Color(0.45f, 0.42f, 0.38f),
                4 => new Color(0.38f, 0.2f, 0.45f),
                _ => new Color(0.75f, 0.72f, 0.66f),
            };
            string[] rows =
            {
                "....kkkkkk......",
                "....khhhhk......",
                "....khhhhk......",
                "...kkkkkkkk.....",
                "...kssssssk.....",
                "...ksgksgsk.....",
                "...kssssssk.....",
                "....kssssk......",
                "...kccbbcck.....",
                "..kcccbbccck....",
                "..kcckbbkcck....",
                "..kscckkccsk....",
                "...kccbbcck.....",
                "...kcccccck.....",
                "...kpppppk......",
                "...kpk.kpk......",
                "...kpk.kpk......",
                "...kkk.kkk......",
            };
            return FromTemplate("Leader_" + index, rows, ch => ch switch
            {
                'k' => Outline,
                'h' => new Color(0.22f, 0.2f, 0.2f),
                's' => new Color(0.96f, 0.78f, 0.62f),
                'g' => new Color(1f, 0.75f, 0.25f),
                'c' => coat,
                'b' => new Color(0.85f, 0.65f, 0.25f),
                'p' => new Color(0.25f, 0.22f, 0.28f),
                _ => Color.magenta,
            });
        }

        /// <summary>Structure sprites by structure kind index (Barricade..FieldWorkshop).</summary>
        public static Texture2D Structure(int kind)
        {
            string[][] t =
            {
                new[] { "................", "..kk........kk..", ".kwwk......kwwk.", "..kwwk....kwwk..", "...kwwk..kwwk...", "....kwwkkwwk....", ".....kwwwwk.....", "......kwwk......", ".....kwwwwk.....", "....kwwkkwwk....", "...kwwk..kwwk...", "..kwwk....kwwk..", ".kwwk......kwwk.", ".kkk........kkk." },
                new[] { "................", "......kkkk......", ".....kbbbbk.....", "....kbbbbbbkkkk.", "....kbbbbbbmmmmk", "....kbbbbbbkkkk.", ".....kbbbbk.....", "......kkkk......", ".......kk.......", "......kwwk......", ".....kw..wk.....", "....kw....wk....", "...kw......wk...", "...kk......kk..." },
                new[] { "................", "................", "..kk......kk....", ".krrk....krrk...", "..kk......kk....", "................", "......kk.....kk.", ".....krrk...krrk", "......kk.....kk.", "................", "..kk......kk....", ".krrk....krrk...", "..kk......kk....", "................" },
                new[] { ".......yy.......", "......kyyk......", ".......kk.......", "......kbbk......", ".....kbyybk.....", "......kbbk......", ".....kbyybk.....", "......kbbk......", "......kmmk......", "......kmmk......", "......kmmk......", ".....kmmmmk.....", "....kmmmmmmk....", "...kkkkkkkkkk..." },
                new[] { "................", "......kkkk......", ".....kbbbbk.....", "....kkyyyykk....", "....kyyyyyyk....", "....kyyyyyyk....", "....kyyyyyyk....", "....kkyyyykk....", ".....kbbbbk.....", "......kmmk......", "......kmmk......", "......kmmk......", ".....kmmmmk.....", "....kkkkkkkk...." },
                new[] { "................", ".......kk.......", "......kwwk......", ".....kwwwwk.....", "....kwwwwwwk....", "...kwwwwwwwwk...", "..kwwwwwwwwwwk..", "..kwkkkkkkkkwk..", "..kwk.kmmk.kwk..", "..kwk.kmmk.kwk..", "..kwk.kmmk.kwk..", "..kwkkmmmmkkwk..", "..kwkkkkkkkkwk..", "..kkk......kkk.." },
            };
            int i = Mathf.Clamp(kind, 0, t.Length - 1);
            return FromTemplate("Structure_" + i, t[i], ch => ch switch
            {
                'k' => Outline,
                'w' => new Color(0.55f, 0.38f, 0.22f),
                'b' => new Color(0.82f, 0.62f, 0.25f),
                'm' => new Color(0.4f, 0.42f, 0.46f),
                'r' => new Color(0.85f, 0.2f, 0.15f),
                'y' => new Color(1f, 0.85f, 0.35f),
                _ => Color.magenta,
            });
        }

        /// <summary>Pickup sprites: 0 Aether crystal, 1 brass cog, 2 tea tin.</summary>
        public static Texture2D Pickup(int kind)
        {
            string[][] t =
            {
                new[] { "...kk...", "..kcck..", ".kcwcck.", "kccwccck", ".kcccck.", "..kcck..", "...kk..." },
                new[] { ".k.kk.k.", "kbkbbkbk", ".kbbbbk.", "kbbkkbbk", "kbbkkbbk", ".kbbbbk.", "kbkbbkbk", ".k.kk.k." },
                new[] { ".kkkkkk.", "kggggggk", "krrrrrrk", "krwwwwrk", "krrrrrrk", "kggggggk", ".kkkkkk." },
            };
            int i = Mathf.Clamp(kind, 0, t.Length - 1);
            return FromTemplate("Pickup_" + i, t[i], ch => ch switch
            {
                'k' => Outline,
                'c' => new Color(0.35f, 0.9f, 1f),
                'w' => Color.white,
                'b' => new Color(0.9f, 0.68f, 0.25f),
                'g' => new Color(0.85f, 0.7f, 0.3f),
                'r' => new Color(0.7f, 0.15f, 0.15f),
                _ => Color.magenta,
            });
        }

        /// <summary>Round glowing dot (projectiles, sparks).</summary>
        public static Texture2D Dot(int size, Color c)
        {
            var tex = NewTexture(size, size, "Dot");
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                if (d <= r - 0.5f) tex.SetPixel(x, y, d < r * 0.5f ? Color.white : c);
            }
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Ring outline (remnant rings, plots, shockwaves). Thickness in pixels.</summary>
        public static Texture2D Ring(int size, int thickness, Color c, bool dashed = false)
        {
            var tex = NewTexture(size, size, "Ring");
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f - r, y + 0.5f - r);
                float d = p.magnitude;
                if (d > r - 0.5f || d < r - 0.5f - thickness) continue;
                if (dashed && ((int)((Mathf.Atan2(p.y, p.x) + Mathf.PI) / (Mathf.PI * 2f) * 16f)) % 2 == 1) continue;
                tex.SetPixel(x, y, c);
            }
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Dithered blob shadow.</summary>
        public static Texture2D BlobShadow()
        {
            const int s = 16;
            var tex = NewTexture(s, s, "BlobShadow");
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x + 0.5f - s / 2f) / (s / 2f), dy = (y + 0.5f - s / 2f) / (s / 2f);
                float d = dx * dx + dy * dy;
                if (d < 0.55f || (d < 1f && (x + y) % 2 == 0)) tex.SetPixel(x, y, new Color(0f, 0f, 0f, 1f));
            }
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Tiling ground texture with noise, pebbles and cracks in the world palette.</summary>
        public static Texture2D Ground(Color baseColor, Color accent, int seed)
        {
            const int s = 64;
            var tex = NewTexture(s, s, "Ground");
            tex.wrapMode = TextureWrapMode.Repeat;
            var rng = new Rng((uint)(seed * 31 + 7));
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float n = rng.Range(-0.06f, 0.06f);
                float patch = Mathf.PerlinNoise(x * 0.09f + seed, y * 0.09f) * 0.18f - 0.09f;
                Color c = baseColor * (1f + n + patch);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
            for (int i = 0; i < 40; i++)
            {
                int x = rng.Range(0, s), y = rng.Range(0, s);
                tex.SetPixel(x, y, baseColor * 1.35f);
                tex.SetPixel((x + 1) % s, y, baseColor * 0.6f);
            }
            for (int i = 0; i < 6; i++)
            {
                int x = rng.Range(0, s), y = rng.Range(0, s);
                tex.SetPixel(x, y, Color.Lerp(baseColor, accent, 0.7f));
            }
            for (int c = 0; c < 4; c++)
            {
                int x = rng.Range(0, s), y = rng.Range(0, s);
                for (int k = 0; k < 10; k++)
                {
                    tex.SetPixel((x + s) % s, (y + s) % s, baseColor * 0.55f);
                    x += rng.Range(-1, 2);
                    y += 1;
                }
            }
            tex.Apply(false, false);
            return tex;
        }
    }
}
