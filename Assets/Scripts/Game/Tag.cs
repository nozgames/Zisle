using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Tag")]
    public class Tag : NetworkScriptableObject<Tag>
    {
        private int _shaderPropertyId = -1;

        private void OnEnable()
        {
            _shaderPropertyId = Shader.PropertyToID(name);
        }

        public int ShaderPropertyId => _shaderPropertyId;
    }
}
