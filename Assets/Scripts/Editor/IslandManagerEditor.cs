using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class IslandManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var biomes = new PropertyField();
            biomes.label = "Biomes";
            biomes.BindProperty(serializedObject.FindProperty("_biomes"));
            root.Add(biomes);
            
            return root;
        }
    }
}
