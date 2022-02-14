using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class ConfirmPopupController : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<ConfirmPopupController, UxmlTraits> { }

        private Action _onYes;
        private Action _onNo;
        private Action _onCancel;
        private TextElement _message;
        private Button _yes;
        private Button _no;
        private Button _cancel;

        public string Message { set => _message.text = value; }
        public string Yes { set => _yes.text = value ?? "Yes"; }
        public string No { set => _no.text = value ?? "No"; }
        public string Cancel { set => _cancel.text = value ?? "Cancel"; }
        public Action OnYes { set => _onYes = value; }
        public Action OnNo { set => _onNo = value; }
        public Action OnCancel { set => _onCancel = value; }

        public override void Initialize ()
        {
            base.Initialize();

            _message = this.Q<Label>("message");
            _yes = this.Q<Button>("yes");
            _no = this.Q<Button>("no");
            _cancel = this.Q<Button>("cancel");

            _yes = this.Q<Button>("yes");
            _yes.BindClick(OnYesButton);

            _no = this.Q<Button>("no");
            _no.BindClick(OnNoButton);

            _cancel = this.Q<Button>("cancel");
            _cancel.BindClick(OnCancelButton);
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

            _cancel.EnableInClassList("hidden", _onCancel == null);
            _yes.Focus();
        }
    }
}
