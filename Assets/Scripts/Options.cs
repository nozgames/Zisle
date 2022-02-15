using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoZ.Zisle
{
    public static class Options 
    {
        private const string PlayerPrefsPlayerName = "Options.Player.Name";
        private const string PlayerPrefsPlayerClass = "Options.Player.Class";
        private const string PlayerPrefsScreenShake = "Options.ScreenShake";
        private const string PlayerPrefsSoundVolume = "Options.SoundVolume";
        private const string PlayerPrefsMusicVolume = "Options.MusicVolume";
        private const string PlayerPrefsLanIP = "Options.LanIP";
        private const string PlayerPrefsRebinds = "Options.Rebinds";
        private const string PlayerPrefsGamepadZoomSpeed = "Options.Gamepad.ZoomSpeed";

        private static bool _screenShake;
        private static float _soundVolume;
        private static float _musicVolume;
        private static float _gamepadZoomSpeed;

        private static string _playerName;
        private static string _playerClass;
        private static string _lanIP;

        public static event Action<bool> OnScreenShakeChange;
        public static event Action<float> OnSoundVolumeChange;
        public static event Action<float> OnMusicVolumeChange;
        public static event Action<float> OnGamepadZoomSpeedChange;

        public static string LanIP
        {
            get => _lanIP;
            set
            {
                if (_lanIP == value)
                    return;

                _lanIP = value;
                PlayerPrefs.SetString(PlayerPrefsLanIP, _lanIP);
            }
        }

        public static string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName == value)
                    return;

                _playerName = value;
                PlayerPrefs.SetString(PlayerPrefsPlayerName, _playerName);
            }
        }

        public static string PlayerClass
        {
            get => _playerClass;
            set
            {
                if (_playerClass == value)
                    return;

                _playerClass = value;
                PlayerPrefs.SetString(PlayerPrefsPlayerClass, _playerClass);
            }
        }

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

        public static float GamepadZoomSpeed
        {
            get => _gamepadZoomSpeed;
            set
            {
                _gamepadZoomSpeed = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PlayerPrefsGamepadZoomSpeed, _gamepadZoomSpeed);
                OnGamepadZoomSpeedChange?.Invoke(_gamepadZoomSpeed);
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
            catch
            {
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOptions()
        {
            _screenShake = PlayerPrefs.GetInt(PlayerPrefsScreenShake, 1) > 0;
            _soundVolume = PlayerPrefs.GetFloat(PlayerPrefsSoundVolume, 1.0f);
            _musicVolume = PlayerPrefs.GetFloat(PlayerPrefsMusicVolume, 1.0f);
            _gamepadZoomSpeed = PlayerPrefs.GetFloat(PlayerPrefsGamepadZoomSpeed, 0.5f);
            _playerClass = PlayerPrefs.GetString(PlayerPrefsPlayerClass, "RandomPlayer");
            _playerName = PlayerPrefs.GetString(PlayerPrefsPlayerName, "Player");
            _lanIP = PlayerPrefs.GetString(PlayerPrefsLanIP, "127.0.0.1");
        }
    }
}
