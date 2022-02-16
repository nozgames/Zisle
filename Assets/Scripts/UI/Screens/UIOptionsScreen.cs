using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIOptionsScreen : UIScreen
    {
        private string ResolutionToString(Resolution r) => $"{r.width} x {r.height} @ {r.refreshRate}Hz";

        private VisualElement _generalContent = null;
        private VisualElement _gamepadContent = null;
        private VisualElement _keyboardContent = null;
        private VisualElement _generalTab = null;
        private VisualElement _gamepadTab = null;
        private VisualElement _keyboardTab = null;

        private Toggle _fullscreen;
        private Toggle _screenShake;
        private Slider _soundVoume;
        private Slider _musicVolume;
        private DropdownField _resolutions;

        public Action OnBack = null;

        protected override void Awake ()
        {
            base.Awake();

            _resolutions = this.Q<DropdownField>("resolutions");
            _resolutions.choices = Screen.resolutions.Select(r => ResolutionToString(r)).ToList();
            _resolutions.RegisterValueChangedCallback((e) =>
            {
                var resolution = Screen.resolutions[_resolutions.choices.IndexOf(e.newValue)];
                Screen.SetResolution(resolution.width, resolution.height, UnityEngine.Screen.fullScreenMode, resolution.refreshRate);
            });

            _fullscreen = this.Q<Toggle>("fullscreen");
            _fullscreen.RegisterValueChangedCallback((e) => Screen.fullScreenMode = e.newValue ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            _screenShake = this.Q<Toggle>("screen-shake");
            _screenShake.RegisterValueChangedCallback(e => Options.ScreenShake = e.newValue);

            _soundVoume = this.Q<Slider>("sound-volume");
            _soundVoume.lowValue = 0.0f;
            _soundVoume.highValue = 1.0f;
            _soundVoume.RegisterValueChangedCallback(e => Options.SoundVolume = e.newValue);

            _musicVolume = this.Q<Slider>("music-volume");
            _musicVolume.lowValue = 0.0f;
            _musicVolume.highValue = 1.0f;
            _musicVolume.RegisterValueChangedCallback(e => Options.MusicVolume = e.newValue);

            Q<Panel>("panel").OnClose(OnBackInternal);

            _generalContent = Q("general-content");
            _gamepadContent = Q("gamepad-content");
            _keyboardContent = Q("keyboard-content");

            _generalTab = BindClick("general-tab", OnGeneralTab);
            _generalTab.SetEnabled(false);

            _keyboardTab = BindClick("keyboard-tab", OnKeyboardTab);
            _gamepadTab = BindClick("gamepad-tab", OnGamepadTab);
        }

        private void OnKeyboardTab()
        {
            _generalContent.EnableInClassList(USS.Hidden, true);
            _gamepadContent.EnableInClassList(USS.Hidden, true);
            _keyboardContent.EnableInClassList(USS.Hidden, false);
            _generalTab.SetEnabled(true);
            _keyboardTab.SetEnabled(false);
            _gamepadTab.SetEnabled(true);
        }

        private void OnGamepadTab()
        {
            _generalContent.EnableInClassList(USS.Hidden, true);
            _gamepadContent.EnableInClassList(USS.Hidden, false);
            _keyboardContent.EnableInClassList(USS.Hidden, true);
            _generalTab.SetEnabled(true);
            _keyboardTab.SetEnabled(true);
            _gamepadTab.SetEnabled(false);
        }

        private void OnGeneralTab()
        {
            _generalContent.EnableInClassList(USS.Hidden, false);
            _gamepadContent.EnableInClassList(USS.Hidden, true);
            _keyboardContent.EnableInClassList(USS.Hidden, true);
            _generalTab.SetEnabled(false);
            _keyboardTab.SetEnabled(true);
            _gamepadTab.SetEnabled(true);
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

        protected override void OnShow()
        {
            base.OnShow();

            _fullscreen.value = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            _screenShake.value = Options.ScreenShake;
            _soundVoume.value = Options.SoundVolume;
            _musicVolume.value = Options.MusicVolume;

            if (!_fullscreen.value)
                _resolutions.value = ResolutionToString(new Resolution { width = Screen.width, height = Screen.height, refreshRate = Screen.currentResolution.refreshRate });
            else
                _resolutions.value = ResolutionToString(Screen.currentResolution);

            OnGeneralTab();
        }
    }
}
