using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace SpellSystem
{
    /// Tiny wrapper so the sample controls work with both the old Input Manager and the new Input System.
    public static class SpellInput
    {
        public static Vector2 PointerScreen()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        public static bool Fire()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        /// index 0 = key "1", up to key "8".
        public static bool NumberDown(int index)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return false;
            KeyControl[] keys = { k.digit1Key, k.digit2Key, k.digit3Key, k.digit4Key, k.digit5Key, k.digit6Key, k.digit7Key, k.digit8Key };
            return index >= 0 && index < keys.Length && keys[index].wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Alpha1 + index);
#endif
        }

        public static Vector2 Move()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;
            float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
            float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y).normalized;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
#endif
        }
    }
}
