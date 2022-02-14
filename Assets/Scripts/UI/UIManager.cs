using System;
using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Tweening;
using System.Collections;
using NoZ.Animations;

namespace NoZ.Zisle.UI
{
    public class UIManager : Singleton<UIManager>
    {
#if UNITY_EDITOR
        private const float MinLoadingTime = 0.0f;
#else
        private const float MinLoadingTime = 1.0f;
#endif

        [Header("General")]
        [SerializeField] private GameObject _postProcsUI = null;
        [SerializeField] private GameObject _postProcsGame = null;

        [Header("Background")]
        [SerializeField] private Transform _background = null;
        [SerializeField] private GameOptions _backgroundOptions = null;
        [SerializeField] private GameObject _backgroundIslandPrefab = null;
        [SerializeField] private Transform _backgroundIslands = null;

        [Header("Preview")]
        [SerializeField] private RenderTexture _previewLeftTexture = null;
        [SerializeField] private RenderTexture _previewRightTexture = null;
        [SerializeField] private GameObject _previewLeft = null;
        [SerializeField] private GameObject _previewRight = null;
        [SerializeField] private ActorDefinition _previewNoPlayer = null;
        [SerializeField] private GameObject _previewChangeEffect = null;

        private VisualElement _root;
        private ScreenElement _activeController;
        private bool _transitioning;

        public Texture PreviewLeftTexture => _previewLeftTexture;
        public Texture PreviewRightTexture => _previewRightTexture;
        public ActorDefinition PreviewNoPlayer => _previewNoPlayer;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement;
            //doc.enabled = true;

            // Initialzie and bind all UIControllers
            _root.Query<ScreenElement>().ForEach(c => c.Initialize());

            ScreenElement<OptionsController>.Bind(_root).Hide();
            ScreenElement<UIKeyboardControls>.Bind(_root).Hide();
            ScreenElement<UIGamepadControls>.Bind(_root).Hide();
            ScreenElement<ConfirmPopupController>.Bind(_root).Hide();
            ScreenElement<CooperativeController>.Bind(_root).Hide();
            ScreenElement<CooperativeJoinController>.Bind(_root).Hide();
            ScreenElement<UILoadingController>.Bind(_root).Hide();
            ScreenElement<UI.UIGame>.Bind(_root).Hide();
            ScreenElement<UIGameMenu>.Bind(_root).Hide();
            ScreenElement<TitleScreen>.Bind(_root).Hide();
            ScreenElement<LobbyScreen>.Bind(_root).Hide();
            ScreenElement<UIDebugController>.Bind(_root).Hide();

            InputManager.Instance.OnUIClose += () => _activeController.OnNavigationBack();

            InputManager.Instance.EnableMenuActions();

            var loading = ScreenElement<UILoadingController>.Bind(_root);
            loading.Show();
            loading.OnBeforeTransitionIn();
            loading.OnAfterTransitionIn();
            _activeController = loading;

            var blur = loading.BlurBackground;
            _postProcsUI.SetActive(blur);
            _postProcsGame.SetActive(!blur);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }


        public void ShowConfirmationPopup (string message, string yes=null, string no=null, string cancel=null, Action onYes=null, Action onNo=null, Action onCancel=null)
        {
            var popup = ScreenElement<ConfirmPopupController>.Instance;
            popup.Message = message;
            popup.Yes = yes;
            popup.No = no;
            popup.OnYes = onYes;
            popup.OnNo = onNo;
            popup.OnCancel = onCancel;
            popup.Cancel = cancel;
            TransitionTo(ScreenElement<ConfirmPopupController>.Instance);
        }

        public void ShowOptions (Action onBack=null)
        {
            var options = ScreenElement<OptionsController>.Instance;
            if(onBack != null)
                options.OnBack = onBack;
            TransitionTo(options);
        }

        public void ShowKeyboardControls() =>
            TransitionTo(ScreenElement<UIKeyboardControls>.Instance);

        public void ShowGamepadControls() =>
            TransitionTo(ScreenElement<UIGamepadControls>.Instance);

        public void ShowTitle () =>
            TransitionTo(ScreenElement<TitleScreen>.Instance);

        public void ShowCooperative () =>
            TransitionTo(ScreenElement<CooperativeController>.Instance);

        public void ShowCooperativeJoin () =>
            TransitionTo(ScreenElement<CooperativeJoinController>.Instance);

        public void ShowLoading (WaitForDone wait = null) =>
            TransitionTo(ScreenElement<UILoadingController>.Instance, wait);

        public void ShowGame() =>
            TransitionTo(ScreenElement<UI.UIGame>.Instance);

        public void ShowGameMenu() =>
            TransitionTo(ScreenElement<UIGameMenu>.Instance);

        private void TransitionTo(ScreenElement controller, WaitForDone wait = null)
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
            ScreenElement<UI.UIGame>.Instance.AddFloatingText(text, className, position, duration);
        }

        public void OnAfterSubsystemInitialize() => ShowMainMenu();

        /// <summary>
        /// Join or create a lobby
        /// </summary>
        public void JoinLobby (string connection, bool create=false)
        {
            IEnumerator JoinLobbyCoroutine(string connection, bool create=false)
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading);
                yield return waitForLoading;

                if (create)
                    yield return GameManager.Instance.CreateLobbyAsync(connection);
                else
                    yield return GameManager.Instance.JoinLobbyAsync(connection);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                TransitionTo(ScreenElement<LobbyScreen>.Instance);
            }

            StartCoroutine(JoinLobbyCoroutine(connection,create));
        }

        public void StartGame ()
        {
            IEnumerator StartGameCoroutine()
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading);
                yield return waitForLoading;

                ClearBackground();

                GameManager.Instance.CameraOffset = Vector3.zero;

                yield return GameManager.Instance.StartGameAsync();

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                TransitionTo(ScreenElement<UI.UIGame>.Instance);
            }

            StartCoroutine(StartGameCoroutine());
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

                // Leave previous lobby
                yield return GameManager.Instance.LeaveLobbyAsync();

                GenerateBackground();

                GameManager.Instance.CameraOffset = new Vector3(6f, 0, 0);
                GameManager.Instance.FrameCamera(_backgroundIslands.position);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                ShowTitle();

                if (wait != null)
                    wait.IsDone = true;
            }

            StartCoroutine(ShowMainMenuCoroutine(wait));
        }

        public void ShowLeftPreview (ActorDefinition def, bool playEffect=true) => ShowPreview(_previewLeft, def, playEffect);
        public void ShowRightPreview(ActorDefinition def, bool playEffect = true) => ShowPreview(_previewRight, def, playEffect);

        private void ShowPreview (GameObject preview, ActorDefinition def, bool playEffect)
        {
            if (preview.transform.childCount > 0)
                Destroy(preview.transform.GetChild(0).gameObject);

            if (null == def || def.Preview == null)
            {
                preview.transform.parent.gameObject.SetActive(false);
                return;
            }

            var go = Instantiate(def.Preview, preview.transform);
            var renderer = go.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.gameObject.layer = LayerMask.NameToLayer("Preview");

            var animator = renderer.transform.parent.gameObject.GetOrAddComponent<Animator>();
            var blend = animator.gameObject.AddComponent<BlendedAnimationController>();

            IEnumerator Test()
            {
                yield return new WaitForEndOfFrame();
                blend.Play(def.Prefab.GetComponent<Actor>().IdleAnimation, blendIn: false);
            }

            StartCoroutine(Test());

            if (playEffect && null != _previewChangeEffect)
                Instantiate(_previewChangeEffect, preview.transform.parent).gameObject.layer = LayerMask.NameToLayer("Preview");


            preview.transform.parent.gameObject.SetActive(true);
        }

        public void GenerateBackground (int startingPaths = 4)
        {
            ClearBackground();

            var options = _backgroundOptions.ToGeneratorOptions();
            options.StartingLanes = startingPaths;
            var cells = (new WorldGenerator()).Generate(options);

            // Spawn the islands in simplified form
            // Spawn the islands on the host will all prefabs
            foreach (var cell in cells)
            {
                var biome = NetworkScriptableObject<Biome>.Get(cell.BiomeId);
                if (biome == null)
                    throw new InvalidProgramException($"Biome id {cell.BiomeId} not found");

                var islandPrefab = biome.Islands[cell.IslandIndex];

                // Instatiate the island itself
                var island = Instantiate(
                    _backgroundIslandPrefab,
                    IslandGrid.CellToWorld(cell.Position),
                    Quaternion.Euler(0, 90 * (int)cell.Rotation, 0), 
                    _backgroundIslands);
                island.GetComponent<MeshFilter>().sharedMesh = islandPrefab.GetComponent<MeshFilter>().sharedMesh;

                var meshRenderer = island.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = new Material[] { biome.Material, meshRenderer.sharedMaterials[1] };

                // Spawn bridges
                // TODO: eventaully need to spawn just the mesh if bridge is networked
                if (cell.Position != Vector2Int.zero && biome.Bridge != null)
                {
                    var from = IslandGrid.CellToWorld(cell.Position);
                    var to = IslandGrid.CellToWorld(cell.To);

                    var bridge = new GameObject();
                    Instantiate(biome.Bridge.GetComponentInChildren<MeshFilter>().gameObject, bridge.transform);
                    bridge.transform.position = (from + to) * 0.5f;
                    bridge.transform.rotation = Quaternion.LookRotation((to - from).normalized, Vector3.up);
                    bridge.transform.SetParent(_backgroundIslands);
                }
            }

            _background.gameObject.SetActive(true);
        }

        private void ClearBackground()
        {
            for (int i = _backgroundIslands.childCount - 1; i >= 0; i--)
                Destroy(_backgroundIslands.GetChild(i).gameObject);

            _background.gameObject.SetActive(false);
        }

        public void ToggleDebug ()
        {
            ScreenElement<UIDebugController>.Instance.Toggle();
        }
    }
}

