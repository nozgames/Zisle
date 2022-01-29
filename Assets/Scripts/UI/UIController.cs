using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIController : VisualElement
    {
        public virtual void Show() => parent.RemoveFromClassList("hidden");
        public virtual void Hide() => parent.AddToClassList("hidden");

        public virtual void Initialize () { }

        public VisualElement BindClick (string name, System.Action action=null)
        {
            var element = this.Q(name);
            if (element is Button button)
                button.clicked += () =>
                {
                    UIManager.Instance.PlayClickSound();
                    action?.Invoke();
                };
            else
                element.AddManipulator(new Clickable((e) =>
                {
                    UIManager.Instance.PlayClickSound();
                    action?.Invoke();
                }));

            return element;
        }
    }
}
