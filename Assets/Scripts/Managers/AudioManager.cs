using NoZ.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace NoZ.Zisle
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("General")]
        [SerializeField] private AudioMixer _mixer = null;
        [SerializeField] private AudioMixerGroup _soundMixerGroup = null;

        [Header("2D Sounds")]
        [SerializeField] private AudioSource _buttonClick = null;
        [SerializeField] private AudioSource _timerTick = null;
        [SerializeField] private AudioSource _joinGame = null;
        [SerializeField] private AudioSource _battleStartSound = null;

        [Header("Music")]
        [SerializeField] private AudioSource _idleMusic = null;
        [SerializeField] private AudioSource _battleMusic = null;
        [SerializeField] private float _battleMusicCrossFadeDuration = 0.1f;

        private float _battleMusicVolume = 0.0f;

        public AudioMixerGroup SoundMixerGroup => _soundMixerGroup;

        public float BattleMusicVolume
        {
            get => _battleMusicVolume;
            set
            {
                _battleMusicVolume = value;
                UpdateMusicVolume();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _mixer.SetFloat("SoundVolume", LinearToDB(Options.SoundVolume));

            UpdateMusicVolume();

            Options.OnMusicVolumeChange += (v) => UpdateMusicVolume();
            Options.OnSoundVolumeChange += (v) => _mixer.SetFloat("SoundVolume", LinearToDB(v));
        }

        private float LinearToDB(float v)
        {
            if (v <= float.Epsilon)
                return -80.0f;

            return Mathf.Log10(v) * 20.0f;
        }

        public void PlayButtonClick() => _buttonClick.Play();
        public void PlayTimerTick() => _timerTick.Play();
        public void PlayJoinGame() => _joinGame.Play();

        private void UpdateMusicVolume()
        {
            if (_battleMusicVolume <= 0.0f && _battleMusic.isPlaying)
                _battleMusic.Stop();
            else if (_battleMusicVolume > 0.0f && !_battleMusic.isPlaying)
                _battleMusic.Play();

            if (_battleMusicVolume < 1.0f && !_idleMusic.isPlaying)
                _idleMusic.Play();
            else if (_battleMusicVolume >= 1.0f && _idleMusic.isPlaying)
                _idleMusic.Stop();

            _mixer.SetFloat("MusicVolume", LinearToDB(Options.MusicVolume * (1.0f - _battleMusicVolume)));
            _mixer.SetFloat("BattleMusicVolume", LinearToDB(Options.MusicVolume * _battleMusicVolume));
        }

        public void PlayBattleMusic ()
        {
            this.TweenFloat("BattleMusicVolume", 1.0f)
                .Duration(_battleMusicCrossFadeDuration)
                .EaseInOutQuadratic()
                .Play();
        }

        public void PlayIdleMusic ()
        {
            this.TweenFloat("BattleMusicVolume", 0.0f)
                .Duration(_battleMusicCrossFadeDuration)
                .EaseInOutQuadratic()
                .Play();
        }

        public void PlayBattleStart() => _battleStartSound.Play();
    }
}
