using NoZ.Modules;
using System.Collections;
using UnityEngine;
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
            UIManager.Instance.PlayClickSound();
            OnBack();
        }

        private void OnBack() => UIManager.Instance.ShowTitle();
        private void OnJoin() => UIManager.Instance.ShowCooperativeJoin();
        private void OnHost()
        {
            // TODO: game creation options at some point if there are any
            UIManager.Instance.ShowConnecting();

            MultiplayerManager.Instance.OnConnected -= OnConnected;
            MultiplayerManager.Instance.OnConnected += OnConnected;
            MultiplayerManager.Instance.Host();
        }

        private void OnJoinLocal()
        {
            UIManager.Instance.ShowConnecting();

            MultiplayerManager.Instance.OnConnected -= OnConnected;
            MultiplayerManager.Instance.OnConnected += OnConnected;
            MultiplayerManager.Instance.JoinLocal();
        }

        private void OnHostLocal()
        {
            // TODO: game creation options at some point if there are any
            UIManager.Instance.ShowConnecting();

            MultiplayerManager.Instance.OnConnected -= OnConnected;
            MultiplayerManager.Instance.OnConnected += OnConnected;
            MultiplayerManager.Instance.HostLocal();
        }

        private void OnContinue() => UIManager.Instance.ShowTitle();
        private void OnConnected()
        {
            IEnumerator Delay ()
            {
                yield return new WaitForSeconds(2.0f);
                UIManager.Instance.ShowGame();
            }

            GameManager.Instance.StartCoroutine(Delay());
        } 
    }
}
