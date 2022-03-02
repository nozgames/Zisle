using UnityEngine;

namespace NoZ.Zisle.Commands
{
    // TODO: damage over time

    public class Damage : EffectComponent
    {
        public override bool ApplyOnClient => false;

        public override void Apply(EffectComponentContext context)
        {
            var source = context.Source;
            var target = context.Target;

            // Calculate the total damage and apply to the target
            var attack = source.GetAttributeValue(ActorAttribute.Attack);
            var defense = target.GetAttributeValue(ActorAttribute.Defense);
            var damage = Mathf.Max(attack - defense, 0.0f);
            target.Damage(source, damage);
        }

        public override void Release(EffectComponentContext context)
        {
        }

        public override void Remove(EffectComponentContext context)
        {
        }
    }
}
