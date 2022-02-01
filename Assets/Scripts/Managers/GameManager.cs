using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoZ;
using System;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;

        public override void Initialize()
        {
            base.Initialize();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;
        }

        private void OnPlayerMenu()
        {
            UIManager.Instance.ShowGameMenu();
        }

        public void StopGame()
        {
            // TODO: stop multiplayer game
            
            UIManager.Instance.ShowTitle();
        }

        public void Resume ()
        {
            InputManager.Instance.EnableMenuActions(false);
            InputManager.Instance.EnablePlayerActions(true);
        }

        public void Pause ()
        {
            InputManager.Instance.EnableMenuActions(true);
            InputManager.Instance.EnablePlayerActions(false);
        }
    }
}

