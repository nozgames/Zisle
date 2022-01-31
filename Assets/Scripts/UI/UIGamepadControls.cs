using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIGamepadControls : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIGamepadControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBackInternal ).Focus();

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
            UIManager.Instance.PlayClickSound();
            OnBackInternal();
        }
    }
}
