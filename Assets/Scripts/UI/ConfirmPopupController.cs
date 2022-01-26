using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class ConfirmPopupController : UIController
    {
        public new class UxmlFactory : UxmlFactory<ConfirmPopupController, UxmlTraits> { }

        private Action _onYes;
        private Action _onNo;
        private TextElement _title;
        private TextElement _message;
        private TextElement _yes;
        private TextElement _no;

        public override void Initialize ()
        { 
            _title = this.Q<Label>("title");
            _message = this.Q<Label>("message");
            _yes = this.Q<Label>("yes");
            _no = this.Q<Label>("no");

            _yes.AddManipulator(new Clickable(OnYes));
            _no.AddManipulator(new Clickable(OnNo));
        }

        public void Show (string title, string message, string yes=null, string no=null, Action onYes=null, Action onNo = null)
        {
            Show();

            _title = this.Q<Label>("title");
            _message = this.Q<Label>("message");
            _yes = this.Q<Label>("yes");
            _no = this.Q<Label>("no");


            _title.text = title;
            _message.text = message;
            _yes.text = yes ?? "Yes";
            _no.text = no ?? "No";
            _onYes = onYes;
            _onNo = onNo;
        }

        private void OnYes(EventBase e) => PerformAction(_onYes);

        private void OnNo(EventBase e) => PerformAction(_onNo);

        private void PerformAction (Action action)
        {
            _onYes = null;
            _onNo = null;
            Hide();
            action?.Invoke();
        }
    }
}
