using NoZ.Tweening;
using System;
using System.Collections.Generic;
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

        [Header("3D Sounds")]
        [SerializeField] private AudioSource _spatialPrefab = null;
        [SerializeField] private Transform _spatialPoolTransform = null;
        [SerializeField] private int _maxSpatialPoolSize = 32;

        [Header("Music")]
        [SerializeField] private float _musicCrossFadeDuration = 1.0f;
        [SerializeField] private AudioSource _idleMusic = null;
        [SerializeField] private AudioSource _battleMusic = null;
        [SerializeField] private AudioSource _bossMusic = null;

        private AudioSource _currentMusic = null;
        private float _musicDuckVolume = 1.0f;
        private UnityEngine.Pool.LinkedPool<AudioSource> _spatialPool;
        private List<AudioSource> _activeAudioSources;

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

            _activeAudioSources = new List<AudioSource>();
            _spatialPool = new UnityEngine.Pool.LinkedPool<AudioSource>(
                createFunc: SpatialPoolCreate,
                actionOnGet: SpatialPoolGet,
                actionOnRelease: SpatialPoolRelease,
                actionOnDestroy: SpatialPoolDestroy,
                maxSize: _maxSpatialPoolSize
                );
        }

        private void SpatialPoolDestroy(AudioSource source) => Destroy(source.gameObject);
        private void SpatialPoolGet(AudioSource source) { }
        private void SpatialPoolRelease(AudioSource source)
        {
            source.clip = null;
            source.transform.SetParent(_spatialPoolTransform);
        }

        private AudioSource SpatialPoolCreate()
        {
            var source = Instantiate(_spatialPrefab, _spatialPoolTransform).GetComponent<AudioSource>();
            source.transform.SetParent(transform);
            source.name = "SpatialAudioSource";
            return source;
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

        /// <summary>
        /// Play a sound relative to the given game object
        /// </summary>
        public AudioSource PlaySound (AudioShader shader, GameObject gameObject)
        {
            if (shader == null)
                return null;

            if (!gameObject.activeInHierarchy)
                return null;

            var source = _spatialPool.Get();
            source.transform.SetParent(gameObject.transform,false);
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            source.enabled = true;
            source.PlayOneShot(shader);
            _activeAudioSources.Add(source);
            return source;
        }

        private void LateUpdate()
        {
            // Check for any audio sources that have finished playing and return them to the pool
            for(int i= _activeAudioSources.Count-1; i>=0; i--)
            {
                if(!_activeAudioSources[i].isPlaying)
                {
                    var source = _activeAudioSources[i];
                    _activeAudioSources.RemoveAt(i);
                    _spatialPool.Release(source);
                }
            }
        }
    }
}
