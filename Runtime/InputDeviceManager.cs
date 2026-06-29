using System;
using System.Collections.Generic;

#if UNITY_INPUT
using UnityEngine;
using UnityEngine.InputSystem;
#endif

namespace CupkekGames.Input
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

        // Single source of truth for "the action that dismisses a loading screen".
        // Must live in one of _loadingActionMaps (the only maps enabled during loading),
        // so a press-to-continue gate can never wait on a disabled action.
        private static string _loadingContinueActionName;
        public static string LoadingContinueActionName => _loadingContinueActionName;

        public static event Action<PlayerInput, int> OnPlayerAdded;
        public static event Action<PlayerInput, int> OnPlayerRemoved;
#endif
        private static List<InputIconControlScheme> _currentSchemes = new List<InputIconControlScheme>();
        public static List<InputIconControlScheme> CurrentSchemes => _currentSchemes;

        public static event Action<InputIconControlScheme> OnControlSchemeChange;

        // With "Enter Play Mode Without Domain Reload" these statics survive
        // between play sessions; player lists and event listeners from the
        // previous session would leak into the next one.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
#if UNITY_INPUT
            InputSystem.onActionChange -= OnActionChange;
            _playerInputs = new List<PlayerInput>();
            _escapeActionNames = new List<string>() { "UI/Cancel" };
            _loadingActionMaps = null;
            _loadingContinueActionName = null;
            OnPlayerAdded = null;
            OnPlayerRemoved = null;
#endif
            _currentSchemes = new List<InputIconControlScheme>();
            OnControlSchemeChange = null;
        }

#if UNITY_INPUT
        public static void OnEnable(List<string> escapeActionNames, List<string> loadingActionMaps,
            bool localMultiplayer, string loadingContinueActionName = "Loading/Submit")
        {
            _playerInputs = new List<PlayerInput>();
            _currentSchemes = new List<InputIconControlScheme>();
            _escapeActionNames = escapeActionNames;
            _loadingActionMaps = loadingActionMaps;
            _loadingContinueActionName = loadingContinueActionName;

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

#if UNITY_INPUT
        /// <summary>
        /// The continue actions for every active <see cref="PlayerInput"/>, resolved from
        /// <see cref="LoadingContinueActionName"/>. Because that name points into a loading
        /// action map (the only maps enabled during loading), the returned actions are
        /// guaranteed live — a press-to-continue gate can wait on them safely. Falls back to
        /// the project-wide <see cref="InputSystem.actions"/> when no PlayerInputs are registered.
        /// </summary>
        public static IEnumerable<InputAction> GetLoadingContinueActions()
        {
            if (string.IsNullOrEmpty(_loadingContinueActionName)) yield break;

            if (_playerInputs.Count == 0)
            {
                var shared = InputSystem.actions?.FindAction(_loadingContinueActionName);
                if (shared != null) yield return shared;
                yield break;
            }

            foreach (var playerInput in _playerInputs)
            {
                var action = playerInput.actions?.FindAction(_loadingContinueActionName);
                if (action != null) yield return action;
            }
        }
#endif

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
