using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIController : VisualElement
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
                if (_visible == value)
                    return;

                _visible = value;
                parent.EnableInClassList("hidden", !_visible);

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

    public class UIController<T> where T : UIController
    {
        public static T Instance { get; set; }
        public static T Bind(VisualElement element) => Instance = element.Q<T>();
    }
}
