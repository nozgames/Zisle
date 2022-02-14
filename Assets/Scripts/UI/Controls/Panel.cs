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
        public class PanelTraits : UxmlTraits { }

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
                if (string.IsNullOrEmpty(value))
                    _title.AddToClassList("hidden");
                else
                {
                    _title.RemoveFromClassList("hidden");
                    _title.text = value;
                }                
            }            
        }

        public Panel ()
        {
            AddToClassList("zisle-panel");

            _title = this.Add<Label>().AddClass("zisle-panel-title").AddClass("hidden");
            _items = this.Add<VisualElement>().AddClass("zisle-panel-items");
            _close = this.Add<RaisedButton>().Text("X").AddClass("zisle-panel-close").AddClass("hidden").BindClick(() => _onClose?.Invoke());
        }

        public void AddItem(VisualElement element) => _items.Add(element);

        public TElement AddItem<TElement>(string name=null) where TElement : VisualElement, new() => 
            _items.Add<TElement>(name);

        public Panel SetTitle (string title)
        {
            Title = title;
            return this;
        }

    }
}
