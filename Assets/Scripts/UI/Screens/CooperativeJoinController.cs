using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class CooperativeJoinController : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<CooperativeJoinController, UxmlTraits> { }

        private TextField _joinCode;
        private VisualElement _join;

        public override void Initialize()
        {
            base.Initialize();

            var joinCode = this.Q<TextField>("join-code");
            joinCode.SetPlaceholderText("Join Code");
            joinCode.maxLength = 6;
            joinCode.RegisterValueChangedCallback(OnValueChanged);
            joinCode.Focus();
            _joinCode = joinCode;

            _join = this.Q("join");
            _join.BindClick(OnJoin);
            _join.SetEnabled(false);

            this.Q("back").BindClick(OnBack);
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            var upper = evt.newValue.ToUpper();
            if (upper != evt.newValue)
                _joinCode.SetValueWithoutNotify(upper);

            if(_join != null)
                _join.SetEnabled(upper.Length == 6);
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBack();
        }

        private void OnBack() => UIManager.Instance.ShowCooperative();
        private void OnJoin() => UIManager.Instance.JoinLobby(_joinCode.text);
    }
}
