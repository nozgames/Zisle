using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(GroupEffect))]
    public class GroupEffectEditor : Editor
    {
        private VisualElement _root;
        private VisualElement _inspector;
        private VisualElement _effects;
        private VisualElement _effectsContent;

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public void OnDisable()
        {
            _effects = null;
            _effectsContent = null;
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= OnNextEditorFrame;
        }

        private void OnUndoRedo()
        {
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList("root");
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CommonEditor.uss"));
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/GroupEffectEditor.uss"));

            _inspector = ActorEffectEditor.CreateEditor(serializedObject, new string[] { "_effects" });
            root.Add(_inspector);

            _effects = CreateEffectsTab ();
            var eventsTab = new TabButton("Effects", _effects);
            var tabs = new TabbedView();
            tabs.name = "tabs";
            tabs.Add(eventsTab);
            root.Add(tabs);

            //UpdateEffects();

            EditorApplication.update += OnNextEditorFrame;

            _root = root;
            return root;
        }

        
        private void OnNextEditorFrame()
        {
            _root.Unbind();
            _inspector.Bind(serializedObject);

            EditorApplication.update -= OnNextEditorFrame;
            UpdateEffects();
        }

        private VisualElement CreateEffectsTab()
        {
            var effects = new VisualElement();
            effects.name = "effects";

            _effectsContent = new VisualElement();
            _effectsContent.name = "effects__content";
            effects.Add(_effectsContent);

            var addEffect = new Button();
            addEffect.AddToClassList("add-button");
            addEffect.text = "Add Effect";
            addEffect.clicked += () =>
            {
                var genericMenu = new GenericDropdownMenu();

                foreach (var type in TypeCache.GetTypesDerivedFrom<Effect>())
                {
                    genericMenu.AddItem(NicifyEffectName(type.Name), false, () =>
                    {
                        var prop = serializedObject.FindProperty("_effects");
                        prop.InsertArrayElementAtIndex(prop.arraySize);
                        var eprop = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                        var i = CreateInstance(type);
//                        (i as Effect).Lifetime = EffectLifetime.Inherit;
                        i.name = type.Name;
                        AssetDatabase.AddObjectToAsset(i, target);
                        eprop.objectReferenceValue = i;
                        serializedObject.ApplyModifiedProperties();
                        UpdateEffects();
                    }); ;
                }
                genericMenu.DropDown(addEffect.worldBound, addEffect, anchored: true);
            };
            effects.Add(addEffect);

            return effects;
        }

        private string NicifyEffectName(string name)
        {
            if (name.EndsWith("Effect"))
                name = name.Substring(0, name.Length - 6);

            return ObjectNames.NicifyVariableName(name);
        }

        private void UpdateEffects ()
        {
            _effectsContent.Clear();
            _effectsContent.Unbind();

            var effectsPropety = serializedObject.FindProperty("_effects");
            for (int i = 0; i < effectsPropety.arraySize; i++)
            {
                var effectProperty = effectsPropety.GetArrayElementAtIndex(i);
                var effect = (Effect)effectProperty.objectReferenceValue;
                var effectIndex = i;

                var effectItem = new VisualElement();
                effectItem.AddToClassList("elements__item");

                var foldout = new Foldout();
                foldout.text = NicifyEffectName(effect.GetType().Name);

                var content = ActorEffectEditor.CreateEditor(new SerializedObject(effect));
                content.AddToClassList("elements__item__content");

                var options = new VisualElement();
                options.AddToClassList("options-button");
                options.pickingMode = PickingMode.Position;
                options.AddManipulator(new Clickable(() =>
                {
                    var genericMenu = new GenericDropdownMenu();
                    if (effectIndex == 0)
                        genericMenu.AddDisabledItem("Move Up", false);
                    else
                        genericMenu.AddItem("Move Up", false, () => {
                            effectsPropety.MoveArrayElement(effectIndex, effectIndex - 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateEffects();
                        });

                    if (effectIndex >= effectsPropety.arraySize - 1)
                        genericMenu.AddDisabledItem("Move Down", false);
                    else
                        genericMenu.AddItem("Move Down", false, () => {
                            effectsPropety.MoveArrayElement(effectIndex, effectIndex + 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateEffects();
                        });
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem("Remove Effect", false, () => {
                        for (int i = effectsPropety.arraySize - 1; i >= 0; i--)
                        {
                            var conditionElement = effectsPropety.GetArrayElementAtIndex(i);
                            if (conditionElement.objectReferenceValue == effect)
                            {
                                AssetDatabase.RemoveObjectFromAsset(effect);
                                effectsPropety.DeleteArrayElementAtIndex(i);
                                effectItem.parent.Remove(effectItem);
                            }
                        }

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateEffects();
                    });
                    genericMenu.DropDown(options.worldBound, options, false);
                }));
                foldout.Q("unity-checkmark").parent.Add(options);

                foldout.Add(content);

                effectItem.Add(foldout);
                _effectsContent.Add(effectItem);
            }
        }
    }
}
