using NoZ;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoZ.Zisle
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Global")]
        [SerializeField] private InputActionReference _debugMenu = null;

        [Header("UI")]
        [SerializeField] private InputActionReference _uiMenu = null;
        [SerializeField] private InputActionReference _uiClose = null;

        [Header("Player")]
        [SerializeField] private InputActionReference _playerMenu = null;
        [SerializeField] private InputActionReference _playerMove = null;
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

            //_playerMenu.action.started += (ctx) => onPlayerMenu?.Invoke();
            //_debugMenu.action.started += (ctx) => onDebugMenu?.Invoke();
            //_uiClose.action.started += (ctx) => onUIClose?.Invoke();

            //_debugMenu.action.Enable();
        }

        public void EnableMenuActions (bool enable=true)
        {
            if(enable)
            {
                _uiClose.action.Enable();
            }
            else
            {
                _uiClose.action.Disable();
            }            
        }

        /// <summary>
        /// Enable or disable the player actions
        /// </summary>
        /// <param name="enable"></param>
        public void EnablePlayerActions (bool enable=true)
        {
            if (enable)
            {
                _playerMove.action.Enable();
                //_playerLook.action.Enable();
                //_playerMenu.action.Enable();
            }
            else
            {
                _playerMove.action.Disable();
                //_playerLook.action.Disable();
                //_playerMenu.action.Disable();
            }
        }

        /// <summary>
        /// Read the current value of player move
        /// </summary>
        public Vector2 playerMove => Instance?._playerMove.action.ReadValue<Vector2>() ?? Vector2.zero;

        /// <summary>
        /// Read the current value of player look
        /// </summary>
        public Vector2 playerLook => Instance?._playerLook.action.ReadValue<Vector2>() ?? Vector2.zero;

    }
}
