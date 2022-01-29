using System;
using UnityEngine.UIElements;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class UIManager : Singleton<UIManager>
    {
        private VisualElement _root;
        private UIController _activeController;
        private bool _transitioning;

        private class UIController<T> where T : UIController
        {
            public static T instance { get; set; }
            public static T Bind(VisualElement element) => instance = element.Q<T>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();            

            _root = GetComponent<UIDocument>().rootVisualElement;
            
            // Initialzie and bind all UIControllers
            _root.Query<UIController>().ForEach(c => c.Initialize());

            var title = UIController<TitleController>.Bind(_root);
            var options = UIController<OptionsController>.Bind(_root);
            var confirmPopup = UIController<ConfirmPopupController>.Bind(_root);

            options.Hide();
            confirmPopup.Hide();

            title.Show();
            _activeController = title;
        }

        public void ShowConfirmationPopup (string message, string yes=null, string no=null, Action onYes=null, Action onNo=null)
        {
            var popup = UIController<ConfirmPopupController>.instance;
            popup.Message = message;
            popup.Yes = yes;
            popup.No = no;
            popup.OnYes = onYes;
            popup.OnNo = onNo;
            TransitionTo(UIController<ConfirmPopupController>.instance);
        }

        public void ShowOptions ()
        {
            TransitionTo(UIController<OptionsController>.instance);
        }

        public void ShowTitle ()
        {
            TransitionTo(UIController<TitleController>.instance);
        }

        private void HidePopup ()
        {
        }

        private void TransitionTo(UIController controller)
        {
            if (_activeController == controller || _transitioning)
                return;

            _transitioning = true;
            controller.style.opacity = 0;
            controller.Show();
            this.TweenGroup()
                .Element(_activeController.style.TweenFloat("opacity", new StyleFloat(0.0f)).Duration(0.2f).EaseInOutQuadratic())
                .Element(controller.style.TweenFloat("opacity", new StyleFloat(1.0f)).Duration(0.2f).EaseInOutQuadratic())
                .OnStop(() =>
                {
                    _activeController.Hide();
                    _activeController = controller;
                    _transitioning = false;
                })
                .Play();
        }
    }
}
