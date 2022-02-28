using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Set Material Float")]
    public class SetMaterialFloat : ActorEffect
    {
        [SerializeField] private string _propertyName = null;

        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        public int PropertyNameId { get; private set; } = -1;

        private void OnEnable()
        {
            PropertyNameId = Shader.PropertyToID(_propertyName);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PropertyNameId = Shader.PropertyToID(_propertyName);
        }
#endif

        public override void Apply(ActorEffectContext context)
        {
            context.Target.MaterialProperties.SetColor(PropertyNameId, _value);
        }

        public override void Remove(ActorEffectContext context)
        {
            context.Target.ResetMaterialProperty(PropertyNameId);
        }

        public override bool DoesOverride(ActorEffect effect) =>
            (effect is SetMaterialFloat set && set.PropertyNameId == PropertyNameId);
    }
}