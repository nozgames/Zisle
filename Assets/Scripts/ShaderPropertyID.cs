using UnityEngine;

namespace NoZ.Zisle
{
    public static class ShaderPropertyID
    {
        public static int HitColor { get; private set; }

        static ShaderPropertyID()
        {
            foreach (var propertyInfo in typeof(ShaderPropertyID).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                propertyInfo.SetValue(null, Shader.PropertyToID(propertyInfo.Name));
        }
    }
}
