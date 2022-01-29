using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public enum MusicChannel
    {
        Ambient,
        Battle
    }

    [RequireComponent(typeof(AudioSource))]
    public class MusicVolume : MonoBehaviour
    {
        [SerializeField] private MusicChannel _channel = MusicChannel.Ambient;

        private AudioSource _audioSource;        

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = Options.MusicVolume;
            Options.OnMusicVolumeChange += (v) =>
            {
                _audioSource.volume = v;
                _audioSource.enabled = _audioSource.volume > 0;
            };
        }
    }
}
