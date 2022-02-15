using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Tweening;

namespace NoZ.Zisle.UI
{
    public class UIGame : UIScreen
    {
        public override bool BlurBackground => false;

        private class FloatingText
        {
            public Vector3 Position { get; set; }
            public VisualElement Element;
            public float Opacity { get; set; }
            public Tween Tween;
        }

        private VisualElement _floatingTextContainer;
        private List<FloatingText> _floatingText = new List<FloatingText>();
        private List<WorldVisualElement> _worldElements = new List<WorldVisualElement>();

        protected override void Awake()
        {
            base.Awake();
            _floatingTextContainer = this.Q("floating-text-container");

            WorldVisualElement.Root = _floatingTextContainer;
        }
    
        protected override void OnShow ()
        {
            base.OnShow();

            GameManager.Instance.Resume();

            UIManager.Instance.StartCoroutine(UpdateFloatingText());
            UIManager.Instance.StartCoroutine(UpdateWorldElements());
        }

        protected override void OnHide()
        {
            base.OnHide();
            GameManager.Instance.Pause();
        }

        public void AddFloatingText (string text, string className, Vector3 position, float duration=1.0f)
        {
            var label = new Label(text);
            if(className != null)
                label.AddToClassList(className);
            
            var ft = new FloatingText { Opacity = 1.0f, Element = label, Position = position };
            _floatingText.Add(ft);
            _floatingTextContainer.Add(label);

            ft.Tween = ft.TweenGroup()
                .Element(ft.TweenVector("Position", position + Vector3.up).EaseInCubic().Duration(duration))
                .Element(ft.TweenFloat("Opacity", 0.0f).Delay(duration * 0.5f).EaseOutQuadratic().Duration(duration*0.5f))
                .Play();
        }        

        private IEnumerator UpdateWorldElements()
        {
            // Update until the game shuts down
            var wait = new WaitForEndOfFrame();
            while (UIManager.IsInitialized)
            {
                yield return wait;
                WorldVisualElement.UpdateElements();
            }
        }

        private IEnumerator UpdateFloatingText ()
        {
            var camera = Camera.main;
            while(UIManager.IsInitialized)
            {
                for (int i = 0; i < _floatingText.Count; i++)
                {
                    var ft = _floatingText[i];
                    if(!ft.Tween.isPlaying)
                    {
                        _floatingText.RemoveAt(i);
                        _floatingTextContainer.Remove(ft.Element);
                        i--;
                        continue;
                    }

                    var screen = camera.WorldToViewportPoint(ft.Position);

                    ft.Element.style.top = new StyleLength(new Length((1-screen.y) * 100, LengthUnit.Percent));
                    ft.Element.style.left = new StyleLength(new Length(screen.x * 100, LengthUnit.Percent));
                    ft.Element.style.opacity = ft.Opacity;
                }

                yield return null;
            }
        }

        public void Add (WorldVisualElement worldElement)
        {
            _worldElements.Add(worldElement);
            _floatingTextContainer.Add(worldElement.VisualElement);
        }
    }
}
