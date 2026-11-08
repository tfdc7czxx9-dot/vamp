using UnityEngine;

namespace Vamp.Game
{
    /// <summary>Shader variant a material needs. Each has its own template asset so the variant survives build stripping.</summary>
    public enum MaterialVariant
    {
        /// <summary>Simple Lit, alpha clip.</summary>
        LitClip,
        /// <summary>Simple Lit, alpha clip + emission.</summary>
        LitClipEmissive,
        /// <summary>Simple Lit, opaque.</summary>
        LitOpaque,
        /// <summary>Simple Lit, opaque + emission.</summary>
        LitOpaqueEmissive,
        /// <summary>Unlit, alpha clip.</summary>
        UnlitClip,
        /// <summary>Unlit, opaque.</summary>
        UnlitOpaque,
    }

    /// <summary>
    /// Creates URP materials by copying template material assets (created by Brasslight > Setup Project).
    /// Keywords live on the templates, never toggled at runtime: shader_feature variants enabled only at runtime
    /// are stripped from player builds. Without templates (editor only) it builds them via Shader.Find.
    /// </summary>
    public sealed class MaterialFactory
    {
        private readonly Material[] _templates = new Material[6];

        /// <summary>Creates the factory; <paramref name="templates"/> is indexed by <see cref="MaterialVariant"/> (entries may be null).</summary>
        public MaterialFactory(Material[] templates)
        {
            for (int i = 0; i < _templates.Length; i++)
            {
                var t = templates != null && i < templates.Length ? templates[i] : null;
                _templates[i] = t != null ? t : CreateFallback((MaterialVariant)i);
            }
        }

        /// <summary>Configures a template's keywords and properties (shared with the editor setup).</summary>
        public static void ConfigureTemplate(Material m, MaterialVariant v)
        {
            bool clip = v == MaterialVariant.LitClip || v == MaterialVariant.LitClipEmissive || v == MaterialVariant.UnlitClip;
            bool emissive = v == MaterialVariant.LitClipEmissive || v == MaterialVariant.LitOpaqueEmissive;
            m.enableInstancing = true;
            m.SetFloat("_AlphaClip", clip ? 1f : 0f);
            m.SetFloat("_Cutoff", 0.5f);
            if (clip) m.EnableKeyword("_ALPHATEST_ON"); else m.DisableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cull", clip ? 0f : 2f);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.white * 0.01f);
            }
            else m.DisableKeyword("_EMISSION");
        }

        /// <summary>Shader name for a variant.</summary>
        public static string ShaderName(MaterialVariant v) =>
            v == MaterialVariant.UnlitClip || v == MaterialVariant.UnlitOpaque ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Simple Lit";

        private static Material CreateFallback(MaterialVariant v)
        {
            var s = Shader.Find(ShaderName(v));
            if (s == null)
            {
                Debug.LogError($"[Materials] Shader '{ShaderName(v)}' not found. Run Brasslight > Setup Project in the editor.");
                s = Shader.Find("Hidden/InternalErrorShader");
            }
            var m = new Material(s) { name = "Template_" + v };
            ConfigureTemplate(m, v);
            return m;
        }

        private Material From(MaterialVariant v, string name)
        {
            var m = new Material(_templates[(int)v]) { name = name, enableInstancing = true };
            return m;
        }

        /// <summary>Alpha-clipped, double-sided sprite material (lit or unlit). Emission makes it glow under bloom.</summary>
        public Material Sprite(Texture2D tex, bool lit, Color tint, Color emission)
        {
            bool emissive = lit && emission.maxColorComponent > 0f;
            var m = From(!lit ? MaterialVariant.UnlitClip : emissive ? MaterialVariant.LitClipEmissive : MaterialVariant.LitClip, "Sprite_" + tex.name);
            m.SetTexture("_BaseMap", tex);
            m.mainTexture = tex;
            m.SetColor("_BaseColor", tint);
            if (emissive)
            {
                m.SetColor("_EmissionColor", emission);
                m.SetTexture("_EmissionMap", tex);
            }
            return m;
        }

        /// <summary>Opaque lit material for ground and props.</summary>
        public Material Opaque(Texture2D tex, Color tint, Color emission, Vector2 tiling)
        {
            bool emissive = emission.maxColorComponent > 0f;
            var m = From(emissive ? MaterialVariant.LitOpaqueEmissive : MaterialVariant.LitOpaque, "Opaque_" + (tex != null ? tex.name : "Color"));
            m.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            if (tex != null) m.SetTextureScale("_BaseMap", tiling);
            m.SetColor("_BaseColor", tint);
            if (emissive) m.SetColor("_EmissionColor", emission);
            return m;
        }

        /// <summary>Unlit solid colour (lines, glow). HDR colours > 1 bloom.</summary>
        public Material Glow(Color hdr)
        {
            var m = From(MaterialVariant.UnlitOpaque, "Glow");
            m.SetTexture("_BaseMap", Texture2D.whiteTexture);
            m.SetColor("_BaseColor", hdr);
            return m;
        }
    }
}
