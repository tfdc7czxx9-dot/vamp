// Compile-check stubs mirroring com.unity.inputsystem 1.20 signatures used by Vamp.Game (verified against package source).
using UnityEngine;

namespace UnityEngine.InputSystem.Utilities
{
    public struct ReadOnlyArray<T>
    {
        public int Count => 0;
        public T this[int index] => default;
    }
}

namespace UnityEngine.InputSystem
{
    public enum InputActionType { Value, Button, PassThrough }
    public enum TouchPhase { None, Began, Moved, Ended, Canceled, Stationary }

    public sealed class InputAction
    {
        public InputAction(string name = null, InputActionType type = default, string binding = null, string interactions = null, string processors = null, string expectedControlType = null) { }
        public TValue ReadValue<TValue>() where TValue : struct => default;
        public bool WasPressedThisFrame() => false;
        public void Enable() { }
        public void Disable() { }
    }

    public static class InputActionSetupExtensions
    {
        public struct BindingSyntax { }
        public struct CompositeSyntax
        {
            public CompositeSyntax With(string name, string binding, string groups = null, string processors = null) => this;
        }
        public static BindingSyntax AddBinding(this InputAction action, string path, string interactions = null, string processors = null, string groups = null) => default;
        public static CompositeSyntax AddCompositeBinding(this InputAction action, string composite, string interactions = null, string processors = null) => default;
    }
}

namespace UnityEngine.InputSystem.EnhancedTouch
{
    public static class EnhancedTouchSupport
    {
        public static void Enable() { }
    }

    public struct Touch
    {
        public static UnityEngine.InputSystem.Utilities.ReadOnlyArray<Touch> activeTouches => default;
        public UnityEngine.InputSystem.TouchPhase phase => default;
        public int touchId => 0;
        public Vector2 screenPosition => default;
        public Vector2 startScreenPosition => default;
    }
}
