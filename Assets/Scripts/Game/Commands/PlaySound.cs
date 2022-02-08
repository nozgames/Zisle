using UnityEngine;
using System.Collections;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Sound")]
    public class PlaySound : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private AudioClip[] _clips = null;

        private int _next = 0;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            if (_clips == null || _clips.Length == 0)
                return;

            target.StartCoroutine(PlaySoundCoroutine(target, _clips[_next]));

            _next = (_next + 1) % _clips.Length;
        }

        static IEnumerator PlaySoundCoroutine(Actor target, AudioClip clip)
        {
            var audioSource = target.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SoundMixerGroup;
            audioSource.Play();

            while (audioSource.isPlaying)
                yield return null;

            Destroy(audioSource);
        }
    }
}
