using UnityEngine;
using UnityEngine.Audio;

namespace NoZ.Zisle
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("General")]
        [SerializeField] private AudioMixer _mixer = null;

        [Header("2D Sounds")]
        [SerializeField] private AudioSource _buttonClick = null;

        public override void Initialize()
        {
            base.Initialize();

            _mixer.SetFloat("MusicVolume", LinearToDB(Options.MusicVolume));
            _mixer.SetFloat("SoundVolume", LinearToDB(Options.SoundVolume));

            Options.OnMusicVolumeChange += (v) => _mixer.SetFloat("MusicVolume", LinearToDB(v));
            Options.OnSoundVolumeChange += (v) => _mixer.SetFloat("SoundVolume", LinearToDB(v));
        }

        private float LinearToDB(float v)
        {
            if (v <= float.Epsilon)
                return -80.0f;

            return Mathf.Log10(v) * 20.0f;
        }

        public void PlayButtonClick() => _buttonClick.Play();
    }
}
