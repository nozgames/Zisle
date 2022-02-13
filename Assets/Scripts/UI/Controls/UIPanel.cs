using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIPanel : Button
    {
        public new class UxmlFactory : UxmlFactory<UIPanel, PanelTraits> { }

        public class PanelTraits : Button.UxmlTraits
        {
            UxmlStringAttributeDescription _buttonText = new UxmlStringAttributeDescription { name = "button-text", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var shadow = new VisualElement();
                shadow.AddToClassList("zisle-button-shadow");
                ve.Add(shadow);

                var raised = new VisualElement();
                raised.AddToClassList("zisle-button-raised");
                ve.Add(raised);

                var buttonText = _buttonText.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(buttonText))
                {
                    var text = new Label();
                    text.AddToClassList("zisle-button-text");
                    text.text = buttonText;
                    ve.Add(text);
                }
            }
        }

        public UIPanel ()
        { 
        }
    }
}
