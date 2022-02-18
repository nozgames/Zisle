using NoZ.Zisle.UI;
using System;
using UnityEngine;

namespace NoZ.Zisle.UI
{
    public class UIGameOverScreen : UIScreen
    {
        protected override void Awake()
        {
            base.Awake();

            BindClick("ok", OnQuit);
        }

        private void OnQuit()
        {
            GameManager.Instance.StopGameAsync();

            UIManager.Instance.ShowLobby();
        }

        protected override void OnShow()
        {
            base.OnShow();

            var panel = Q<Panel>("panel");
            panel.Title = Game.Instance.IsVictory ? "victory" : "defeat";
            panel.EnableInClassList("victory", Game.Instance.IsVictory);
        }
    }
}
