using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class CooperativeController : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<CooperativeController, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            this.Q("back").BindClick(OnBack).Focus();
            this.Q("join").BindClick(OnJoin);
            this.Q("host").BindClick(OnHost);
            this.Q("continue").BindClick(OnContinue);
            this.Q("host-local").BindClick(OnHostLocal);
            this.Q("join-local").BindClick(OnJoinLocal);
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBack();
        }

        private void OnBack() => UIManager.Instance.ShowTitle();
        private void OnJoin()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.ShowCooperativeJoin();
        }

        private void OnHost()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.JoinLobby(null,true);
        }

        private void OnJoinLocal()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.JoinLobby("127.0.0.1");
        }

        private void OnHostLocal()
        {
            GameManager.Instance.MaxPlayers = 2;
            UIManager.Instance.JoinLobby("127.0.0.1",true);
        }

        private void OnContinue() => UIManager.Instance.ShowTitle();
    }
}
