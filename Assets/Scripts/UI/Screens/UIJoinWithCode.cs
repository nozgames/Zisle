using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIJoinWithCode : UIScreen
    {
        private TextField _joinCode;
        private VisualElement _join;

        public override void OnShow()
        {
            base.OnShow();

            Q<Panel>("panel").OnClose(OnBack);

            _joinCode = Q<TextField>("join-code");
            _joinCode.value = "";
            _joinCode.maxLength = 6;
            _joinCode.RegisterValueChangedCallback(OnValueChanged);
            _joinCode.Focus();

            _join = Q("join");
            _join.BindClick(OnJoin);
            _join.SetEnabled(false);
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
