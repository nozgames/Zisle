using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoZ.Zisle
{
    public static class Options 
    {
        private const string PlayerPrefsScreenShake = "Options.ScreenShake";
        private const string PlayerPrefsSoundVolume = "Options.SoundVolume";
        private const string PlayerPrefsMusicVolume = "Options.MusicVolume";
        private const string PlayerPrefsRebinds = "Options.Rebinds";

        private static bool _screenShake;
        private static float _soundVolume;
        private static float _musicVolume;

        public static event Action<bool> OnScreenShakeChange;
        public static event Action<float> OnSoundVolumeChange;
        public static event Action<float> OnMusicVolumeChange;

        public static bool ScreenShake
        {
            get => _screenShake;
            set
            {
                _screenShake = value;

                PlayerPrefs.SetInt(PlayerPrefsScreenShake, value ? 1 : 0);

                OnScreenShakeChange?.Invoke(_screenShake);
            }
        }

        public static float SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PlayerPrefsSoundVolume, _soundVolume);
                OnSoundVolumeChange?.Invoke(_soundVolume);
            }
        }

        public static float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PlayerPrefsMusicVolume, _musicVolume);
                OnMusicVolumeChange?.Invoke(_musicVolume);
            }
        }

        public static void SaveBindings(InputActionAsset actions)
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PlayerPrefsRebinds, rebinds);
        }

        public static void LoadBindings (InputActionAsset actions)
        {
            var rebinds = PlayerPrefs.GetString(PlayerPrefsRebinds);

            try
            {
                actions.LoadBindingOverridesFromJson(rebinds);
            }
            catch (Exception ex)
            {
            }            
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOptions()
        {
            _screenShake = PlayerPrefs.GetInt(PlayerPrefsScreenShake, 1) > 0;
            _soundVolume = PlayerPrefs.GetFloat(PlayerPrefsSoundVolume, 1.0f);
            _musicVolume = PlayerPrefs.GetFloat(PlayerPrefsMusicVolume, 1.0f);
        }
    }
}
