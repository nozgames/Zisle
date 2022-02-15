using NoZ.Zisle.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    /// <summary>
    /// Represents a user interface screen
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIScreen : MonoBehaviour
    {
        /// <summary>
        /// True if the UI post procs should be used for this screen
        /// </summary>
        public virtual bool BlurBackground => true;

        /// <summary>
        /// Root visual element for this screen
        /// </summary>
        public VisualElement Root { get; private set; }

        public virtual void OnBeforeTransitionOut() { }
        public virtual void OnBeforeTransitionIn() { }
        public virtual void OnAfterTransitionOut() { }
        public virtual void OnAfterTransitionIn() { }

        /// <summary>
        /// Called when the "back" button is pressed on the keyboard or gamepad
        /// </summary>
        public virtual void OnNavigationBack() { }

        /// <summary>
        /// Get / Set the visible state of the screen
        /// </summary>
        public bool IsVisible
        {
            get => Root.visible;
            set
            {
                if (value == Root.visible)
                    return;

                Root.style.visibility = value ? Visibility.Visible : Visibility.Hidden;

                if (value)
                {
                    OnShow();
                    StartCoroutine(LateShow());
                }
                else
                    OnHide();
            }
        }

        private IEnumerator LateShow()
        {
            yield return new WaitForEndOfFrame();
            OnLateShow();
        }

        /// <summary>
        /// Set the visible state of the screen without calling OnShow or OnHide
        /// </summary>
        public void SetVisibleNoCallback(bool visible) => Root.visible = visible;

        /// <summary>
        /// Called when IsVisible is set to true
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// Called at the end of the frame in which IsVisible was set to true
        /// </summary>
        protected virtual void OnLateShow() { }

        /// <summary>
        /// Called when IsVisible is set to false
        /// </summary>
        protected virtual void OnHide() { }
        

        protected virtual void Awake()
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
                    action?.Invoke();
                };
            else if (element is RaisedButton raisedButton)
            {
                raisedButton.clicked += () =>
                {
                    action?.Invoke();
                };
            }
            else
                element.AddManipulator(new Clickable((e) =>
                {
                    action?.Invoke();
                }));

            return element;
        }

        /// <summary>
        /// Bind an element to the click sound and given action
        /// </summary>
        public VisualElement BindClick(string name, System.Action action = null) =>
            BindClick(Root.Q(name), action);

        /// <summary>
        /// Bind an element to the click sound and given action
        /// </summary>
        public TElement BindClick<TElement>(string name, System.Action action = null) where TElement : VisualElement =>
            BindClick(Root.Q(name), action) as TElement;

        /// <summary>
        /// Return an element with the given name
        /// </summary>
        public VisualElement Q(string name) => Root.Q(name);

        /// <summary>
        /// Return an element with the given name and type
        /// </summary>
        public TElement Q<TElement>(string name) where TElement : VisualElement => Root.Q<TElement>(name);
    }
}
