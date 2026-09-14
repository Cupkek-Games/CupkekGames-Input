using CupkekGames.KeyValueDatabases;
using UnityEngine;

#if UNITY_INPUT
using UnityEngine.InputSystem;
#endif

namespace CupkekGames.Input
{
    [CreateAssetMenu(fileName = "InputIconDatabaseSO", menuName = "CupkekGames/Input/Input Icon Database")]
    public class InputIconDatabaseSO : ScriptableObject
    {
        [Header("Keyboard & Mouse")]
        [SerializeField] private KeyValueDatabase<string, InputIconResult> _keyboardMouse;
        [Header("Xbox")]
        [SerializeField] private KeyValueDatabase<string, InputIconResult> _xbox;
        [Header("PlayStation 4")]
        [SerializeField] private KeyValueDatabase<string, InputIconResult> _playStation;
        [Header("PlayStation 5")]
        [SerializeField] private KeyValueDatabase<string, InputIconResult> _playStation5;
#if UNITY_INPUT
        public InputIconResultExtra GetInputPromptFromName(PlayerInput playerInput, string actionName, string preferredDevice = null)
        {
            return GetInputPrompt(playerInput, playerInput.actions[actionName], false, preferredDevice);
        }
        /// <summary>
        /// The binding a prompt shows for <paramref name="action"/>: the first binding of the
        /// player's current control scheme, or, when <paramref name="preferredDevice"/> names a
        /// device layout (for example "Mouse"), the first such binding on that device, falling
        /// back to the scheme's first binding when the device has none.
        /// </summary>
        public InputIconResultExtra GetInputPrompt(PlayerInput playerInput, InputAction action, bool textOnly = false, string preferredDevice = null)
        {
            InputIconResultExtra result = new InputIconResultExtra();

            string controlScheme = playerInput.currentControlScheme;
            result.BindingIndex = action.GetBindingIndex(group: controlScheme);
            if (!string.IsNullOrEmpty(preferredDevice))
            {
                int preferred = FindBindingOnDevice(action, controlScheme, preferredDevice);
                if (preferred != -1)
                {
                    result.BindingIndex = preferred;
                }
            }
            if (result.BindingIndex == -1)
            {
                Debug.LogWarning("bindingIndex == -1: " + action.name + " - " + controlScheme);
                return result;
            }
            var binding = action.bindings[result.BindingIndex];
            if (binding.isPartOfComposite)
            {
                // hard coded logic - assumes that if you found a part of a composite, that it's the first one.
                // And that the one preceeding it, must be the 'Composite head' that contains the parts
                result.BindingIndex--;
            }
            string displayString = action.GetBindingDisplayString(result.BindingIndex, out string deviceLayoutName, out string controlPath, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            // Debug.Log(deviceLayoutName + " - " + displayString + " - " + controlPath);

            if (!textOnly)
            {
                InputIconControlScheme device = InputIconControlSchemeExtensions.FromString(controlScheme);

                KeyValueDatabase<string, InputIconResult> database = GetDatabase(device);
                if (controlPath != null && database.ContainsKey(controlPath))
                {
                    result.IconResult = database.GetValue(controlPath);
                }
                else
                {
                    result.IconResult = null;
                    result.Text = displayString;
                }
            }
            else
            {
                result.Text = displayString;
            }

            return result;
        }
#endif

        private static int FindBindingOnDevice(InputAction action, string controlScheme, string deviceLayout)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isComposite || !BindingInScheme(binding, controlScheme)) continue;
                string layout = InputControlPath.TryGetDeviceLayout(binding.effectivePath);
                if (layout != null && string.Equals(layout, deviceLayout, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool BindingInScheme(InputBinding binding, string controlScheme)
        {
            if (string.IsNullOrEmpty(binding.groups)) return false;
            foreach (string group in binding.groups.Split(InputBinding.Separator))
            {
                if (group == controlScheme) return true;
            }
            return false;
        }

        public KeyValueDatabase<string, InputIconResult> GetDatabase(InputIconControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                case InputIconControlScheme.KeyboardMouse:
                    return _keyboardMouse;
                case InputIconControlScheme.PlayStation4:
                    return _playStation;
                case InputIconControlScheme.PlayStation5:
                    return _playStation5;
                default:
                    return _xbox;
            }
        }
    }
}
