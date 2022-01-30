using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoZ;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
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
    }
}

