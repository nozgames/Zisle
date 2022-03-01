using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(Effect))]
    public class EffectEditor : Editor
    {
        private VisualElement _root;
        private VisualElement _inspector;
        private VisualElement _components;
        private VisualElement _componentsContent;

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public void OnDisable()
        {
            _components = null;
            _componentsContent = null;
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

            _inspector = EditorHelpers.CreateInspector(serializedObject, (f) => f.propertyPath != "_components");
            root.Add(_inspector);

            var durationField = _inspector.Q<PropertyField>("_duration");
            var lifetimeField = _inspector.Q<PropertyField>("_lifetime");

            void UpdateDuration()
            {
                var lifetimeProperty = serializedObject.FindProperty("_lifetime");
                var hidden = lifetimeProperty.enumValueIndex != (int)EffectLifetime.Time;
                durationField.EnableInClassList("hidden", hidden);
            }

            lifetimeField.RegisterValueChangeCallback((e) => UpdateDuration());

            UpdateDuration();

            EditorHelpers.HandleTargetType(serializedObject, _inspector);

            _components = CreateComponentsTab ();

            var componentsFoldout = new Foldout();
            componentsFoldout.text = "Components";
            componentsFoldout.AddToClassList("elements");
            componentsFoldout.Add(_components);
            root.Add(componentsFoldout);

            UpdateComponents();

            EditorApplication.update += OnNextEditorFrame;

            _root = root;
            return root;
        }
        
        private void OnNextEditorFrame()
        {
            _root.Unbind();
            _inspector.Bind(serializedObject);

            EditorApplication.update -= OnNextEditorFrame;
            UpdateComponents();
        }

        private VisualElement CreateComponentsTab()
        {
            var components = new VisualElement();
            components.name = "components";

            _componentsContent = new VisualElement();
            _componentsContent.name = "components__content";
            components.Add(_componentsContent);

            var addComponent = new Button();
            addComponent.AddToClassList("add-button");
            addComponent.text = "Add Component";
            addComponent.clicked += () =>
            {
                var genericMenu = new GenericDropdownMenu();

                foreach (var type in TypeCache.GetTypesDerivedFrom<EffectComponent>().Where(t => !t.IsAbstract).OrderBy(t => t.Name))
                {
                    genericMenu.AddItem(NicifyComponentName(type.Name), false, () =>
                    {
                        var prop = serializedObject.FindProperty("_components");
                        prop.InsertArrayElementAtIndex(prop.arraySize);
                        var eprop = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                        var i = CreateInstance(type);
//                        (i as Effect).Lifetime = EffectLifetime.Inherit;
                        i.name = type.Name;
                        AssetDatabase.AddObjectToAsset(i, target);
                        eprop.objectReferenceValue = i;
                        serializedObject.ApplyModifiedProperties();
                        UpdateComponents();
                    }); ;
                }
                genericMenu.DropDown(addComponent.worldBound, addComponent, anchored: true);
            };

            var buttons = new VisualElement();
            buttons.name = "buttons";
            buttons.Add(addComponent);
            components.Add(buttons);

            return components;
        }

        private string NicifyComponentName(string name)
        {
            if (name.EndsWith("Component"))
                name = name.Substring(0, name.Length - 6);

            return ObjectNames.NicifyVariableName(name);
        }

        private void UpdateComponents ()
        {
            _componentsContent.Clear();
            _componentsContent.Unbind();

            var componentsProperty = serializedObject.FindProperty("_components");
            for (int i = 0; i < componentsProperty.arraySize; i++)
            {
                var componentProperty = componentsProperty.GetArrayElementAtIndex(i);
                var component = (EffectComponent)componentProperty.objectReferenceValue;
                var componentIndex = i;

                var componentItem = new VisualElement();
                componentItem.AddToClassList("elements__item");

                var foldout = new Foldout();
                foldout.text = NicifyComponentName(component.GetType().Name);
                foldout.value = false;

                var content = EffectComponentEditor.CreateEditor(new SerializedObject(component));
                content.AddToClassList("elements__item__content");

                var options = new VisualElement();
                options.AddToClassList("options-button");
                options.pickingMode = PickingMode.Position;
                options.AddManipulator(new Clickable(() =>
                {
                    var genericMenu = new GenericDropdownMenu();
                    if (componentIndex == 0)
                        genericMenu.AddDisabledItem("Move Up", false);
                    else
                        genericMenu.AddItem("Move Up", false, () => {
                            componentsProperty.MoveArrayElement(componentIndex, componentIndex - 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateComponents();
                        });

                    if (componentIndex >= componentsProperty.arraySize - 1)
                        genericMenu.AddDisabledItem("Move Down", false);
                    else
                        genericMenu.AddItem("Move Down", false, () => {
                            componentsProperty.MoveArrayElement(componentIndex, componentIndex + 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateComponents();
                        });
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem("Remove Effect", false, () => {
                        for (int i = componentsProperty.arraySize - 1; i >= 0; i--)
                        {
                            var conditionElement = componentsProperty.GetArrayElementAtIndex(i);
                            if (conditionElement.objectReferenceValue == component)
                            {
                                AssetDatabase.RemoveObjectFromAsset(component);
                                componentsProperty.DeleteArrayElementAtIndex(i);
                                componentItem.parent.Remove(componentItem);
                            }
                        }

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateComponents();
                    });
                    genericMenu.DropDown(options.worldBound, options, false);
                }));
                foldout.Q("unity-checkmark").parent.Add(options);

                foldout.Add(content);

                componentItem.Add(foldout);
                _componentsContent.Add(componentItem);
            }
        }
    }
}
