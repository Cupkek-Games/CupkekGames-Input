using System;
using System.Collections.Generic;

#if UNITY_INPUT
using UnityEngine;
using UnityEngine.InputSystem;
#endif

namespace CupkekGames.Core
{
    public static class InputDeviceManager
    {
#if UNITY_INPUT
        private static List<PlayerInput> _playerInputs = new List<PlayerInput>();
        public static List<PlayerInput> PlayerInputs => _playerInputs;

        private static List<string> _escapeActionNames = new List<string>() { "UI/Cancel" };
        public static List<string> EscapeActionNames => _escapeActionNames;

        private static List<string> _loadingActionMaps;
        public static List<string> LoadingActionMaps => _loadingActionMaps;

        public static event Action<PlayerInput, int> OnPlayerAdded;
        public static event Action<PlayerInput, int> OnPlayerRemoved;
#endif
        private static List<InputIconControlScheme> _currentSchemes = new List<InputIconControlScheme>();
        public static List<InputIconControlScheme> CurrentSchemes => _currentSchemes;

        public static event Action<InputIconControlScheme> OnControlSchemeChange;

#if UNITY_INPUT
        public static void OnEnable(List<string> escapeActionNames, List<string> loadingActionMaps,
            bool localMultiplayer)
        {
            _playerInputs = new List<PlayerInput>();
            _currentSchemes = new List<InputIconControlScheme>();
            _escapeActionNames = escapeActionNames;
            _loadingActionMaps = loadingActionMaps;

            if (!localMultiplayer)
            {
                InputSystem.onActionChange += OnActionChange;
            }

            // Project wide action maps
            SetDisabledActionMaps(InputSystem.actions, _loadingActionMaps);
        }

        public static void OnDisable()
        {
            InputSystem.onActionChange -= OnActionChange;
        }

        public static void AddPlayerInput(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                Debug.LogError("InputDeviceManager: AddPlayerInput: PlayerInput is null");
                return;
            }

            foreach (string actionName in _escapeActionNames)
            {
                InputAction action = playerInput.actions[actionName];
                if (action != null)
                {
                    action.performed += OnEscape;
                }
            }

            int playerIndex = _playerInputs.Count;
            _playerInputs.Add(playerInput);

            _currentSchemes.Add(InputIconControlSchemeExtensions.FromString(playerInput.currentControlScheme));

            OnPlayerAdded?.Invoke(playerInput, playerIndex);

            SetDisabledActionMaps(playerInput.actions, _loadingActionMaps);
        }

        public static void RemovePlayerInput(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                Debug.LogError("InputDeviceManager: RemovePlayerInput: PlayerInput is null");
                return;
            }

            foreach (string actionName in _escapeActionNames)
            {
                InputAction action = playerInput.actions[actionName];
                if (action != null)
                {
                    action.performed -= OnEscape;
                }
            }

            int indexOf = _playerInputs.IndexOf(playerInput);
            if (indexOf != -1)
            {
                OnPlayerRemoved?.Invoke(playerInput, indexOf);
                _playerInputs.RemoveAt(indexOf);
                _currentSchemes.RemoveAt(indexOf);
            }
        }

        private static void OnEscape(InputAction.CallbackContext context)
        {
            InputEscapeManager.InputEscapeEvent?.Invoke();
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                UpdateControlScheme(0);
            }
            else if (change == InputActionChange.BoundControlsChanged)
            {
                UpdateControlScheme(0, true);
            }
        }

        public static void UpdateControlScheme(int inputIndex, bool forceInvoke = false)
        {
            PlayerInput playerInput = _playerInputs[inputIndex];
            InputIconControlScheme newScheme =
                InputIconControlSchemeExtensions.FromString(playerInput.currentControlScheme);

            UpdateControlScheme(newScheme, inputIndex, forceInvoke);
        }


        public static void ActionMapEnableOthers(InputActionAsset inputActionAsset, List<string> actionMapsToEnable)
        {
            // Disable all action maps first
            foreach (var actionMap in inputActionAsset.actionMaps)
            {
                actionMap.Disable();
            }

            // Enable only the specified action maps
            foreach (var actionMapName in actionMapsToEnable)
            {
                var actionMap = inputActionAsset.FindActionMap(actionMapName);
                if (actionMap != null)
                {
                    actionMap.Enable();
                }
                else
                {
                    Debug.LogWarning($"Action map '{actionMapName}' not found.");
                }
            }
        }

        public static void SetEnabledActionMaps(InputActionAsset asset, ICollection<string> mapsToEnable)
        {
            foreach (var map in asset.actionMaps)
            {
                if (mapsToEnable.Contains(map.name))
                {
                    map.Enable();
                }
                else
                {
                    map.Disable();
                }
            }
        }

        public static void SetDisabledActionMaps(InputActionAsset asset, ICollection<string> mapsToDisable)
        {
            foreach (var map in asset.actionMaps)
            {
                if (mapsToDisable.Contains(map.name))
                {
                    map.Disable();
                }
                else
                {
                    map.Enable();
                }
            }
        }

#endif

        public static void OnLoadingStart()
        {
#if UNITY_INPUT
            SetEnabledActionMaps(InputSystem.actions, _loadingActionMaps);
            foreach (var playerInput in _playerInputs)
            {
                SetEnabledActionMaps(playerInput.actions, _loadingActionMaps);
            }
#endif
        }

        public static void OnLoadingEnd()
        {
#if UNITY_INPUT
            SetDisabledActionMaps(InputSystem.actions, _loadingActionMaps);
            foreach (var playerInput in _playerInputs)
            {
                SetDisabledActionMaps(playerInput.actions, _loadingActionMaps);
            }
#endif
        }

        /// <summary>
        /// Adds a player control scheme to the current schemes list.
        /// </summary>
        /// <param name="controlScheme">The control scheme to add.</param>
        /// <remarks>
        /// This method serves as a backup for when the Unity Input System is not loaded.
        /// It is deprecated and should not be used in new code.
        /// </remarks>
        [Obsolete(
            "This method is deprecated. It only serves as a backup for when the Unity Input System is not loaded.")]
        public static void AddPlayerControlScheme(InputIconControlScheme controlScheme)
        {
            _currentSchemes.Add(controlScheme);
            OnControlSchemeChange?.Invoke(controlScheme);
        }

        public static void UpdateControlScheme(InputIconControlScheme newScheme, int inputIndex,
            bool forceInvoke = false)
        {
            InputIconControlScheme currentScheme = _currentSchemes[inputIndex];
            if (currentScheme != newScheme || forceInvoke)
            {
                OnControlSchemeChange?.Invoke(newScheme);
            }

            _currentSchemes[inputIndex] = newScheme;
        }
    }
}
