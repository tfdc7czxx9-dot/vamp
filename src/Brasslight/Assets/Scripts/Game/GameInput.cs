using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Vamp.Game
{
    /// <summary>
    /// All player input: keyboard/mouse, gamepad (Steam Deck) and a floating touch joystick.
    /// Gameplay only needs <see cref="Move"/>; menus use the navigation edges.
    /// </summary>
    public sealed class GameInput
    {
        private readonly InputAction _move;
        private readonly InputAction _navigate;
        private readonly InputAction _submit;
        private readonly InputAction _cancel;
        private readonly InputAction _pause;
        private Vector2 _lastNav;
        private int _joystickTouchId = -1;

        /// <summary>Movement in [-1, 1]^2 (length ≤ 1).</summary>
        public Vector2 Move { get; private set; }
        /// <summary>Navigation edge this frame (one step per press).</summary>
        public Vector2Int NavStep { get; private set; }
        /// <summary>Confirm pressed this frame.</summary>
        public bool Submit { get; private set; }
        /// <summary>Back pressed this frame.</summary>
        public bool Cancel { get; private set; }
        /// <summary>Pause pressed this frame.</summary>
        public bool Pause { get; private set; }
        /// <summary>True while a touch joystick is held.</summary>
        public bool JoystickActive { get; private set; }
        /// <summary>Joystick origin in screen pixels.</summary>
        public Vector2 JoystickOrigin { get; private set; }
        /// <summary>Joystick knob in screen pixels.</summary>
        public Vector2 JoystickKnob { get; private set; }
        /// <summary>Screen rects where touches must not start the joystick (e.g. pause button).</summary>
        public Rect[] JoystickExclusions = System.Array.Empty<Rect>();
        /// <summary>Joystick radius in screen pixels.</summary>
        public float JoystickRadius => Mathf.Max(60f, Screen.dpi > 0 ? Screen.dpi * 0.45f : 90f);

        /// <summary>Creates and enables all actions.</summary>
        public GameInput()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");
            _move.AddBinding("<Gamepad>/dpad");

            _navigate = new InputAction("Navigate", InputActionType.Value);
            _navigate.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _navigate.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            _navigate.AddBinding("<Gamepad>/leftStick");
            _navigate.AddBinding("<Gamepad>/dpad");

            _submit = new InputAction("Submit", InputActionType.Button);
            _submit.AddBinding("<Keyboard>/enter");
            _submit.AddBinding("<Keyboard>/space");
            _submit.AddBinding("<Gamepad>/buttonSouth");

            _cancel = new InputAction("Cancel", InputActionType.Button);
            _cancel.AddBinding("<Keyboard>/escape");
            _cancel.AddBinding("<Keyboard>/backspace");
            _cancel.AddBinding("<Gamepad>/buttonEast");

            _pause = new InputAction("Pause", InputActionType.Button);
            _pause.AddBinding("<Keyboard>/escape");
            _pause.AddBinding("<Keyboard>/p");
            _pause.AddBinding("<Gamepad>/start");

            _move.Enable(); _navigate.Enable(); _submit.Enable(); _cancel.Enable(); _pause.Enable();
            EnhancedTouchSupport.Enable();
        }

        /// <summary>Screen positions (GUI space, y down) of touches that began this frame; used for UI taps.</summary>
        public readonly System.Collections.Generic.List<Vector2> Taps = new System.Collections.Generic.List<Vector2>(4);

        /// <summary>Samples all devices. Call once per frame before reading. The joystick is only active during gameplay.</summary>
        public void Update(bool gameplay)
        {
            Vector2 move = _move.ReadValue<Vector2>();
            Taps.Clear();
            var all = ETouch.activeTouches;
            for (int i = 0; i < all.Count; i++)
                if (all[i].phase == UnityEngine.InputSystem.TouchPhase.Began)
                    Taps.Add(new Vector2(all[i].screenPosition.x, Screen.height - all[i].screenPosition.y));
            if (gameplay) UpdateTouch(ref move);
            else { _joystickTouchId = -1; JoystickActive = false; }
            Move = Vector2.ClampMagnitude(move, 1f);

            Vector2 nav = _navigate.ReadValue<Vector2>();
            Vector2Int step = Vector2Int.zero;
            if (_lastNav.magnitude < 0.5f && nav.magnitude >= 0.5f)
            {
                if (Mathf.Abs(nav.x) > Mathf.Abs(nav.y)) step.x = nav.x > 0 ? 1 : -1;
                else step.y = nav.y > 0 ? 1 : -1;
            }
            _lastNav = nav;
            NavStep = step;
            Submit = _submit.WasPressedThisFrame();
            Cancel = _cancel.WasPressedThisFrame();
            Pause = _pause.WasPressedThisFrame();
        }

        private void UpdateTouch(ref Vector2 move)
        {
            JoystickActive = false;
            var touches = ETouch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                if (_joystickTouchId < 0 && t.phase == UnityEngine.InputSystem.TouchPhase.Began && !Excluded(t.screenPosition))
                {
                    _joystickTouchId = t.touchId;
                    JoystickOrigin = t.screenPosition;
                }
                if (t.touchId != _joystickTouchId) continue;
                if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    _joystickTouchId = -1;
                    continue;
                }
                Vector2 delta = t.screenPosition - JoystickOrigin;
                float r = JoystickRadius;
                // Floating joystick: the origin follows the finger when it is dragged past the radius.
                if (delta.magnitude > r) JoystickOrigin = t.screenPosition - delta.normalized * r;
                delta = t.screenPosition - JoystickOrigin;
                JoystickKnob = t.screenPosition;
                JoystickActive = true;
                Vector2 v = delta / r;
                if (v.magnitude > 0.12f) move = v;
                return;
            }
            if (_joystickTouchId >= 0 && touches.Count == 0) _joystickTouchId = -1;
        }

        private bool Excluded(Vector2 screenPos)
        {
            // Rects are in GUI space (y down); convert.
            var guiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
            foreach (var r in JoystickExclusions) if (r.Contains(guiPos)) return true;
            return false;
        }

        /// <summary>Releases the touch joystick (e.g. when a menu opens).</summary>
        public void ResetTouch() => _joystickTouchId = -1;
    }
}
