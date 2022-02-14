using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIKeyboardControls : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<UIKeyboardControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            this.Q("back").BindClick(OnBackInternal).Focus();
        }

        private void OnBackInternal()
        {
            UIManager.Instance.ShowOptions();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBackInternal();
        }
    }
}
