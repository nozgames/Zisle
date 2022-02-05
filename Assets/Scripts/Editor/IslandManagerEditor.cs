using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(IslandManager))]
    public class IslandManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var biomes = new PropertyField();
            biomes.label = "Biomes";
            biomes.BindProperty(serializedObject.FindProperty("_biomes"));
            root.Add(biomes);

            var options = new PropertyField();
            options.label = "Options";
            options.BindProperty(serializedObject.FindProperty("_defaultOptions"));
            root.Add(options);

            var button = new Button();
            button.text = "Generate";
            button.clicked += () => (target as IslandManager).GenerateIslands();
            root.Add(button);

            var clearButton = new Button();
            clearButton.text = "Clear";
            clearButton.clicked += () => (target as IslandManager).ClearIslands();
            root.Add(clearButton);

            return root;
        }
    }
}
