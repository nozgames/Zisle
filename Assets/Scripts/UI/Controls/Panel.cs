using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class Panel : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Panel, PanelTraits> { }

        public class PanelTraits : UxmlTraits
        {
            UxmlStringAttributeDescription _title = new UxmlStringAttributeDescription { name = "title", defaultValue = "" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var panel = ve as Panel;
                panel._title.text = _title.GetValueFromBag(bag, cc).Localized();
            }
        }

        private VisualElement _items;
        private VisualElement _close;
        private Label _title;

        private Action _onClose;

        public Panel OnClose (Action action)
        {
            _onClose = action;
            _close.RemoveFromClassList("hidden");
            return this;
        }

        public string Title
        {
            get => _title.text;
            set
            {
                _title.text = value.Localized();
                UpdateTitle();
            }            
        }

        public Panel ()
        {
            AddToClassList("zisle-panel");

            _title = base.hierarchy.Add<Label>().AddClass("zisle-panel-title").AddClass("hidden");
            _items = base.hierarchy.Add<VisualElement>().AddClass("zisle-panel-items");
            _close = base.hierarchy.Add<RaisedButton>().Text("X").AddClass("zisle-panel-close").AddClass("hidden").AddClass("zisle-button-red").BindClick(() => _onClose?.Invoke());

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (string.IsNullOrEmpty(_title.text))
                _title.AddToClassList("hidden");
            else
                _title.RemoveFromClassList("hidden");
        }

        public override VisualElement contentContainer => _items;

        public Panel SetTitle (string title)
        {
            Title = title;
            return this;
        }

    }
}
