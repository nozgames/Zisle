using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class CooperativeController : UIController
    {
        public new class UxmlFactory : UxmlFactory<CooperativeController, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBack).Focus();
            BindClick("join", OnJoin);
            BindClick("host", OnHost);
            BindClick("continue", OnContinue);
        }

        private void OnBack() => UIManager.Instance.ShowTitle();
        private void OnJoin() => UIManager.Instance.ShowCooperativeJoin();
        private void OnHost()
        {
            // TODO: game creation options at some point if there are any
            UIManager.Instance.ShowConnecting();
        }

        private void OnContinue() => UIManager.Instance.ShowTitle();
    }
}
