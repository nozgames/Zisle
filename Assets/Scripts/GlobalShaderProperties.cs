using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Global Shader")]
    public class GlobalShaderProperties : ScriptableObject
    {
        [SerializeField] private Color _shadowColor = Color.black;
        [SerializeField] private Color _sunColor = Color.white;

#if UNITY_EDITOR
        private void OnValidate() => UpdateProperties();

        [InitializeOnLoadMethod]
        private void UpdatePropertiesOnLoade() => UpdateProperties();
#endif

        private void OnEnable() => UpdateProperties();

        private void UpdateProperties()
        {
            Shader.SetGlobalColor("_ShadowColor", _shadowColor);
            Shader.SetGlobalColor("_SunColor", _sunColor);
        }
    }
}
