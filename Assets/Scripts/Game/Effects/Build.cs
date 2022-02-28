using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Build")]
    public class Build : ActorEffect
    {
        public override void Apply(ActorEffectContext context)
        {
            if (context.Target is Building building)
                building.Heal(context.Source, context.Source.GetAttributeValue(ActorAttribute.Build));
        }

        public override void Remove(ActorEffectContext context)
        {
        }
    }
}
