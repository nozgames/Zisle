using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Play Sound")]
    public class PlaySound : ActorEffect
    {
        [SerializeField] private AudioShader _shader = null;

        public override void Apply(ActorEffectContext context)
        {
            AudioManager.Instance.PlaySound(_shader, context.Target.gameObject);
        }

        public override void Remove(ActorEffectContext context)
        {
        }
    }
}
