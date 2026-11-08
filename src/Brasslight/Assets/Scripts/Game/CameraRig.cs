using UnityEngine;

namespace Vamp.Game
{
    /// <summary>
    /// Diorama camera: perspective, narrow FOV, fixed pitch (ADR-0002). Follows a focus point with smoothing
    /// and supports screen shake.
    /// </summary>
    public sealed class CameraRig
    {
        /// <summary>The camera.</summary>
        public Camera Cam { get; }
        /// <summary>Pitch in degrees.</summary>
        public float Pitch = 52f;
        /// <summary>Vertical field of view.</summary>
        public float Fov = 28f;
        /// <summary>Visible half-height on the ground plane (world units).</summary>
        public float HalfHeight = 9.5f;
        /// <summary>Screen shake enabled (accessibility option).</summary>
        public bool ShakeEnabled = true;

        private Vector3 _focus;
        private float _shake;

        /// <summary>Distance from camera to focus.</summary>
        public float Distance => HalfHeight / Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad);

        /// <summary>Creates the camera GameObject.</summary>
        public CameraRig(Transform parent)
        {
            var go = new GameObject("DioramaCamera");
            go.transform.SetParent(parent, false);
            Cam = go.AddComponent<Camera>();
            Cam.fieldOfView = Fov;
            Cam.nearClipPlane = 1f;
            Cam.farClipPlane = 250f;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            go.tag = "MainCamera";
            go.AddComponent<AudioListener>();
        }

        /// <summary>Adds a shake impulse (0..1).</summary>
        public void Shake(float amount) { if (ShakeEnabled) _shake = Mathf.Min(1f, _shake + amount); }

        /// <summary>Snaps to a focus point.</summary>
        public void Snap(Vector3 focus) { _focus = focus; Apply(0f); }

        /// <summary>Follows a focus point.</summary>
        public void Follow(Vector3 focus, float dt)
        {
            _focus = Vector3.Lerp(_focus, focus, 1f - Mathf.Exp(-8f * dt));
            Apply(dt);
        }

        private void Apply(float dt)
        {
            Cam.fieldOfView = Fov;
            var rot = Quaternion.Euler(Pitch, 0f, 0f);
            Vector3 offset = Vector3.zero;
            if (_shake > 0f)
            {
                offset = Random.insideUnitSphere * _shake * 0.35f;
                _shake = Mathf.Max(0f, _shake - dt * 2.5f);
            }
            Cam.transform.SetPositionAndRotation(_focus - rot * Vector3.forward * Distance + offset, rot);
        }
    }
}
