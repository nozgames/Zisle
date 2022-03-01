using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Group")]
    public class GroupEffect : Effect
    {
        [SerializeReference]
        [SerializeField] private Effect[] _effects = null;

        private void OnEnable()
        {
        }

        public override void Apply(EffectContext context)
        {
            for (int i = 0; i < _effects.Length; i++)
                context.Target.AddEffect(context.Source, _effects[i], context);

            // Make the group effect go away after it spawns its children.
            // TODO: if at some point we want to be able to remove the group effect we will
            //       need to tag all of the children as coming from the group
            context.Lifetime = EffectLifetime.Instant;
        }

        public override void Remove(EffectContext context)
        {
        }
    }
}
