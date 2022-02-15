using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIConfirmationScreen : UIScreen
    {
        private Action _onYes;
        private Action _onNo;
        private Action _onCancel;
        private RaisedButton _yes;
        private RaisedButton _no;
        private RaisedButton _cancel;
        private Panel _panel;
        private Label _message;

        public string Title { set => _panel.Title = value.Localized(); }
        public string Message { set => _message.text = value.Localized(); }
        public string Yes { set => _yes.text = value ?? "yes".Localized(); }
        public string No { set => _no.text = value ?? "no".Localized(); }
        public string Cancel { set => _cancel.text = value ?? "cancel".Localized(); }
        public Action OnYes { set => _onYes = value; }
        public Action OnNo { set => _onNo = value; }
        public Action OnCancel { set => _onCancel = value; }

        protected override void OnEnable()
        {
            base.OnEnable();

            _panel = Q<Panel>("panel");
            _message = Q<Label>("message");
            _yes = BindClick<RaisedButton>("yes", OnYesButton);
            _no = BindClick<RaisedButton>("no", OnNoButton);
            _cancel = BindClick<RaisedButton>("cancel", OnCancelButton);

            Yes = null;
            No = null;
            Cancel = null;
        }

        private void OnYesButton() => PerformAction(_onYes);

        private void OnNoButton() => PerformAction(_onNo);

        private void OnCancelButton() => PerformAction(_onCancel);

        private void PerformAction (Action action)
        {
            _onYes = null;
            _onNo = null;
            _onCancel = null;
            action?.Invoke();
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            _message.EnableInClassList("hidden", string.IsNullOrEmpty(_message.text));
            _yes.EnableInClassList("hidden", _onYes == null);
            _no.EnableInClassList("hidden", _onNo == null);
            _cancel.EnableInClassList("hidden", _onCancel == null);

            _yes.Focus();
        }
    }
}
