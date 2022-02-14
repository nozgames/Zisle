using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class ScreenElement : VisualElement
    {
        private bool _visible;

        public virtual bool BlurBackground => true;

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
        
        public void Toggle() => IsVisible = !IsVisible;

        public virtual void Initialize () { }

        public bool IsVisible
        {
            get => _visible;
            set
            {
                parent.EnableInClassList("hidden", !value);

                if (_visible == value)
                    return;

                _visible = value;

                if (value)
                    OnShow();
                else
                    OnHide();
            }
        }

        public virtual void OnBeforeTransitionOut() { }
        public virtual void OnBeforeTransitionIn() { }
        public virtual void OnAfterTransitionOut() { }
        public virtual void OnAfterTransitionIn() { }

        public virtual void OnNavigationBack() { }

        public virtual void OnShow () { }
        public virtual void OnHide () { }
    }

    public class ScreenElement<T> where T : ScreenElement
    {
        public static T Instance { get; set; }
        public static T Bind(VisualElement element) => Instance = element.Q<T>();
    }
}
