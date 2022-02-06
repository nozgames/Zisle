using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIController : VisualElement
    {
        public virtual bool BlurBackground => true;

        public virtual void Show() => parent.RemoveFromClassList("hidden");
        public virtual void Hide() => parent.AddToClassList("hidden");

        public virtual void Initialize () { }

        public VisualElement BindClick (string name, System.Action action=null)
        {
            var element = this.Q(name);
            if (element is Button button)
                button.clicked += () =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                };
            else
                element.AddManipulator(new Clickable((e) =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                }));

            return element;
        }

        public virtual void OnBeforeTransitionOut() { }
        public virtual void OnBeforeTransitionIn() { }
        public virtual void OnAfterTransitionOut() { }
        public virtual void OnAfterTransitionIn() { }

        public virtual void OnNavigationBack() { }
    }

    public class UIController<T> where T : UIController
    {
        public static T Instance { get; set; }
        public static T Bind(VisualElement element) => Instance = element.Q<T>();
    }
}
