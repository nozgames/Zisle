using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Damage")]
    public class Damage : ActorEffect
    {
        public override void Apply(ActorEffectContext context)
        {
            var source = context.Source;
            var target = context.Target;

            // Calculate the total damage and apply to the target
            var attack = source.GetAttributeValue(ActorAttribute.Attack);
            var defense = target.GetAttributeValue(ActorAttribute.Defense);
            var damage = Mathf.Max(attack - defense, 0.0f);
            target.Damage(source, damage);
        }

        public override void Remove(ActorEffectContext context)
        {
        }
    }
}
