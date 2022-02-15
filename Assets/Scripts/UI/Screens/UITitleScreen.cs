using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Zisle.UI;

namespace NoZ.Zisle.UI
{
    public class UITitleScreen : UIScreen
    {
        private VisualElement _solo = null;

        public override void OnShow ()
        {
            base.OnShow();

            _solo = BindClick("solo", OnSolo);
            BindClick("cooperative", OnMultiplayer);
            BindClick("options", OnOptions);
            BindClick("quit", OnQuit);
        }

        private void OnMultiplayer() => UIManager.Instance.ShowMultiplayer();
        private void OnOptions() => UIManager.Instance.ShowOptions();

        private void OnSolo()
        {
            GameManager.Instance.MaxPlayers = 1;
            UIManager.Instance.JoinLobby("127.0.0.1", true);
        }

        private void OnQuit()
        {
            UIManager.Instance.Confirm(title:"quit?", message:"quit-desktop", onYes: () =>
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

            _solo.Focus();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnQuit();
        }
    }
}
