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

        public override void OnAfterTransitionIn()
        {
            base.OnAfterTransitionIn();

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
                onNo: () => UIManager.Instance.ShowGameMenu(), 
                onYes: () => UIManager.Instance.ShowTitle());
        }

        private void OnResume()
        {
            UIManager.Instance.ShowGame();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClickSound();
            OnResume();
        }
    }
}
