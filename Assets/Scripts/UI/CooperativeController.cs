using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class CooperativeController : UIController
    {
        public new class UxmlFactory : UxmlFactory<CooperativeController, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBack).Focus();
            BindClick("join", OnJoin);
            BindClick("host", OnHost);
            BindClick("continue", OnContinue);

            BindClick("host-local", OnHostLocal);
            BindClick("join-local", OnJoinLocal);
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
