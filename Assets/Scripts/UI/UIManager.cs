using System;
using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("General")]
        [SerializeField] private GameObject _postProcsUI;
        [SerializeField] private GameObject _postProcsGame;

        [Header("Sounds")]
        [SerializeField] private AudioClip _clickSound;

        private VisualElement _root;
        private UIController _activeController;
        private bool _transitioning;

        private class UIController<T> where T : UIController
        {
            public static T Instance { get; set; }
            public static T Bind(VisualElement element) => Instance = element.Q<T>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement;
            doc.enabled = true;

            // Initialzie and bind all UIControllers
            _root.Query<UIController>().ForEach(c => c.Initialize());

            UIController<OptionsController>.Bind(_root).Hide();
            UIController<UIKeyboardControls>.Bind(_root).Hide();
            UIController<UIGamepadControls>.Bind(_root).Hide();
            UIController<ConfirmPopupController>.Bind(_root).Hide();
            UIController<CooperativeController>.Bind(_root).Hide();
            UIController<CooperativeJoinController>.Bind(_root).Hide();
            UIController<ConnectingController>.Bind(_root).Hide();
            UIController<UIGame>.Bind(_root).Hide();
            UIController<UIGameMenu>.Bind(_root).Hide();

            InputManager.Instance.OnUIClose += () => _activeController.OnNavigationBack();

            InputManager.Instance.EnableMenuActions();


            var title = UIController<TitleController>.Bind(_root);
            title.Show();
            _activeController = title;

            var blur = title.BlurBackground;
            _postProcsUI.SetActive(blur);
            _postProcsGame.SetActive(!blur);
        }

        public void ShowConfirmationPopup (string message, string yes=null, string no=null, string cancel=null, Action onYes=null, Action onNo=null, Action onCancel=null)
        {
            var popup = UIController<ConfirmPopupController>.Instance;
            popup.Message = message;
            popup.Yes = yes;
            popup.No = no;
            popup.OnYes = onYes;
            popup.OnNo = onNo;
            popup.OnCancel = onCancel;
            popup.Cancel = cancel;
            TransitionTo(UIController<ConfirmPopupController>.Instance);
        }

        public void ShowOptions (Action onBack=null)
        {
            var options = UIController<OptionsController>.Instance;
            if(onBack != null)
                options.OnBack = onBack;
            TransitionTo(options);
        }

        public void ShowKeyboardControls() =>
            TransitionTo(UIController<UIKeyboardControls>.Instance);

        public void ShowGamepadControls() =>
            TransitionTo(UIController<UIGamepadControls>.Instance);

        public void ShowTitle () =>
            TransitionTo(UIController<TitleController>.Instance);

        public void ShowCooperative () =>
            TransitionTo(UIController<CooperativeController>.Instance);

        public void ShowCooperativeJoin () =>
            TransitionTo(UIController<CooperativeJoinController>.Instance);        

        public void ShowConnecting () =>
            TransitionTo(UIController<ConnectingController>.Instance);

        public void ShowGame() =>
            TransitionTo(UIController<UIGame>.Instance);

        public void ShowGameMenu() =>
            TransitionTo(UIController<UIGameMenu>.Instance);

        private void TransitionTo(UIController controller, Action onDone=null)
        {
            if (_activeController == controller || _transitioning)
                return;


            _activeController.OnBeforeTransitionOut();
            controller.OnBeforeTransitionIn();

            var blur = controller.BlurBackground;
            _postProcsUI.SetActive(blur);
            _postProcsGame.SetActive(!blur);

            _transitioning = true;
            controller.style.opacity = 0;
            controller.Show();
            this.TweenGroup()
                .Element(_activeController.style.TweenFloat("opacity", new StyleFloat(0.0f)).Duration(0.2f).EaseInOutQuadratic())
                .Element(controller.style.TweenFloat("opacity", new StyleFloat(1.0f)).Duration(0.2f).EaseInOutQuadratic())
                .OnStop(() =>
                {
                    _activeController.OnAfterTransitionOut();
                    _activeController.Hide();
                    _activeController = controller;
                    controller.OnAfterTransitionIn();
                    _transitioning = false;
                    onDone?.Invoke();
                })
                .Play();
        }

        public void PlayClickSound ()
        {
            AudioManager.Instance.Play(_clickSound);
        }
    }
}
