using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIControlBinding : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UIControlBinding, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _ActionMap = new UxmlStringAttributeDescription { name = "action-map", defaultValue = "" };
            UxmlStringAttributeDescription _ActionName = new UxmlStringAttributeDescription { name = "action-name", defaultValue = "" };

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

                for(int i=0; i<2; i++)
                {
                    var bindingIndex = i;
                    var bindingGroup = new VisualElement();
                    bindingGroup.AddToClassList("zisle-control-binding__binding");

                    var bindingButton = new Button();
                    bindingButton.RegisterCallback<GeometryChangedEvent>((evt) =>
                    {
                        if (InputManager.Instance == null)
                        {
                            bindingButton.text = "A";
                            return;
                        }
                        bindingButton.text = InputManager.Instance.GetBinding(
                            _ActionMap.GetValueFromBag(bag, cc),
                            _ActionName.GetValueFromBag(bag, cc),
                            bindingIndex);
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

                        InputManager.Instance.PerformInteractiveRebinding(
                            _ActionMap.GetValueFromBag(bag, cc),
                            _ActionName.GetValueFromBag(bag, cc),
                            bindingIndex,
                            onComplete: () =>
                            {
                                bindingButton.RemoveFromClassList("hidden");
                                waitForButton.AddToClassList("hidden");
                                bindingButton.text = InputManager.Instance.GetBinding(
                                    _ActionMap.GetValueFromBag(bag, cc),
                                    _ActionName.GetValueFromBag(bag, cc),
                                    bindingIndex);
                            }
                            );
                    };
                }

                group.Add(multipleBindings);
                binding.Add(group);
            }
        }
    }
}
