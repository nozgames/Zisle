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
            _close.RemoveFromClassList(USS.Hidden);
            return this;
        }

        public string Title
        {
            get => _title.text;
            set
            {
                _title.text = value?.Localized();
                UpdateTitle();
            }            
        }

        public Panel ()
        {
            AddToClassList(USS.Panel);

            _title = hierarchy.Add<Label>().AddClass(USS.PanelTitle).AddClass(USS.Hidden);
            _items = hierarchy.Add<VisualElement>().AddClass(USS.PanelItems);
            _close = hierarchy.Add<RaisedButton>().AddClass(USS.PanelClose).AddClass(USS.ButtonRed).BindClick(() => _onClose?.Invoke());
            _close.Add<VisualElement>().AddClass(USS.PanelCloseIcon);
            _close.AddClass(USS.Hidden);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            _title.EnableInClassList(USS.Hidden, string.IsNullOrEmpty(_title.text));
        }

        public override VisualElement contentContainer => _items;

        public Panel SetTitle (string title)
        {
            Title = title;
            return this;
        }
    }
}
