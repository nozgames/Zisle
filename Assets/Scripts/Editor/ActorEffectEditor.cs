using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(ActorEffect), true, isFallback = true)]
    public class ActorEffectEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CommonEditor.uss"));
            root.AddToClassList("root");
            root.Add(CreateEditor(serializedObject));
            return root;
        }

        public static VisualElement CreateEditor (SerializedObject effectObject, string[] ignoreProperties = null)
        {
            var inspector = EditorHelpers.CreateInspector(effectObject, (f) => ignoreProperties == null || !ignoreProperties.Contains(f.propertyPath));
            var durationField = inspector.Q<PropertyField>("_duration");
            var lifetimeField = inspector.Q<PropertyField>("_lifetime");

            void UpdateDuration()
            {
                var lifetimeProperty = effectObject.FindProperty("_lifetime");
                var hidden = lifetimeProperty.enumValueIndex != (int)ActorEffectLifetime.Time;
                durationField.EnableInClassList("hidden", hidden);
            }

            lifetimeField.RegisterValueChangeCallback((e) => UpdateDuration());

            UpdateDuration();

            EditorHelpers.HandleTargetType(effectObject, inspector);

            return inspector;
        }
    }
}
