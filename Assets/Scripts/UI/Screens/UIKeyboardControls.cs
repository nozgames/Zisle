using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIKeyboardControls : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIKeyboardControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBackInternal).Focus();
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
