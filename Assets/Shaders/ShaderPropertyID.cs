using UnityEngine;

namespace Survival
{
    public static class ShaderPropertyID
    {
        public static int HorizonColor { get; private set; }
        public static int FogSize { get; private set; }
        public static int FogColor { get; private set; }
        public static int SkyColor { get; private set; }
        public static int StarColor { get; private set; }
        public static int SunSize { get; private set; }
        public static int SunColor { get; private set; }

        static ShaderPropertyID()
        {
            foreach (var propertyInfo in typeof(ShaderPropertyID).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                propertyInfo.SetValue(null, Shader.PropertyToID(propertyInfo.Name));
        }
    }
}
