using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIMultiplayerScreen : UIScreen
    {
        public override bool MainMenuOnly => true;

        protected override void Awake()
        {
            base.Awake();

            Q<Panel>("panel").OnClose(OnNavigationBack);

            Q("join").BindClick(OnJoin).Focus();
            Q("host").BindClick(OnHost);
            Q("continue").BindClick(OnContinue);
            Q("host-local").BindClick(OnHostLocal);
            Q("join-local").BindClick(OnJoinLocal);
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClickSound();
            OnBack();
        }

        private void OnBack() => UIManager.Instance.ShowTitle();
        private void OnJoin()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.ShowJoinWithCode();
        }

        private void OnHost()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.JoinLobby(null,true);
        }

        private void OnJoinLocal()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.ShowJoinWithIP();
        }

        private void OnHostLocal()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.JoinLobby("127.0.0.1",true);
        }

        private void OnContinue() => UIManager.Instance.ShowTitle();
    }
}
