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
        [SerializeField] private AudioSource _victorySound = null;
        [SerializeField] private AudioSource _defeatSound = null;

        [Header("Music")]
        [SerializeField] private float _musicCrossFadeDuration = 1.0f;
        [SerializeField] private AudioSource _idleMusic = null;
        [SerializeField] private AudioSource _battleMusic = null;
        [SerializeField] private AudioSource _bossMusic = null;

        private AudioSource _currentMusic = null;
        private float _musicDuckVolume = 1.0f;

        public float MusicDuckVolume
        {
            get => _musicDuckVolume;
            set
            {
                _musicDuckVolume = value;
                UpdateMusicVolume();
            }
        }

        public AudioMixerGroup SoundMixerGroup => _soundMixerGroup;

        public override void Initialize()
        {
            base.Initialize();

            _mixer.SetFloat("SoundVolume", LinearToDB(Options.SoundVolume));

            UpdateMusicVolume();

            Options.OnMusicVolumeChange += (v) => UpdateMusicVolume();
            Options.OnSoundVolumeChange += (v) => _mixer.SetFloat("SoundVolume", LinearToDB(v));

            PlayIdleMusic();
        }

        private float LinearToDB(float v)
        {
            if (v <= float.Epsilon)
                return -80.0f;

            return Mathf.Log10(v) * 20.0f;
        }

        public void PlayButtonClickSound() => _buttonClick.Play();
        public void PlayTimerTickSound() => _timerTick.Play();
        public void PlayJoinGameSound() => _joinGame.Play();
        public void PlayVictorySound() => _victorySound.Play();
        public void PlayDefeatSound() => _defeatSound.Play();
        public void PlayBattleStartSound() => _battleStartSound.Play();

        private void UpdateMusicVolume()
        {
            _mixer.SetFloat("MusicVolume", LinearToDB(Options.MusicVolume * MusicDuckVolume));
        }

        /// <summary>
        /// Duck the music volume for the given duration
        /// </summary>
        /// <param name="duration"></param>
        public void DuckMusic (float duration=1.0f, float duckOutDuration=0.1f, float duckInDuration=0.5f)
        {
            this.TweenSequence()
                .Element(this.TweenFloat("MusicDuckVolume", 0.0f).Duration(duckOutDuration).EaseInQuadratic())
                .Element(this.TweenWait(Mathf.Max(0.1f, duration - (duckOutDuration + duckInDuration))))
                .Element(this.TweenFloat("MusicDuckVolume", 1.0f).Duration(duckInDuration).EaseOutQuadratic())
                .Play();
        }

        private void SetMusic (AudioSource music)
        {
            if (_currentMusic != null)
            {
                var stop = _currentMusic;
                _currentMusic.TweenFloat("volume", 0.0f)
                    .Duration(_musicCrossFadeDuration)
                    .EaseInOutQuadratic()
                    .OnStop(() => stop.Stop()).Play();
            }                

            _currentMusic = music;
            if (null == _currentMusic)
                return;

            _currentMusic.Play();
            _currentMusic.volume = 0.0f;
            _currentMusic.TweenFloat("volume", 1.0f)
                .Duration(_musicCrossFadeDuration)
                .EaseInOutQuadratic()
                .Play();
        }

        public void PlayBattleMusic() => SetMusic(_battleMusic);
        public void PlayBossMusic() => SetMusic(_bossMusic);
        public void PlayIdleMusic() => SetMusic(_idleMusic);
    }
}
