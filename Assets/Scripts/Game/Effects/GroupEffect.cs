using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Group")]
    public class GroupEffect : ActorEffect
    {
        [SerializeReference]
        [SerializeField] private ActorEffect[] _effects = null;

        public override void Apply(ActorEffectContext context)
        {
            for (int i = 0; i < _effects.Length; i++)
                context.Target.AddEffect(context.Source, _effects[i], context);

            // Make the group effect go away after it spawns its children.
            // TODO: if at some point we want to be able to remove the group effect we will
            //       need to tag all of the children as coming from the group
            context.Lifetime = ActorEffectLifetime.Instant;
        }

        public override void Remove(ActorEffectContext context)
        {
        }
    }
}
