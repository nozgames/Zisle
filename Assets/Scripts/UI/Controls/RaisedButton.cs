using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public enum RaisedButtonColor
    {
        Red,
        Orange,
        Blue
    }

    public class RaisedButton : BindableElement
    {
        public new class UxmlFactory : UxmlFactory<RaisedButton, RaisedButtonTraits> 
        { 
        }

        public class RaisedButtonTraits : UxmlTraits
        {
            UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text", defaultValue = "" };
            UxmlEnumAttributeDescription<RaisedButtonColor> _color = new UxmlEnumAttributeDescription<RaisedButtonColor> { name = "color", defaultValue = RaisedButtonColor.Blue };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var button = ve as RaisedButton;
                button._text.text = _text.GetValueFromBag(bag, cc).Localized();
                button.Color = _color.GetValueFromBag(bag, cc);
            }
        }

        private Clickable m_Clickable;
        private Label _text;

        public string text
        {
            get => _text.text;
            set => _text.text = value;
        }

        public Clickable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);

                m_Clickable = value;

                if (m_Clickable != null)
                    this.AddManipulator(m_Clickable);
            }
        }

        public event Action clicked
        {
            add
            {
                if (m_Clickable == null)
                    clickable = new Clickable(value);
                else
                    m_Clickable.clicked += value;
            }
            remove
            {
                if (m_Clickable != null)
                    m_Clickable.clicked -= value;
            }
        }

        private RaisedButtonColor _color = RaisedButtonColor.Red;

        public RaisedButtonColor Color
        {
            get => _color;
            set
            {
                _color = value;

#if false
                RemoveFromClassList("zisle-button-red");
                RemoveFromClassList("zisle-button-orange");
                RemoveFromClassList("zisle-button-blue");

                switch(value)
                {
                    case RaisedButtonColor.Red: AddToClassList("zisle-button-red"); break;
                    case RaisedButtonColor.Orange: AddToClassList("zisle-button-orange"); break;
                    case RaisedButtonColor.Blue: AddToClassList("zisle-button-blue"); break;
                }
#endif
            }
        }

        public RaisedButton SetColor (RaisedButtonColor color)
        {
            Color = color;
            return this;
        }

        public RaisedButton() : this(RaisedButtonColor.Red, null) { }

        public RaisedButton(RaisedButtonColor color) : this(color, null) { }

        public RaisedButton(RaisedButtonColor color, Action action)
        {
            AddToClassList("zisle-button");
            
            clickable = new Clickable(action);
            focusable = true;
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            var shadow = this.Add<VisualElement>().AddClass("zisle-button-shadow");
            var raised = this.Add<VisualElement>().AddClass("zisle-button-raised");

            _text = this.Add<Label>();

            Color = color;
        }



        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            var method = m_Clickable.GetType().GetMethod("SimulateSingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(m_Clickable, new object[] {evt, 100});
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            IPanel panel = base.panel;
            if (panel != null && panel.contextType == ContextType.Editor && (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space))
            {
                var method = m_Clickable.GetType().GetMethod("SimulateSingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(m_Clickable, new object[] { evt, 100 });
                evt.StopPropagation();
            }
        }

        protected override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
        {
            string text = this.text;
            if (string.IsNullOrEmpty(text))
                text = " ";

            return _text.MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
        }
    }
}
