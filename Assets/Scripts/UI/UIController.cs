using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIController : VisualElement
    {
        public void Show()
        {
            parent.RemoveFromClassList("hidden");
        }

        public void Hide()
        {
            parent.AddToClassList("hidden");
        }

        public virtual void Initialize ()
        {

        }
    }
}
