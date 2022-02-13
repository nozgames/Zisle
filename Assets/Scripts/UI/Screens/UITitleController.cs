using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UITitleController : UIController
    {
        public new class UxmlFactory : UxmlFactory<UITitleController, UxmlTraits> { }

        public override void Initialize()
        {
            BindClick("solo-button", OnSolo).Focus();
            BindClick("coop-button", OnCooperative);
            BindClick("options-button", OnOptions);
            BindClick("quit-button", OnQuit);
        }

        private void OnSolo()
        {
            GameManager.Instance.MaxPlayers = 1;
            UIManager.Instance.JoinLobby("127.0.0.1", true);
        }

        private void OnCooperative()
        {
            UIManager.Instance.ShowCooperative();
        }

        private void OnOptions() => UIManager.Instance.ShowOptions();

        private void OnQuit()
        {
            UIManager.Instance.ShowConfirmationPopup("Quit to desktop?", onYes: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            },
            onNo:() => UIManager.Instance.ShowTitle()
            );
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            this.Q("solo-button").Focus();
        }
    }
}
