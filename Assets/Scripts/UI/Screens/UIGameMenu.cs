using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIGameMenu : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<UIGameMenu, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            this.Q("resume").BindClick(OnResume);
            this.Q("quit").BindClick(OnQuit);
            this.Q("options").BindClick(OnOptions);
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
                onYes: () => UIManager.Instance.ShowMainMenu());
        }

        private void OnResume()
        {
            UIManager.Instance.ShowGame();
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            this.Q("resume").Focus();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnResume();
        }
    }
}
