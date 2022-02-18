using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class RaisedButton : BindableElement
    {
        public new class UxmlFactory : UxmlFactory<RaisedButton, RaisedButtonTraits> 
        { 
        }

        public class RaisedButtonTraits : UxmlTraits
        {
            UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text", defaultValue = "" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var button = ve as RaisedButton;
                button._text.text = _text.GetValueFromBag(bag, cc).Localized();
            }
        }

        private Clickable m_Clickable;
        private Label _text;

        public string text
        {
            get => _text.text;
            set => _text.text = value;
        }

        public event Action clicked;

        public RaisedButton() : this(null) { }

        public RaisedButton(Action action)
        {
            m_Clickable = new Clickable(OnClicked);

            AddToClassList(USS.Button);
            this.AddManipulator(m_Clickable);

            focusable = true;
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            _text = this.Add<VisualElement>().AddClass(USS.ButtonRaised).Add<Label>().AddClass(USS.ButtonText);
        }

        private void OnClicked()
        {
            AudioManager.Instance.PlayButtonClickSound();
            clicked?.Invoke();
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            var method = m_Clickable.GetType().GetMethod("SimulateSingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(m_Clickable, new object[] {evt, 100});
            evt.StopPropagation();

            AudioManager.Instance.PlayButtonClickSound();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            IPanel panel = base.panel;
            if (panel != null && panel.contextType == ContextType.Editor && (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space))
            {
                var method = m_Clickable.GetType().GetMethod("SimulateSingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(m_Clickable, new object[] { evt, 100 });
                AudioManager.Instance.PlayButtonClickSound();
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
