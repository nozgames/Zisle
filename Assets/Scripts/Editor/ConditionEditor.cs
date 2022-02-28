using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public static class ConditionEditor
    {
        public static VisualElement CreateEditor (SerializedObject serializedObject)
        {
            var properties = EditorHelpers.CreateInspector(serializedObject);
            EditorHelpers.HandleTargetType(serializedObject, properties);
            return properties;
        }
    }
}
