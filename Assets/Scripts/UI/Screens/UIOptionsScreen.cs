using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIOptionsScreen : UIScreen
    {
        private string ResolutionToString(Resolution r) => $"{r.width} x {r.height} @ {r.refreshRate}Hz";

        public Action OnBack = null;

        protected override void Awake ()
        {
            base.Awake();

            BindClick("back",OnBackInternal).Focus();
            BindClick("keyboard-controls", () => UIManager.Instance.ShowKeyboardControls());
            BindClick("gamepad-controls", () => UIManager.Instance.ShowGamepadControls());

            var resolutions = this.Q<DropdownField>("resolutions");
            resolutions.choices = Screen.resolutions.Select(r => ResolutionToString(r)).ToList();
            resolutions.value = ResolutionToString(UnityEngine.Screen.currentResolution);
            resolutions.RegisterValueChangedCallback((e) =>
            {
                var resolution = UnityEngine.Screen.resolutions[resolutions.choices.IndexOf(e.newValue)];
                UnityEngine.Screen.SetResolution(resolution.width, resolution.height, UnityEngine.Screen.fullScreenMode, resolution.refreshRate);
            });

            var fullscreen = this.Q<Toggle>("fullscreen");
            fullscreen.value = UnityEngine.Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            fullscreen.RegisterValueChangedCallback((e) => UnityEngine.Screen.fullScreenMode = e.newValue ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            var screenShake = this.Q<Toggle>("screen-shake");
            screenShake.value = Options.ScreenShake;
            screenShake.RegisterValueChangedCallback(e => Options.ScreenShake = e.newValue);

            var soundVoume = this.Q<Slider>("sound-volume");
            soundVoume.lowValue = 0.0f;
            soundVoume.highValue = 1.0f;
            soundVoume.value = Options.SoundVolume;
            soundVoume.RegisterValueChangedCallback(e => Options.SoundVolume = e.newValue);

            var musicVolume = this.Q<Slider>("music-volume");
            musicVolume.lowValue = 0.0f;
            musicVolume.highValue = 1.0f;
            musicVolume.value = Options.MusicVolume;
            musicVolume.RegisterValueChangedCallback(e => Options.MusicVolume = e.newValue);
        }

        private void OnBackInternal()
        {
            if (OnBack == null)
                UIManager.Instance.ShowTitle();
            else
                OnBack?.Invoke();
        }

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBackInternal();
        }
    }
}