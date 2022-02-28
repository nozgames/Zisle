using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public static class EditorHelpers
    {
        public static VisualElement CreateInspector(SerializedObject serializedObject, Func<SerializedProperty,bool> filter = null)
        {
            var inspector = new VisualElement();
            inspector.AddToClassList("inspector");

            var property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    if (property.propertyPath == "m_Script")
                        continue;

                    if (filter != null && !filter(property))
                        continue;

                    var propertyField = new PropertyField(property);
                    propertyField.name = property.propertyPath;
                    inspector.Add(propertyField);

                } while(property.NextVisible(false));
            }

            inspector.Bind(serializedObject);

            return inspector;
        }

        public static void HandleTargetType (SerializedObject serializedObject, VisualElement inspector)
        {
            var targetTypeField = inspector.Q<PropertyField>("_target");
            var targetFinderField = inspector.Q<PropertyField>("_targetFinder");

            if (null == targetTypeField || null == targetFinderField)
                return;

            targetFinderField.label = " ";

            void UpdateTarget()
            {
                var targetProperty = serializedObject.FindProperty("_target");
                var hidden = targetProperty.enumValueIndex != (int)TargetType.Custom;
                targetFinderField.EnableInClassList("hidden", hidden);
            }

            targetTypeField.RegisterValueChangeCallback((v) => UpdateTarget());

            UpdateTarget();
        }

        public static string NicifyConditionName(string name)
        {
            if (name.EndsWith("Condition"))
                name = name.Substring(0, name.Length - 9);

            return ObjectNames.NicifyVariableName(name);
        }

        public static string NicifyEventName (string name)
        {
            if (name.EndsWith("Event"))
                name = name.Substring(0, name.Length - 5);

            return ObjectNames.NicifyVariableName(name);
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (var t in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(t);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }
    }
}
