using UnityEngine;

namespace NoZ.Zisle
{
    public class Heal : EffectComponent
    {
        public override bool ApplyOnClient => false;

        public override void Apply(EffectComponentContext context)
        {
        }

        public override void Remove(EffectComponentContext context)
        {
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
