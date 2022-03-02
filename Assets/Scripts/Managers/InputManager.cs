using NoZ;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using NoZ.Zisle.UI;

namespace NoZ.Zisle
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Global")]
        [SerializeField] private InputActionAsset _inputActions = null;

        [Header("UI")]
        [SerializeField] private InputActionReference _uiClose = null;
        [SerializeField] private InputActionReference _uiDebug = null;

        [Header("Player")]
        [SerializeField] private float _playerYaw = 180.0f;
        [SerializeField] private InputActionReference _playerMenu = null;
        [SerializeField] private InputActionReference _playerMove = null;
        [SerializeField] private InputActionReference _playerMoveLeft = null;
        [SerializeField] private InputActionReference _playerMoveRight = null;
        [SerializeField] private InputActionReference _playerMoveUp = null;
        [SerializeField] private InputActionReference _playerMoveDown = null;
        [SerializeField] private InputActionReference _playerPrimary = null;
        [SerializeField] private InputActionReference _playerSecondary = null;
        [SerializeField] private InputActionReference _playerBuild = null;
        [SerializeField] private InputActionReference _playerZoom = null;
        [SerializeField] private InputActionReference _playerLook = null;

        [Header("Gamepad")]
        [SerializeField] private float _gamepadZoomSpeedMin = 0.1f;
        [SerializeField] private float _gamepadZoomSpeedMax = 1.0f;

        private bool _gamepad = false;

        /// <summary>
        /// Event fired when the menu button is pressed by the player
        /// </summary>
        public event Action onPlayerMenu;

        /// <summary>
        /// Event fired when the close button is pressed in a user interface
        /// </summary>
        public event Action OnUIClose;

        public event Action<bool> OnGamepadChanged;

        public event Action<PlayerButton> OnPlayerButton;

        public event Action<bool> OnPlayerBuild;

        public event Action<float> OnPlayerZoom;


        /// <summary>
        /// True if there is an active gamepad
        /// </summary>
        public bool HasGamepad
        {
            get => _gamepad;
            set
            {
                if (_gamepad == value)
                    return;

                _gamepad = value;
                OnGamepadChanged?.Invoke(_gamepad);
            }
        }

        public Vector3 PlayerLook 
        {
            get
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var ray = PlayerLookRay;
                plane.Raycast(ray, out var hit);
                return ray.origin + ray.direction * hit;
            }
        }

        public Ray PlayerLookRay => Camera.main.ScreenPointToRay(_playerLook.action.ReadValue<Vector2>());

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _playerPrimary.action.started += (ctx) => OnPlayerButtonStart(PlayerButton.Primary);
            _playerPrimary.action.canceled += (ctx) => OnPlayerButtonStop(PlayerButton.Primary);
            _playerBuild.action.started += (ctx) => OnPlayerBuild?.Invoke(ctx.action.activeControl.device is Gamepad);
            _playerMenu.action.started += (ctx) => onPlayerMenu?.Invoke();
            _playerSecondary.action.started += (ctx) => OnPlayerButtonStart(PlayerButton.Secondary);
            _playerSecondary.action.canceled += (ctx) => OnPlayerButtonStop(PlayerButton.Secondary);

            //_debugMenu.action.started += (ctx) => onDebugMenu?.Invoke();
            _uiClose.action.started += (ctx) => OnUIClose?.Invoke();
            _uiDebug.action.started += (ctx) => UIManager.Instance.ToggleDebug();
            _uiDebug.action.Enable();

            //_playerZoom.action.performed += (ctx) => OnPlayerZoom?.Invoke(ctx.ReadValue<float>());

            //_debugMenu.action.Enable();

            UpdateGamepad();
            InputSystem.onDeviceChange += (d, c) => UpdateGamepad();

            Options.LoadBindings(_inputActions);
        }

        private bool _playerButtonPressed = false;
        private PlayerButton _playerButton;

        private void OnPlayerButtonStart(PlayerButton button)
        {
            _playerButton = button;
            _playerButtonPressed = true;
            OnPlayerButton?.Invoke(button);
        }

        private void OnPlayerButtonStop(PlayerButton button)
        {
            if (_playerButton != button)
                return;

            _playerButtonPressed = false;
        }

        private void FixedUpdate()
        {
            if(_playerButtonPressed)
                OnPlayerButton?.Invoke(_playerButton);
        }

        public void EnableMenuActions(bool enable = true)
        {
            if (enable)
            {
                _uiClose.action.Enable();                
            }
            else
            {
                _uiClose.action.Disable();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
        }

        private void Update()
        {
            if(_playerZoom.action.enabled && _playerZoom.action.activeControl != null)
            {
                var zoom = _playerZoom.action.ReadValue<float>();
                if (zoom < -float.Epsilon || zoom >= float.Epsilon)
                {
                    if (_playerZoom.action.activeControl.device is Gamepad)
                        zoom *= Mathf.Lerp(_gamepadZoomSpeedMin, _gamepadZoomSpeedMax, Options.GamepadZoomSpeed);

                    OnPlayerZoom?.Invoke(zoom);
                }
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
                _playerZoom.action.Enable();
                _playerMove.action.Enable();
                _playerMoveLeft.action.Enable();
                _playerMoveRight.action.Enable();
                _playerMoveUp.action.Enable();
                _playerMoveDown.action.Enable();
                _playerMenu.action.Enable();
                _playerPrimary.action.Enable();
                _playerSecondary.action.Enable();
                _playerLook.action.Enable();
                _playerBuild.action.Enable();
            }
            else
            {
                _playerZoom.action.Disable();
                _playerMove.action.Disable();
                _playerMoveLeft.action.Disable();
                _playerMoveRight.action.Disable();
                _playerMoveUp.action.Disable();
                _playerMoveDown.action.Disable();
                _playerMenu.action.Disable();
                _playerPrimary.action.Disable();
                _playerSecondary.action.Disable();
                _playerLook.action.Disable();
                _playerBuild.action.Disable();
            }
        }

        /// <summary>
        /// Read the current value of player move
        /// </summary>
        public Vector3 PlayerMove 
        {
            get 
            {
                var keyboardMove = new Vector2 (
                    -1.0f * _playerMoveLeft.action.ReadValue<float>() + _playerMoveRight.action.ReadValue<float>(),
                    _playerMoveUp.action.ReadValue<float>() + -1.0f * _playerMoveDown.action.ReadValue<float>()).normalized;

                var gamepadMove = _playerMove.action.ReadValue<Vector2>();
                var move = gamepadMove.sqrMagnitude > keyboardMove.sqrMagnitude ? gamepadMove : keyboardMove;

                return Quaternion.Euler(0.0f, CameraManager.Instance.IsometricYaw + _playerYaw, 0.0f) * move.ToVector3XZ();
            }
        }

        public void PerformInteractiveRebinding(string actionMap, string actionName, int bindingIndex, bool gamepad = false, Action onComplete = null)
        {
            var map = _inputActions.FindActionMap(actionMap);
            if (map == null)
                throw new ArgumentException("actionMap");

            var action = map.FindAction(actionName);
            if (action == null)
                throw new ArgumentException("actionName");

            var operation = action.PerformInteractiveRebinding(bindingIndex);
            operation.WithExpectedControlType<AxisControl>();
            operation.WithExpectedControlType<ButtonControl>();
            if (gamepad)
                operation.WithControlsHavingToMatchPath("<Gamepad>")
                    .WithControlsExcluding("<Gamepad>/leftstick")
                    .WithControlsExcluding("<Gamepad>/rightstick")
                    .WithControlsExcluding("<Gamepad>/select")
                    .WithControlsExcluding("<Gamepad>/start");
            else
                operation.WithControlsHavingToMatchPath("<Keyboard>")
                    .WithControlsHavingToMatchPath("<Mouse>");
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

        private void UpdateGamepad() =>
            HasGamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
    }
}
