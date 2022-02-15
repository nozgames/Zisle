using NoZ.Zisle.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [RequireComponent(typeof(UIDocument))]
    public class UIScreen : MonoBehaviour
    {
        public virtual bool BlurBackground => true;
        public VisualElement Root { get; private set; }

        public virtual void OnBeforeTransitionOut() { }
        public virtual void OnBeforeTransitionIn() { }
        public virtual void OnAfterTransitionOut() { }
        public virtual void OnAfterTransitionIn() { }

        public virtual void OnNavigationBack() { }

        public virtual void OnShow() { }
        public virtual void OnHide() { }

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
            Root = GetComponent<UIDocument>().rootVisualElement;
        }

        public VisualElement BindClick(VisualElement element, System.Action action = null)
        {
            if (null == element)
                return null;

            if (element is Button button)
                button.clicked += () =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                };
            else if (element is RaisedButton raisedButton)
            {
                raisedButton.clicked += () =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                };
            }
            else
                element.AddManipulator(new Clickable((e) =>
                {
                    AudioManager.Instance.PlayButtonClick();
                    action?.Invoke();
                }));

            return element;
        }

        public VisualElement BindClick(string name, System.Action action = null)
        {
            return BindClick(Root.Q(name), action);
        }

        public TElement BindClick<TElement>(string name, System.Action action = null) where TElement : VisualElement
        {
            return BindClick(Root.Q(name), action) as TElement;
        }

        public VisualElement Q(string name) => Root.Q(name);
        public TElement Q<TElement>(string name) where TElement : VisualElement => Root.Q<TElement>(name);
    }
}
