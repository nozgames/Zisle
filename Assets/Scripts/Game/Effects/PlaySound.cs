using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Play Sound")]
    public class PlaySound : ActorEffect
    {
        [SerializeField] private AudioClip[] _clips = null;

        [Range(0, 1)]
        [SerializeField] private float _volume = 1f;

        [Range(0, 2)]
        [SerializeField] private float _pitch = 1f;

        [Range(0, 1)]
        [SerializeField] private float _randomPitch = 0f;

        [Range(0, 1)]
        [SerializeField] private float _randomVolume = 0f;

        [Tooltip("Range the clip can be heard from when playing spatially")]
        [SerializeField] private float _spatialRange = 10f;

        public override void Apply(ActorEffectContext context)
        {
            if (_clips == null || _clips.Length == 0)
                return;

            AudioManager.Instance.PlaySound(
                context.Target.gameObject,
                _clips[Random.Range(0, _clips.Length)],
                _volume + Random.Range(-_randomVolume, _randomVolume),
                _pitch + Random.Range(-_randomPitch, _randomPitch),
                _spatialRange);
        }

        public override void Remove(ActorEffectContext context)
        {
        }
    }
}
