// Compile-check stubs mirroring URP 17.3 / Core RP signatures used by Vamp.Game (verified against package source).
using UnityEngine;

namespace UnityEngine.Rendering
{
    public class VolumeParameter<T> { public T value; public void Override(T x) { value = x; } }
    public class MinFloatParameter : VolumeParameter<float> { }
    public class ClampedFloatParameter : VolumeParameter<float> { }
    public class FloatParameter : VolumeParameter<float> { }
    public class BoolParameter : VolumeParameter<bool> { }
    public class ColorParameter : VolumeParameter<Color> { }
    public class VolumeComponent : ScriptableObject { }

    public sealed class VolumeProfile : ScriptableObject
    {
        public T Add<T>(bool overrides = false) where T : VolumeComponent => CreateInstance<T>();
    }

    public class Volume : MonoBehaviour
    {
        public bool isGlobal { get; set; }
        public float priority;
        public float weight = 1f;
        public VolumeProfile sharedProfile;
        public VolumeProfile profile { get; set; }
    }
}

namespace UnityEngine.Rendering.Universal
{
    public enum DepthOfFieldMode { Off, Gaussian, Bokeh }
    public enum TonemappingMode { None, Neutral, ACES }
    public enum AntialiasingMode { None, FastApproximateAntialiasing, SubpixelMorphologicalAntiAliasing }
    public sealed class DepthOfFieldModeParameter : VolumeParameter<DepthOfFieldMode> { }
    public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode> { }

    public sealed class DepthOfField : VolumeComponent
    {
        public DepthOfFieldModeParameter mode = new DepthOfFieldModeParameter();
        public MinFloatParameter gaussianStart = new MinFloatParameter();
        public MinFloatParameter gaussianEnd = new MinFloatParameter();
        public ClampedFloatParameter gaussianMaxRadius = new ClampedFloatParameter();
        public BoolParameter highQualitySampling = new BoolParameter();
        public MinFloatParameter focusDistance = new MinFloatParameter();
        public ClampedFloatParameter aperture = new ClampedFloatParameter();
        public ClampedFloatParameter focalLength = new ClampedFloatParameter();
    }

    public sealed class Bloom : VolumeComponent
    {
        public MinFloatParameter threshold = new MinFloatParameter();
        public MinFloatParameter intensity = new MinFloatParameter();
        public ClampedFloatParameter scatter = new ClampedFloatParameter();
        public ColorParameter tint = new ColorParameter();
        public BoolParameter highQualityFiltering = new BoolParameter();
    }

    public sealed class Vignette : VolumeComponent
    {
        public ColorParameter color = new ColorParameter();
        public ClampedFloatParameter intensity = new ClampedFloatParameter();
        public ClampedFloatParameter smoothness = new ClampedFloatParameter();
    }

    public sealed class ColorAdjustments : VolumeComponent
    {
        public FloatParameter postExposure = new FloatParameter();
        public ClampedFloatParameter contrast = new ClampedFloatParameter();
        public ColorParameter colorFilter = new ColorParameter();
        public ClampedFloatParameter hueShift = new ClampedFloatParameter();
        public ClampedFloatParameter saturation = new ClampedFloatParameter();
    }

    public sealed class Tonemapping : VolumeComponent
    {
        public TonemappingModeParameter mode = new TonemappingModeParameter();
    }

    public sealed class UniversalAdditionalCameraData : MonoBehaviour
    {
        public bool renderPostProcessing { get; set; }
        public AntialiasingMode antialiasing { get; set; }
    }

    public static class CameraExtensions
    {
        public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this Camera camera) => null;
    }

    public sealed class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        public float renderScale { get; set; }
        public float shadowDistance { get; set; }
        public int maxAdditionalLightsCount { get; set; }
        public override RenderPipeline CreatePipeline() => null;
    }
}
