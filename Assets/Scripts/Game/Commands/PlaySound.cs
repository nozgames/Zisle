using UnityEngine;
using System.Collections;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Sound")]
    public class PlaySound : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private AudioClip _clip = null;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            target.StartCoroutine(PlaySoundCoroutine(target, _clip));
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
