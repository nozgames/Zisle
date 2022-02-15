using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class ConfirmationScreen : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<ConfirmationScreen, UxmlTraits> { }

        private Action _onYes;
        private Action _onNo;
        private Action _onCancel;
        private RaisedButton _yes;
        private RaisedButton _no;
        private RaisedButton _cancel;
        private Panel _panel;
        private Label _message;

        public string Title { set => _panel.Title = value; }
        public string Message { set => _message.text = value; }
        public string Yes { set => _yes.text = value ?? "yes".Localized(); }
        public string No { set => _no.text = value ?? "no".Localized(); }
        public string Cancel { set => _cancel.text = value ?? "cancel".Localized(); }
        public Action OnYes { set => _onYes = value; }
        public Action OnNo { set => _onNo = value; }
        public Action OnCancel { set => _onCancel = value; }

        public ConfirmationScreen()
        {
            _panel = this.Add<Panel>();
            _message = _panel.AddItem<Label>().AddClass("message");
            _message.text = "This is some placeholder message text";

            var buttons = _panel.AddItem<VisualElement>().AddClass("buttons");
            _yes = buttons.Add<RaisedButton>().SetColor(RaisedButtonColor.Blue).BindClick(OnYesButton);
            _no = buttons.Add<RaisedButton>().SetColor(RaisedButtonColor.Orange).AddClass("gap").BindClick(OnNoButton);
            _cancel = buttons.Add<RaisedButton>().SetColor(RaisedButtonColor.Orange).AddClass("gap").BindClick(OnCancelButton);

            _panel.Title = "Confirm";

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
            Hide();
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
