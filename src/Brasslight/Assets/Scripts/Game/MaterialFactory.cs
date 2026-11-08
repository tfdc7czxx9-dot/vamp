using UnityEngine;

namespace Vamp.Game
{
    /// <summary>
    /// Creates URP materials for sprites, props and ground. Templates are assigned by the editor setup
    /// (Brasslight > Setup) so the shaders are included in player builds; Shader.Find is an editor fallback.
    /// </summary>
    public sealed class MaterialFactory
    {
        private readonly Material _litTemplate;
        private readonly Material _unlitTemplate;

        /// <summary>Creates the factory from optional templates.</summary>
        public MaterialFactory(Material litTemplate, Material unlitTemplate)
        {
            _litTemplate = litTemplate != null ? litTemplate : new Material(FindShader("Universal Render Pipeline/Simple Lit"));
            _unlitTemplate = unlitTemplate != null ? unlitTemplate : new Material(FindShader("Universal Render Pipeline/Unlit"));
        }

        private static Shader FindShader(string name)
        {
            var s = Shader.Find(name);
            if (s == null)
            {
                Debug.LogError($"[Materials] Shader '{name}' not found. Run Brasslight > Setup in the editor.");
                s = Shader.Find("Hidden/InternalErrorShader");
            }
            return s;
        }

        /// <summary>Alpha-clipped, double-sided sprite material (lit or unlit). Emission makes it glow under bloom.</summary>
        public Material Sprite(Texture2D tex, bool lit, Color tint, Color emission)
        {
            var m = new Material(lit ? _litTemplate : _unlitTemplate) { name = "Sprite_" + tex.name };
            m.SetTexture("_BaseMap", tex);
            m.mainTexture = tex;
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_AlphaClip", 1f);
            m.SetFloat("_Cutoff", 0.5f);
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_Surface", 0f);
            if (lit && emission.maxColorComponent > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission);
                m.SetTexture("_EmissionMap", tex);
            }
            m.enableInstancing = true;
            return m;
        }

        /// <summary>Opaque lit material for ground and props.</summary>
        public Material Opaque(Texture2D tex, Color tint, Color emission, Vector2 tiling)
        {
            var m = new Material(_litTemplate) { name = "Opaque_" + (tex != null ? tex.name : "Color") };
            m.SetFloat("_AlphaClip", 0f);
            m.DisableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cull", 2f);
            if (tex != null)
            {
                m.SetTexture("_BaseMap", tex);
                m.SetTextureScale("_BaseMap", tiling);
            }
            else m.SetTexture("_BaseMap", Texture2D.whiteTexture);
            m.SetColor("_BaseColor", tint);
            if (emission.maxColorComponent > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission);
            }
            m.enableInstancing = true;
            return m;
        }

        /// <summary>Unlit solid colour (lines, glow). HDR colours > 1 bloom.</summary>
        public Material Glow(Color hdr)
        {
            var m = new Material(_unlitTemplate) { name = "Glow" };
            m.SetTexture("_BaseMap", Texture2D.whiteTexture);
            m.SetColor("_BaseColor", hdr);
            m.SetFloat("_Cull", 0f);
            return m;
        }
    }
}
