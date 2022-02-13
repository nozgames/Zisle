using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class HealthCircle : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<HealthCircle, UxmlTraits> { }

        private float _health = 1.0f;

        public float Health
        {
            get => _health;
            set
            {
                _health = Mathf.Clamp01(value);
                var e = this.Q(className: "zisle-health-circle-health"); 
                if(e != null)
                    e.style.top = new StyleLength(new Length((1.0f - _health) * 100, LengthUnit.Percent));
            }
        }

        public HealthCircle()
        {
            AddToClassList("zisle-health-circle");

            var mask = new VisualElement();
            mask.AddToClassList("zisle-health-circle-mask");
            Add(mask);

            var health = new VisualElement();
            health.AddToClassList("zisle-health-circle-health");
            health.style.top = new StyleLength(new Length(0, LengthUnit.Percent));
            mask.Add(health);

            var highlight = new VisualElement();
            highlight.AddToClassList("zisle-health-circle-highlight");
            health.Add(highlight);

            var border = new VisualElement();
            border.AddToClassList("zisle-health-circle-border");
            Add(border);
        }
    }
}
