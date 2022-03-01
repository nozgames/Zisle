using UnityEngine;

namespace NoZ.Zisle
{
    public class SetMaterialFloat : EffectComponent
    {
        [SerializeField] private Tag _property = null;

        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        public override Tag Tag => _property;

        public override void Apply(EffectComponentContext context)
        {
            context.Target.MaterialProperties.SetColor(_property.ShaderPropertyId, _value);
        }

        public override void Remove(EffectComponentContext context)
        {
            context.Target.ResetMaterialProperty(_property.ShaderPropertyId);
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
