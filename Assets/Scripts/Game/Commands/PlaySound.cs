using UnityEngine;
using System.Collections;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Sound")]
    public class PlaySound : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private AudioShader _shader = null;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            if (_shader == null)
                return;

            target.StartCoroutine(PlaySoundCoroutine(target, _shader));
        }

        // TODO: reuse
        static IEnumerator PlaySoundCoroutine(Actor target, AudioShader shader)
        {
            var audioSource = target.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SoundMixerGroup;
            audioSource.PlayOneShot(shader);

            while (audioSource.isPlaying)
                yield return null;

            Destroy(audioSource);
        }
    }
}
