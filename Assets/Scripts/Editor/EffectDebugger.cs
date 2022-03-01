using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class EffectDebugger : EditorWindow
    {
        private VisualElement _contents;
        private VisualElement _effects;
        private TextField _actorName;

        [MenuItem("Zisle/Effect Debugger")]
        private static void ShowWindow ()
        {
            var window = EditorWindow.GetWindow<EffectDebugger>();
            window.titleContent = new GUIContent("Effect Debugger");
            window.Show();
        }

        public void OnEnable()
        {

            _contents = new VisualElement();
            _actorName = new TextField("Name");
            _actorName.SetEnabled(false);
            _contents.Add(_actorName);

            var foldout = new Foldout();
            foldout.text = "Effects";
            _effects = foldout;
            _contents.Add(_effects);

            rootVisualElement.Add(_contents);
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CommonEditor.uss"));

            OnSelectionChanged();

            Selection.selectionChanged += OnSelectionChanged;
        }

        public void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            var gameObject = Selection.activeObject as GameObject;
            if (null == gameObject)
            {
                UpdateEffects(null);
                return;
            }

            var actor = gameObject.GetComponentInParent<Actor>();
            if(actor != null)
                UpdateEffects(actor);
        }

        private void UpdateEffects(Actor actor)
        {
            if (_actorName == null)
                return;

            if(actor == null)
            {
                _contents.AddToClassList("hidden");
                return;
            }

            _contents.RemoveFromClassList("hidden");

            _effects.Clear();
            foreach(var stack in actor.Effects.Stacks)
            {
                var stackFoldout = new Foldout();
                stackFoldout.text = ObjectNames.NicifyVariableName(stack.Effect.name);

                foreach(var context in stack.Contexts)
                {
                    var contextFoldout = new Foldout();
                    contextFoldout.text = ObjectNames.NicifyVariableName("x");

                    foreach(var component in context.Components)
                    {
                        contextFoldout.Add(new Label($"{component.EffectComponent.name} / {component.Enabled}"));
                    }

                    stackFoldout.Add(contextFoldout);
                }

                _effects.Add(stackFoldout);
            }

            _actorName.value = actor.name;
        }
    }
}
