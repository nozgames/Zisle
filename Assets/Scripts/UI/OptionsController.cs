using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class OptionsController : UIController
    {
        public new class UxmlFactory : UxmlFactory<OptionsController, UxmlTraits> { }

        private string ResolutionToString(Resolution r) => $"{r.width} x {r.height} @ {r.refreshRate}Hz";

        public OptionsController()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            this.Q<Button>("back").clicked += OnBack;

            var resolutions = this.Q<DropdownField>("resolutions");
            resolutions.choices = Screen.resolutions.Select(r => ResolutionToString(r)).ToList();
            resolutions.value = ResolutionToString(Screen.currentResolution);
            resolutions.RegisterValueChangedCallback((e) =>
            {
                var resolution = Screen.resolutions[resolutions.choices.IndexOf(e.newValue)];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
            });

            var fullscreen = this.Q<Toggle>("fullscreen");
            fullscreen.value = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            fullscreen.RegisterValueChangedCallback((e) => Screen.fullScreenMode = e.newValue ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

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

        private void OnBack()
        {
            UIManager.Instance.ShowTitle();
        }
    }
}
