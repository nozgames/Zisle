using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class ConfirmPopupController : UIController
    {
        public new class UxmlFactory : UxmlFactory<ConfirmPopupController, UxmlTraits> { }

        private Action _onYes;
        private Action _onNo;
        private TextElement _message;
        private Button _yes;
        private Button _no;

        public string Message { set => _message.text = value; }
        public string Yes { set => _yes.text = value ?? "Yes"; }
        public string No { set => _no.text = value ?? "No"; }
        public Action OnYes { set => _onYes = value; }
        public Action OnNo { set => _onNo = value; }

        public override void Initialize ()
        {
            base.Initialize();

            _message = this.Q<Label>("message");
            _yes = this.Q<Button>("yes");
            _no = this.Q<Button>("no");

            _yes.clicked += OnYesButton;
            _no.clicked += OnNoButton;
        }

        private void OnYesButton() => PerformAction(_onYes);

        private void OnNoButton() => PerformAction(_onNo);

        private void PerformAction (Action action)
        {
            _onYes = null;
            _onNo = null;
            Hide();
            action?.Invoke();
        }

        public override void Show()
        {
            base.Show();

            _yes.Focus();
        }
    }
}
