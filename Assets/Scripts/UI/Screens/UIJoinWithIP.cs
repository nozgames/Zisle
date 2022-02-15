using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIJoinWithIP : UIScreen
    {
        private TextField _ip;
        private VisualElement _join;

        protected override void Awake ()
        {
            base.Awake();

            Q<Panel>("panel").OnClose(OnBack);

            _ip = Q<TextField>("ip");
            _ip.value = "";
            _ip.maxLength = 15;
            _ip.RegisterValueChangedCallback(OnValueChanged);

            _join = Q("join");
            _join.BindClick(OnJoin);
            _join.SetEnabled(false);
        }

        protected override void OnShow()
        {
            base.OnShow();

            _ip.value = Options.LanIP;
            _ip.SelectAll();
        }

        protected override void OnLateShow()
        {
            base.OnLateShow();
            _ip.Focus();
        }

        private static Regex TextFilterRegex = new Regex(@"[^\d^\.]");
        private static Regex TextValidateRegex = new Regex(@"^(\d\d?\d?)\.(\d\d?\d?)\.(\d\d?\d?)\.(\d\d?\d?)$");

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            var filtered = TextFilterRegex.Replace(evt.newValue, "");
            if (filtered != evt.newValue)
                _ip.SetValueWithoutNotify(filtered);

            if(_join != null)
                _join.SetEnabled(TextValidateRegex.IsMatch(filtered));
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBack();
        }

        private void OnBack() => UIManager.Instance.ShowMultiplayer();
        private void OnJoin()
        {
            Options.LanIP = _ip.text;
            UIManager.Instance.JoinLobby(_ip.text);
        }
    }
}
