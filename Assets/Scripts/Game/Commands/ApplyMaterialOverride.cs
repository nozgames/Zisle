using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Apply Material Override")]
    public class ApplyMaterialOverride : ActorCommand, IExecuteOnClient
    {
        private abstract class PropertyOverride
        {
            public string Name;

            private int _nameId = -1;

            public int NameId
            {
                get
                {
                    if (_nameId == -1)
                        _nameId = Shader.PropertyToID(Name);

                    return _nameId;
                }
            }

            public abstract void Apply(MaterialPropertyBlock block);
        }

        [System.Serializable]
        private class FloatPropertyOverride : PropertyOverride
        {
            public float Value;
            public override void Apply(MaterialPropertyBlock block) => block.SetFloat(NameId, Value);
        }

        [System.Serializable]
        private class ColorPropertyOverride : PropertyOverride
        {
            public Color Value;
            public override void Apply(MaterialPropertyBlock block) => block.SetColor(NameId, Value);
        }

        private enum Variable
        {
            Time
        }

        [System.Serializable]
        private class VariablePropertyOverride : PropertyOverride
        {
            public Variable Value;
            public override void Apply(MaterialPropertyBlock block)
            {
                switch(Value)
                {
                    case Variable.Time:
                        block.SetFloat(NameId, Time.time);
                        break;
                }
            }
        }

        [SerializeField] private Material _original = null;
        [SerializeField] private Material _override = null;

        [SerializeField] private FloatPropertyOverride[] _floatPropertyOverrides = null;
        [SerializeField] private ColorPropertyOverride[] _colorPropertyOverrides = null;
        [SerializeField] private VariablePropertyOverride[] _variablePropertyOverrides = null;

        private MaterialPropertyBlock _properties;

        private void InitializePropeties(IEnumerable<PropertyOverride> properties)
        {
            foreach(var property in properties)
            {
                if(null == _properties)
                    _properties = new MaterialPropertyBlock();

                property.Apply(_properties);
            }
        }

        private void InitializeProperties ()
        {
            if (_properties != null)
                return;

            if(_floatPropertyOverrides != null && _floatPropertyOverrides.Length > 0)
                InitializePropeties(_floatPropertyOverrides);

            if (_colorPropertyOverrides != null && _colorPropertyOverrides.Length > 0)
                InitializePropeties(_colorPropertyOverrides);
        }

        public void ExecuteOnClient(Actor source, Actor target)
        {
            InitializeProperties();

            // Variable property overrides must be applied each time.
            if (_variablePropertyOverrides != null && _variablePropertyOverrides.Length > 0)
                InitializePropeties(_variablePropertyOverrides);

            if (_override == null)
                return;

            var materialOverride = source.GetComponent<MaterialOverride>();
            if (null == materialOverride)
                return;

            materialOverride.Override(_original, _override, _properties);
        }
    }
}
