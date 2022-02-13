using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class Panel : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Panel, PanelTraits> { }
        public class PanelTraits : UxmlTraits { }

        private VisualElement _items;

        public Panel ()
        {
            AddToClassList("zisle-panel");

            _items = this.Add<VisualElement>();
        }

        public void AddItem(VisualElement element) => _items.Add(element);
    }
}
