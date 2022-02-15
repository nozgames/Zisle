using Unity.Netcode;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIGameMenu : UIScreen
    {
        private VisualElement _resume;

        protected override void Awake()
        {
            base.Awake();

            _resume = Q("resume").BindClick(OnResume);
            Q("quit").BindClick(OnQuit);
            Q("options").BindClick(OnOptions);
        }

        protected override void OnLateShow()
        {
            base.OnLateShow();

            _resume.Focus();
        }

        private void OnOptions()
        {
            UIManager.Instance.ShowOptions(() => UIManager.Instance.ShowGameMenu());
        }

        private void OnQuit()
        {
            UIManager.Instance.Confirm(
                title: "quit?".Localized(),
                message: (GameManager.Instance.IsSolo ? "confirm-quit-solo" : (NetworkManager.Singleton.IsHost ? "confirm-close-lobby" : "confirm-leave-lobby")).Localized(), 
                onNo: () => UIManager.Instance.ShowGame(), 
                onYes: () => UIManager.Instance.ShowMainMenu());
        }

        private void OnResume()
        {
            UIManager.Instance.ShowGame();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnResume();
        }
    }
}
