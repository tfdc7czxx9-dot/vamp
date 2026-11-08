using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Vamp.Game
{
    /// <summary>
    /// Global post-processing volume for the diorama look: tilt-shift-style depth of field, bloom,
    /// vignette and per-world grading. Settings follow the quality tier and accessibility options.
    /// </summary>
    public sealed class PostFx
    {
        private readonly VolumeProfile _profile;
        private readonly DepthOfField _dof;
        private readonly Bloom _bloom;
        private readonly Vignette _vignette;
        private readonly ColorAdjustments _color;
        private readonly Tonemapping _tonemap;

        /// <summary>Creates the global volume.</summary>
        public PostFx(Transform parent)
        {
            var go = new GameObject("PostFx");
            go.transform.SetParent(parent, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _dof = _profile.Add<DepthOfField>(true);
            _bloom = _profile.Add<Bloom>(true);
            _vignette = _profile.Add<Vignette>(true);
            _color = _profile.Add<ColorAdjustments>(true);
            _tonemap = _profile.Add<Tonemapping>(true);
            volume.profile = _profile;

            _tonemap.mode.Override(TonemappingMode.Neutral);
            _vignette.intensity.Override(0.28f);
            _vignette.smoothness.Override(0.45f);
            _bloom.threshold.Override(0.95f);
            _bloom.scatter.Override(0.65f);
        }

        /// <summary>Applies tier, DOF toggle and world grading.</summary>
        public void Apply(QualityTier tier, bool dofEnabled, float cameraDistance, Color worldTint)
        {
            bool dof = dofEnabled && tier != QualityTier.Low;
            if (!dof) _dof.mode.Override(DepthOfFieldMode.Off);
            else if (tier == QualityTier.Medium)
            {
                _dof.mode.Override(DepthOfFieldMode.Gaussian);
                _dof.gaussianStart.Override(cameraDistance + 3f);
                _dof.gaussianEnd.Override(cameraDistance + 16f);
                _dof.gaussianMaxRadius.Override(1.2f);
            }
            else
            {
                _dof.mode.Override(DepthOfFieldMode.Bokeh);
                _dof.focusDistance.Override(cameraDistance);
                _dof.focalLength.Override(tier == QualityTier.Ultra ? 120f : 95f);
                _dof.aperture.Override(5.6f);
            }
            _bloom.intensity.Override(tier == QualityTier.Low ? 0.35f : 0.85f);
            _bloom.highQualityFiltering.Override(tier == QualityTier.Ultra);
            _color.colorFilter.Override(Color.Lerp(Color.white, worldTint, 0.12f));
            _color.saturation.Override(8f);
            _color.contrast.Override(8f);
        }

        /// <summary>Enables post-processing on a camera.</summary>
        public static void EnableOn(Camera cam, QualityTier tier)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = tier >= QualityTier.High ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None;
        }

        /// <summary>Applies render scale and light limits to the active URP asset (player builds only, to keep editor assets untouched).</summary>
        public static void ApplyPipeline(QualityTier tier)
        {
            Application.targetFrameRate = tier == QualityTier.Low ? 30 : 60;
            if (Application.isEditor) return;
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = tier switch { QualityTier.Low => 0.75f, QualityTier.Medium => 0.9f, _ => 1f };
                urp.shadowDistance = tier switch { QualityTier.Low => 0f, QualityTier.Medium => 30f, _ => 60f };
                urp.maxAdditionalLightsCount = tier switch { QualityTier.Low => 4, QualityTier.Medium => 8, _ => 16 };
            }
        }
    }
}
