using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Actor Effects/Set Material Color")]
    public class SetMaterialColor : ActorEffect
    {
        [SerializeField] private string _name = null;
        
        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        private int _nameId = -1;

        private void OnEnable()
        {
            _nameId = Shader.PropertyToID(_name);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _nameId = Shader.PropertyToID(_name);
        }
#endif

        public override void Apply(ActorEffectContext context)
        {
            context.Target.MaterialProperties.SetColor(_name, _value);
        }

        public override void Remove(ActorEffectContext context)
        {
            context.Target.ResetMaterialProperty(_nameId);
        }

        public override bool DoesOverride(ActorEffect effect) =>
            (effect is SetMaterialColor set && set._nameId == _nameId);
    }
}
