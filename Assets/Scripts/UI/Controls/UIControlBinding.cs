using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIControlBinding : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIControlBinding, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _ActionMap = new UxmlStringAttributeDescription { name = "action-map", defaultValue = "" };
            UxmlStringAttributeDescription _ActionName = new UxmlStringAttributeDescription { name = "action-name", defaultValue = "" };
            UxmlIntAttributeDescription _BindingCount = new UxmlIntAttributeDescription { name = "binding-count", defaultValue = 2 };
            UxmlIntAttributeDescription _BindingOffset = new UxmlIntAttributeDescription { name = "binding-offset", defaultValue = 0 };
            UxmlBoolAttributeDescription _Gamepad = new UxmlBoolAttributeDescription { name = "gamepad", defaultValue = false };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var binding = ve as UIControlBinding;

                binding.Clear();

                var group = new VisualElement();
                group.AddToClassList("zisle-control-binding");

                var label = new Label();
                label.text = _ActionName.GetValueFromBag(bag, cc);
                label.AddToClassList("zisle-control-binding__name");
                group.Add(label);

                var multipleBindings = new VisualElement();
                multipleBindings.AddToClassList("zisle-control-binding__bindings");

                var bindingCount = _BindingCount.GetValueFromBag(bag, cc);
                var bindingOffset = _BindingOffset.GetValueFromBag(bag, cc);
                var actionMap = _ActionMap.GetValueFromBag(bag, cc);
                var actionName = _ActionName.GetValueFromBag(bag, cc);
                var gamepad = _Gamepad.GetValueFromBag(bag, cc);
                for (int i=0; i<bindingCount; i++)
                {
                    var bindingIndex = bindingOffset + i;
                    var bindingGroup = new VisualElement();
                    bindingGroup.AddToClassList("zisle-control-binding__binding");

                    var bindingButton = new Button();
                    var bindingLabel = new Label();
                    bindingButton.Add(bindingLabel);

                    bindingButton.RegisterCallback<GeometryChangedEvent>((evt) =>
                    {
                        if (InputManager.Instance == null)
                        {
                            bindingLabel.text = "A";
                            return;
                        }
                        UpdateBinding(bindingLabel, actionMap, actionName, bindingIndex);
                    });
                    bindingButton.AddToClassList("zisle-control-binding__button");
                    bindingGroup.Add(bindingButton);

                    var waitForButton = new Label();
                    waitForButton.text = "Press any Key";
                    waitForButton.AddToClassList("zisle-control-binding__wait");
                    waitForButton.AddToClassList("hidden");
                    bindingGroup.Add(waitForButton);

                    multipleBindings.Add(bindingGroup);

                    bindingButton.clicked += () =>
                    {
                        bindingButton.AddToClassList("hidden");
                        waitForButton.RemoveFromClassList("hidden");

                        InputManager.Instance.PerformInteractiveRebinding(actionMap, actionName, bindingIndex, gamepad: gamepad, onComplete: () =>
                        {
                            bindingButton.RemoveFromClassList("hidden");
                            waitForButton.AddToClassList("hidden");
                            UpdateBinding(bindingLabel, actionMap, actionName, bindingIndex);
                        });
                    };
                }

                group.Add(multipleBindings);
                binding.Add(group);
            }

            private void UpdateBinding (Label label, string actionMap, string actionName, int bindingIndex)
            {
                // Remove the old binding text class
                label.RemoveFromClassList($"zisle-control-binding__binding__{label.text.Replace(' ', '_').Replace('/', '_').ToLower()}");

                label.text = InputManager.Instance.GetBinding(actionMap, actionName, bindingIndex);

                label.AddToClassList($"zisle-control-binding__binding__{label.text.Replace(' ', '_').Replace('/', '_').ToLower()}");
            }
        }
    }
}
