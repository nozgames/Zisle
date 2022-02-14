using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIGamepadControls : ScreenElement
    {
        public new class UxmlFactory : UxmlFactory<UIGamepadControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            this.Q("back").BindClick(OnBackInternal ).Focus();

            var zoomSpeed = this.Q<Slider>("zoom-speed");
            zoomSpeed.lowValue = 0.0f;
            zoomSpeed.highValue = 1.0f;
            zoomSpeed.value = Options.GamepadZoomSpeed;
            zoomSpeed.RegisterValueChangedCallback((v) => Options.GamepadZoomSpeed = v.newValue);
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
