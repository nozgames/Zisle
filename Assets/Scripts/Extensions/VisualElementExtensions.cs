using NoZ.Zisle.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public static class VisualElementExtensions
    {
        public static VisualElement BindClick (this VisualElement element, System.Action action = null)
        {
            if (null == element)
                return null;

            if (element is Button button)
                button.clicked += () =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                };
            else if (element is RaisedButton raisedButton)
            {
                raisedButton.clicked += () =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                };
            }
            else
                element.AddManipulator(new Clickable((e) =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                }));

            return element;
        }

        public static TElement BindClick<TElement>(this TElement element, System.Action action = null) where TElement : VisualElement =>
            BindClick(element as VisualElement, action) as TElement;

        public static TElement LocalizedText<TElement> (this TElement element, string key) where TElement : VisualElement
        {
            if(element is TextElement textElement)
                textElement.Text(key.Localized());
            else if(element is RaisedButton raisedButton)
                raisedButton.text = key.Localized();

            return element;
        }

        public static RaisedButton Text(this RaisedButton element, string text)
        {
            element.text = text;
            return element;
        }

    }
}
