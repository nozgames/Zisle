using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIGame : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIGame, UxmlTraits> { }

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
            InputManager.Instance.EnablePlayerActions(true);
        }

        public override void OnBeforeTransitionOut()
        {
            InputManager.Instance.EnablePlayerActions(false);
        }
    }
}
