using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIGameMenu : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIGameMenu, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("resume", OnResume);
            BindClick("quit", OnQuit);
            BindClick("options", OnOptions);
        }

        private void OnOptions()
        {
            UIManager.Instance.ShowOptions(() => UIManager.Instance.ShowGameMenu());
        }

        private void OnQuit()
        {
            UIManager.Instance.ShowConfirmationPopup(
                message: "Are you sure you want to Quit?\nYou can resume your game later.", 
                onNo: () => UIManager.Instance.ShowGame(), 
                onYes: () => GameManager.Instance.StopGame());
        }

        private void OnResume()
        {
            UIManager.Instance.ShowGame();
        }

        public override void Show()
        {
            base.Show();

            this.Q("resume").Focus();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnResume();
        }
    }
}
