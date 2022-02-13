using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Zisle.UI;

namespace NoZ.Zisle
{
    public class TitleScreen : UIController
    {
        public new class UxmlFactory : UxmlFactory<TitleScreen, UxmlTraits> { }

        private VisualElement _solo = null;

        public TitleScreen()
        {
            AddToClassList("centered-vertical");
            AddToClassList("vertical");

            this.Add<VisualElement>(className: "logo");

            var panel = this.Add<Panel>().AddClass("centered-vertical");
            _solo = panel.Add<RaisedButton>().SetColor(RaisedButtonColor.Blue).LocalizedText("solo").BindClick(OnSolo);
            panel.Add<RaisedButton>().SetColor(RaisedButtonColor.Blue).LocalizedText("cooperative").BindClick(OnCooperative);
            panel.Add<RaisedButton>().SetColor(RaisedButtonColor.Blue).LocalizedText("options").BindClick(OnOptions);
            panel.Add<RaisedButton>().SetColor(RaisedButtonColor.Orange).LocalizedText("quit").BindClick(OnQuit);
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

            _solo.Focus();
        }
    }
}
