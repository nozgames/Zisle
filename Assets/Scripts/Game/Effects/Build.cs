using UnityEngine;

namespace NoZ.Zisle.Commands
{
    public class Build : EffectComponent
    {
        public override bool ApplyOnClient => false;

        public override void Apply(EffectComponentContext context)
        {
            if (context.Target is Building building)
                building.Heal(context.Source, context.Source.GetAttributeValue(ActorAttribute.Build));
        }

        public override void Remove(EffectComponentContext context)
        {
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
