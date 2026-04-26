#if UNITY_INPUT
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#endif

namespace CupkekGames.Core
{
    public enum InputIconControlScheme
    {
        KeyboardMouse,
        Xbox,
        PlayStation4,
        PlayStation5
    }
    public static class InputIconControlSchemeExtensions
    {
        public static InputIconControlScheme FromString(string controlScheme)
        {
            if (string.IsNullOrEmpty(controlScheme))
            {
                return InputIconControlScheme.Xbox;
            }

            if (controlScheme.Contains("Keyboard"))
            {
                // Keyboard or Keyboard&Mouse
                return InputIconControlScheme.KeyboardMouse;
            }
            #if UNITY_INPUT
                else if (controlScheme == "Gamepad")
                {
                    var gamepad = Gamepad.current;
                    var gamepadType = gamepad.GetType();

                    var dualSenseType = Type.GetType("UnityEngine.InputSystem.DualShock.DualSenseGamepadHID, Unity.InputSystem");
                    if (dualSenseType != null && dualSenseType.IsAssignableFrom(gamepadType))
                    {
                        return InputIconControlScheme.PlayStation5;
                    }

                    var dualShockType = Type.GetType("UnityEngine.InputSystem.DualShock.DualShockGamepad, Unity.InputSystem");
                    if (dualShockType != null && dualShockType.IsAssignableFrom(gamepadType))
                    {
                        return InputIconControlScheme.PlayStation4;
                    }
                }
            #endif

            return InputIconControlScheme.Xbox;
        }
    }
}
