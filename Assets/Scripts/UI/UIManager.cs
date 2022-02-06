using System;
using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Tweening;
using NoZ.Events;
using System.Collections;

namespace NoZ.Zisle
{
    public class UIManager : Singleton<UIManager>
    {
#if UNITY_EDITOR
        private const float MinLoadingTime = 0.0f;
#else
        private const float MinLoadingTime = 2.0f;
#endif

        [Header("General")]
        [SerializeField] private GameObject _postProcsUI = null;
        [SerializeField] private GameObject _postProcsGame = null;
        [SerializeField] private GameObject _lobbyPrefab = null;

        [Header("Preview")]
        [SerializeField] private RenderTexture _previewLeftTexture = null;
        [SerializeField] private RenderTexture _previewRightTexture = null;
        [SerializeField] private GameObject _previewLeft = null;
        [SerializeField] private GameObject _previewRight = null;

        private VisualElement _root;
        private UIController _activeController;
        private bool _transitioning;
        private GameObject _lobbyIsland;

        public Texture PreviewLeftTexture => _previewLeftTexture;
        public Texture PreviewRightTexture => _previewRightTexture;

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
            UIController<UILoadingController>.Bind(_root).Hide();
            UIController<UIGame>.Bind(_root).Hide();
            UIController<UIGameMenu>.Bind(_root).Hide();
            UIController<UITitleController>.Bind(_root).Hide();
            UIController<UILobbyController>.Bind(_root).Hide();

            InputManager.Instance.OnUIClose += () => _activeController.OnNavigationBack();

            InputManager.Instance.EnableMenuActions();

            var loading = UIController<UILoadingController>.Bind(_root);
            loading.Show();
            loading.OnBeforeTransitionIn();
            loading.OnAfterTransitionIn();
            _activeController = loading;

            var blur = loading.BlurBackground;
            _postProcsUI.SetActive(blur);
            _postProcsGame.SetActive(!blur);

            GameEvent<GameStateChanged>.OnRaised += OnGameStateChanged;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            GameEvent<GameStateChanged>.OnRaised -= OnGameStateChanged;
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
            TransitionTo(UIController<UITitleController>.Instance);

        public void ShowCooperative () =>
            TransitionTo(UIController<CooperativeController>.Instance);

        public void ShowCooperativeJoin () =>
            TransitionTo(UIController<CooperativeJoinController>.Instance);

        public void ShowLoading (WaitForDone wait = null) =>
            TransitionTo(UIController<UILoadingController>.Instance, wait);

        public void ShowGame() =>
            TransitionTo(UIController<UIGame>.Instance);

        public void ShowGameMenu() =>
            TransitionTo(UIController<UIGameMenu>.Instance);

        private void TransitionTo(UIController controller, WaitForDone wait = null)
        {
            if (_activeController == controller || _transitioning)
            {
                if (wait != null)
                    wait.IsDone = true;
                return;
            }

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

                    if(wait != null)
                        wait.IsDone = true;
                })
                .Play();
        }

        public void AddFloatingText(string text, string className, Vector3 position, float duration = 1.0f)
        {
            UIController<UIGame>.Instance.AddFloatingText(text, className, position, duration);
        }

        private void OnGameStateChanged (object sender, GameStateChanged evt)
        {
        }

        public void OnAfterSubsystemInitialize() => ShowMainMenu();

        public void JoinLobby (string connection, bool create=false)
        {
            IEnumerator JoinLobbyCoroutine(string connection, bool create)
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading);
                yield return waitForLoading;

                yield return GameManager.Instance.LeaveLobbyAsync();

                if(create)
                    yield return GameManager.Instance.CreateLobbyAsync(connection);
                else
                    yield return GameManager.Instance.JoinLobbyAsync(connection);

                _lobbyIsland = Instantiate(_lobbyPrefab);
                GameManager.Instance.CameraOffset = new Vector3(0, 0, 0);
                GameManager.Instance.CameraZoom = 35.0f;
                GameManager.Instance.FrameCamera(_lobbyIsland.transform.position);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                UIController<UILobbyController>.Instance.IsSolo = true;
                TransitionTo(UIController<UILobbyController>.Instance);
            }

            StartCoroutine(JoinLobbyCoroutine(connection, create));
        }

        public void StartGame (GameOptions gameOptions)
        {
            IEnumerator StartGameCoroutine(GameOptions options)
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading);
                yield return waitForLoading;

                Destroy(_lobbyIsland.gameObject);
                _lobbyIsland = null;

                yield return GameManager.Instance.StartGameAsync(options);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                TransitionTo(UIController<UIGame>.Instance);
            }

            StartCoroutine(StartGameCoroutine(gameOptions));
        }

        /// <summary>
        /// Show the main menu
        /// </summary>
        public void ShowMainMenu (WaitForDone wait = null)
        {
            IEnumerator ShowMainMenuCoroutine (WaitForDone wait)
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading);
                yield return waitForLoading;

                // Leave whatever get is started
                if (GameManager.Instance.State != GameState.None)
                    yield return GameManager.Instance.LeaveLobbyAsync();

                // Make sure we have a background game
                yield return GameManager.Instance.CreateLobbyAsync("127.0.0.1:7722");
                yield return GameManager.Instance.StartGameAsync(new GameOptions { MaxIslands = 24, StartingPaths = 4, SpawnEnemies = false });

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                GameManager.Instance.CameraOffset = new Vector3(6f, 0, 0);

                ShowTitle();

                if (wait != null)
                    wait.IsDone = true;
            }

            StartCoroutine(ShowMainMenuCoroutine(wait));
        }

        public void ShowLeftPreview (ActorDefinition def) => ShowPreview(_previewLeft, def);
        public void ShowRightPreview(ActorDefinition def) => ShowPreview(_previewRight, def);

        private void ShowPreview (GameObject preview, ActorDefinition def)
        {
            if (preview.transform.childCount > 0)
                Destroy(preview.transform.GetChild(0).gameObject);

            if (null == def || def.Preview == null)
            {
                preview.transform.parent.gameObject.SetActive(false);
                return;
            }

            Instantiate(def.Preview, preview.transform).GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Preview");
            preview.transform.parent.gameObject.SetActive(true);
        }
    }
}
