using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(Effect), true, isFallback = true)]
    public class EffectComponentEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CommonEditor.uss"));
            root.AddToClassList("root");
            root.Add(CreateEditor(serializedObject));
            return root;
        }

        public static VisualElement CreateEditor (SerializedObject serializedEffectComponent, string[] ignoreProperties = null)
        {
            var inspector = EditorHelpers.CreateInspector(serializedEffectComponent, 
                (f) => ignoreProperties == null || !ignoreProperties.Contains(f.propertyPath));


            return inspector;
        }
    }
}
