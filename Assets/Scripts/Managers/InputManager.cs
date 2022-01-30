using NoZ;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoZ.Zisle
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Global")]
        [SerializeField] private InputActionAsset _inputActions = null;
        [SerializeField] private InputActionReference _debugMenu = null;

        [Header("UI")]
        [SerializeField] private InputActionReference _uiMenu = null;
        [SerializeField] private InputActionReference _uiClose = null;

        [Header("Player")]
        [SerializeField] private InputActionReference _playerMenu = null;
        [SerializeField] private InputActionReference _playerMoveLeft = null;
        [SerializeField] private InputActionReference _playerMoveRight = null;
        [SerializeField] private InputActionReference _playerMoveUp = null;
        [SerializeField] private InputActionReference _playerMoveDown = null;
        [SerializeField] private InputActionReference _playerLook = null;

        /// <summary>
        /// Event fired when the menu button is pressed by the player
        /// </summary>
        public event Action onPlayerMenu;

        /// <summary>
        /// Event fired when the debug menu button is pressed 
        /// </summary>
        public event Action onDebugMenu;

        /// <summary>
        /// Event fired when the close button is pressed in a user interface
        /// </summary>
        public event Action onUIClose;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _playerMenu.action.started += (ctx) => onPlayerMenu?.Invoke();
            //_debugMenu.action.started += (ctx) => onDebugMenu?.Invoke();
            //_uiClose.action.started += (ctx) => onUIClose?.Invoke();

            //_debugMenu.action.Enable();

            Options.LoadBindings(_inputActions);
        }

        public void EnableMenuActions(bool enable = true)
        {
            if (enable)
            {
                //_uiClose.action.Enable();
            }
            else
            {
                //_uiClose.action.Disable();
            }
        }

        /// <summary>
        /// Enable or disable the player actions
        /// </summary>
        /// <param name="enable"></param>
        public void EnablePlayerActions(bool enable = true)
        {
            if (enable)
            {
                _playerMoveLeft.action.Enable();
                _playerMoveRight.action.Enable();
                _playerMoveUp.action.Enable();
                _playerMoveDown.action.Enable();
                _playerMenu.action.Enable();
            }
            else
            {
                _playerMoveLeft.action.Disable();
                _playerMoveRight.action.Disable();
                _playerMoveUp.action.Disable();
                _playerMoveDown.action.Disable();
                _playerMenu.action.Disable();
            }
        }

        /// <summary>
        /// Read the current value of player move
        /// </summary>
        public Vector2 playerMove => new Vector2(
            -1.0f * _playerMoveLeft.action.ReadValue<float>() + _playerMoveRight.action.ReadValue<float>(),
            _playerMoveUp.action.ReadValue<float>() + -1.0f * _playerMoveDown.action.ReadValue<float>()).normalized;

        /// <summary>
        /// Read the current value of player look
        /// </summary>
        public Vector2 playerLook => Instance?._playerLook.action.ReadValue<Vector2>() ?? Vector2.zero;

        public void PerformInteractiveRebinding(string actionMap, string actionName, int bindingIndex, Action onComplete = null)
        {
            var map = _inputActions.FindActionMap(actionMap);
            if (map == null)
                throw new ArgumentException("actionMap");

            var action = map.FindAction(actionName);
            if (action == null)
                throw new ArgumentException("actionName");

            var operation = action.PerformInteractiveRebinding(bindingIndex);
            operation.OnComplete((r) =>
            {
                onComplete?.Invoke();
                operation.Dispose();
                Options.SaveBindings(_inputActions);
            });
            operation.OnCancel((r) =>
            {
                onComplete?.Invoke();
                operation.Dispose();
            });
            operation.Start();
        }

        public string GetBinding(string actionMap, string actionName, int bindingIndex)
        {
            var map = _inputActions.FindActionMap(actionMap);
            if (map == null)
                throw new ArgumentException("actionMap");

            var action = map.FindAction(actionName);
            if (action == null)
                throw new ArgumentException("actionName");

            return action.bindings[bindingIndex].ToDisplayString();
        }
    }
}
