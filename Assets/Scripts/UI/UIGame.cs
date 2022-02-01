using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIGame : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIGame, UxmlTraits> { }

        public override bool BlurBackground => false;

        private Label _joinCode;

        public override void Initialize()
        {
            base.Initialize();

            _joinCode = this.Q<Label>("joincode");
        }

        public override void Show()
        {
            base.Show();

            _joinCode.text = MultiplayerManager.Instance.JoinCode;
        }

        public override void OnAfterTransitionIn()
        {
            GameManager.Instance.Resume();
        }

        public override void OnBeforeTransitionOut()
        {
            GameManager.Instance.Pause();
        }
    }
}
