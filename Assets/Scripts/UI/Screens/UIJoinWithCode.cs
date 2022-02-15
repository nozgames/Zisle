using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIJoinWithCode : UIScreen
    {
        private TextField _joinCode;
        private VisualElement _join;

        protected override void Awake()
        {
            base.Awake();

            Q<Panel>("panel").OnClose(OnBack);

            _joinCode = Q<TextField>("join-code");
            _joinCode.maxLength = 6;
            _joinCode.RegisterValueChangedCallback(OnValueChanged);

            _join = Q("join");
            _join.BindClick(OnJoin);
            _join.SetEnabled(false);
        }

        protected override void OnShow()
        {
            base.OnShow();

            _joinCode.value = "";
        }

        protected override void OnLateShow()
        {
            base.OnLateShow();
            _joinCode.Focus();
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

        private void OnBack() => UIManager.Instance.ShowMultiplayer();
        private void OnJoin() => UIManager.Instance.JoinLobby(_joinCode.text);
    }
}
