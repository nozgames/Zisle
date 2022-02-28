using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(Ability))]
    public class AbilityEditor : Editor
    {
        private VisualElement _root = null;
        private VisualElement _eventsContent = null;
        private VisualElement _conditionsContent = null;
        private VisualElement _inspector = null;
        private Ability _ability = null;

        public void OnEnable()
        {
            _ability = (Ability)target;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public void OnDisable()
        {
            EditorApplication.update -= OnNextUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            UpdateConditions();
            UpdateEvents();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList("root");
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CommonEditor.uss"));
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/AbilityEditor.uss"));

            // Create the inspector for the ability properties
            _inspector = EditorHelpers.CreateInspector(serializedObject, (p) => p.name != "_conditions" && p.name != "_events");
            EditorHelpers.HandleTargetType(serializedObject, _inspector);
            root.Add(_inspector);

            // Create tabs
            var tabs = new TabbedView();
            tabs.name = "tabs";
            tabs.Add(new TabButton("Conditions", CreateConditionsTab()));
            tabs.Add(new TabButton("Events", CreateEventsTab()));
            root.Add(tabs);

            EditorApplication.update += OnNextUpdate;

            _root = root;

            return root;
        }

        private void OnNextUpdate()
        {
            EditorApplication.update -= OnNextUpdate;

            _root.Unbind();
            _inspector.Bind(serializedObject);

            UpdateEvents();
            UpdateConditions();
        }

        private VisualElement CreateConditionsTab()
        {
            var conditions = new VisualElement();
            conditions.name = "conditions";

            _conditionsContent = new VisualElement();
            _conditionsContent.name = "conditions__content";
            conditions.Add(_conditionsContent);

            var addCondition = new Button();
            addCondition.AddToClassList("add-button");
            addCondition.text = "Add Condition";
            addCondition.clicked += () =>
            {
                var genericMenu = new GenericDropdownMenu();

                foreach (var type in TypeCache.GetTypesDerivedFrom<AbilityCondition>().Where(t => !t.IsAbstract))
                {
                    genericMenu.AddItem(EditorHelpers.NicifyConditionName(type.Name), false, () =>
                    {
                        var conditionsProperty = serializedObject.FindProperty("_conditions");
                        conditionsProperty.InsertArrayElementAtIndex(conditionsProperty.arraySize);
                        var elementProperty = conditionsProperty.GetArrayElementAtIndex(conditionsProperty.arraySize - 1);
                        var condition = CreateInstance(type) as AbilityCondition;
                        condition.name = type.Name;
                        AssetDatabase.AddObjectToAsset(condition, _ability);
                        elementProperty.objectReferenceValue = condition;
                        serializedObject.ApplyModifiedProperties();
                        UpdateConditions();
                    }); ;
                }
                genericMenu.DropDown(addCondition.worldBound, addCondition, anchored: true);
            };
            conditions.Add(addCondition);

            return conditions;
        }

        private void UpdateConditions()
        {
            _conditionsContent.Clear();
            _conditionsContent.Unbind();

            var conditionsProperty = serializedObject.FindProperty("_conditions");
            for (int i = 0; i < conditionsProperty.arraySize; i++)
            {
                var conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
                var condition = (AbilityCondition)conditionProperty.objectReferenceValue;
                var conditionIndex = i;

                var conditionItem = new VisualElement();
                conditionItem.AddToClassList("elements__item");
                var foldout = new Foldout();
                foldout.text = EditorHelpers.NicifyConditionName(condition.GetType().Name);

                var content = new VisualElement();
                content.AddToClassList("elements__item__content");

                var options = new VisualElement();
                options.AddToClassList("options-button");
                options.pickingMode = PickingMode.Position;
                options.AddManipulator(new Clickable(() =>
                {
                    var genericMenu = new GenericDropdownMenu();
                    if (conditionIndex == 0)
                        genericMenu.AddDisabledItem("Move Up", false);
                    else
                        genericMenu.AddItem("Move Up", false, () => {
                            conditionsProperty.MoveArrayElement(conditionIndex, conditionIndex - 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateConditions();
                        });

                    if (conditionIndex >= conditionsProperty.arraySize - 1)
                        genericMenu.AddDisabledItem("Move Down", false);
                    else
                        genericMenu.AddItem("Move Down", false, () => {
                            conditionsProperty.MoveArrayElement(conditionIndex, conditionIndex + 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateConditions();
                        });
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem("Remove Condition", false, () => {
                        var conditionsProperty = serializedObject.FindProperty("_conditions");
                        for (int i = conditionsProperty.arraySize - 1; i >= 0; i--)
                        {
                            var conditionElement = conditionsProperty.GetArrayElementAtIndex(i);
                            if (conditionElement.objectReferenceValue == condition)
                            {
                                AssetDatabase.RemoveObjectFromAsset(condition);
                                conditionsProperty.DeleteArrayElementAtIndex(i);
                                conditionItem.parent.Remove(conditionItem);
                            }
                        }

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateConditions();
                    });
                    genericMenu.DropDown(options.worldBound, options, false);
                }));
                foldout.Q("unity-checkmark").parent.Add(options);

                foldout.Add(content);

                // Create the inspector for the condition
                var serializedCondition = new SerializedObject(condition);
                var inspector = ConditionEditor.CreateEditor(serializedCondition);
                content.Add(inspector);

                conditionItem.Add(foldout);
                _conditionsContent.Add(conditionItem);
            }
        }

        private VisualElement CreateEventsTab()
        {
            var events = new VisualElement();
            events.name = "events";

            var addEvent = new Button();
            addEvent.AddToClassList("add-button");
            addEvent.text = "Add Event";
            addEvent.clicked += () =>
            {
                var genericMenu = new GenericDropdownMenu();

                var existingEvents = _ability.Events?.Where(e => e != null).Select(e => e.Event);
                var events = EditorHelpers.FindAssetsByType<Animations.AnimationEvent>()
                    .Where(e => existingEvents == null || !existingEvents.Contains(e));

                foreach (var evt in events)
                    genericMenu.AddItem(EditorHelpers.NicifyEventName(evt.name), false, () =>
                    {
                        var eventHandlersProperty = serializedObject.FindProperty("_events");
                        eventHandlersProperty.InsertArrayElementAtIndex(eventHandlersProperty.arraySize);
                        var eventHandlerProperty = eventHandlersProperty.GetArrayElementAtIndex(eventHandlersProperty.arraySize - 1);

                        var eventHandler = CreateInstance<AbilityEvent>();
                        eventHandler.name = evt.name;
                        eventHandler.Event = evt;
                        AssetDatabase.AddObjectToAsset(eventHandler, _ability);
                        eventHandlerProperty.objectReferenceValue = eventHandler;
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateEvents();
                    });
                genericMenu.DropDown(addEvent.worldBound, addEvent, anchored: true);
            };

            _eventsContent = new VisualElement();
            _eventsContent.name = "events__content";
            events.Add(_eventsContent);

            events.Add(addEvent);

            return events;
        }

        private void UpdateEvents()
        {
            _eventsContent.Clear();
            _eventsContent.Unbind();

            var abilityEventsProperty = serializedObject.FindProperty("_events");
            for(int i=0; i<abilityEventsProperty.arraySize; i++)
            {
                var abilityEventProperty = abilityEventsProperty.GetArrayElementAtIndex(i);
                var abilityEvent = abilityEventProperty.objectReferenceValue as AbilityEvent;
                if (null == abilityEvent)
                    continue;

                var abilityEventSerializedObject = new SerializedObject(abilityEvent);
                abilityEventSerializedObject.Update();
                var animationEvent = abilityEvent.Event;
                if (animationEvent == null)
                    continue;

                var element = new VisualElement();
                element.AddToClassList("elements__item");
                var foldout = new Foldout();
                foldout.text = EditorHelpers.NicifyEventName(animationEvent.name);

                var content = new VisualElement();
                content.AddToClassList("elements__item__content");

                var eventIndex = i;
                var options = new VisualElement();
                options.AddToClassList("options-button");
                options.pickingMode = PickingMode.Position;
                options.AddManipulator(new Clickable(() =>
                {
                    var genericMenu = new GenericDropdownMenu();
                    if (eventIndex == 0)
                        genericMenu.AddDisabledItem("Move Up", false);
                    else
                        genericMenu.AddItem("Move Up", false, () => {
                            abilityEventsProperty.MoveArrayElement(eventIndex, eventIndex - 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateEvents();
                        });

                    if (eventIndex >= abilityEventsProperty.arraySize - 1)
                        genericMenu.AddDisabledItem("Move Down", false);
                    else
                        genericMenu.AddItem("Move Down", false, () => {
                            abilityEventsProperty.MoveArrayElement(eventIndex, eventIndex + 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateEvents();
                        });
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem("Remove Event", false, () => {
                        var abilityEventsProperty = serializedObject.FindProperty("_events");
                        for (int i = abilityEventsProperty.arraySize - 1; i >= 0; i--)
                        {
                            var abilityEventProperty = abilityEventsProperty.GetArrayElementAtIndex(i);
                            var abilityEvent = abilityEventProperty.objectReferenceValue as AbilityEvent;
                            if (abilityEvent.Event == animationEvent)
                            {
                                AssetDatabase.RemoveObjectFromAsset(abilityEvent);
                                abilityEventsProperty.DeleteArrayElementAtIndex(i);
                            }
                        }

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateEvents();
                    });
                    genericMenu.DropDown(options.worldBound, options, false);
                }));
                foldout.Q("unity-checkmark").parent.Add(options);

                // Inspector for Event properties
                var inspector = EditorHelpers.CreateInspector(abilityEventSerializedObject, (p) => p.name != "_event");
                EditorHelpers.HandleTargetType(abilityEventSerializedObject, inspector);
                content.Add(inspector);

                foldout.Add(content);                
                element.Add(foldout);
                _eventsContent.Add(element);
            }
        }
    }
}
