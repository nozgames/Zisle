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

        [Header("Screens")]
        [SerializeField] private Transform _screens = null;
        [SerializeField] private UIClickBlocker _clickBlocker = null;
        [SerializeField] private UIConfirmationScreen _confirmationScreen = null;
        [SerializeField] private UILoadingScreen _loadingScreen = null;
        [SerializeField] private UIMultiplayerScreen _multiplayerScreen = null;
        [SerializeField] private UILobbyScreen _lobbyScreen = null;
        [SerializeField] private UITitleScreen _titleScreen = null;
        [SerializeField] private UIJoinWithCode _joinWithCodeScreen = null;
        [SerializeField] private UIJoinWithIP _joinWithIPScreen = null;
        [SerializeField] private UIDebugScreen _debugScreen = null;
        [SerializeField] private UIGame _gameScreen = null;
        [SerializeField] private UIGameMenu _gameMenuScreen = null;
        [SerializeField] private UIOptionsScreen _optionsScreen = null;

        private UIScreen _activeScreen;
        private bool _mainMenu;

        public Texture PreviewLeftTexture => _previewLeftTexture;
        public Texture PreviewRightTexture => _previewRightTexture;
        public ActorDefinition PreviewNoPlayer => _previewNoPlayer;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            InputManager.Instance.OnUIClose += () => _activeScreen.OnNavigationBack();

            InputManager.Instance.EnableMenuActions();
            InputManager.Instance.EnablePlayerActions(false);

            // Enable all screens and make them invisible
            for (int i = 0; i < _screens.childCount; i++)
            {
                var screen = _screens.GetChild(i).GetComponent<UIScreen>();
                if (null == screen)
                    continue;
                screen.GetComponent<UIDocument>().sortingOrder = i + 1;
                screen.gameObject.SetActive(true);
                screen.SetVisibleNoCallback(false);
            }

            _activeScreen = null;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }


        public void Confirm(string message, string title = null, string yes = null, string no = null, string cancel = null, Action onYes = null, Action onNo = null, Action onCancel = null)
        {
            _confirmationScreen.Message = message;
            _confirmationScreen.Yes = yes;
            _confirmationScreen.Title = title;
            _confirmationScreen.No = no;
            _confirmationScreen.OnYes = onYes;
            _confirmationScreen.OnNo = onNo;
            _confirmationScreen.OnCancel = onCancel;
            _confirmationScreen.Cancel = cancel;
            TransitionTo(_confirmationScreen);
        }

        public void ShowOptions(Action onBack = null)
        {
            var options = _optionsScreen;
            if (onBack != null)
                options.OnBack = onBack;
            TransitionTo(_optionsScreen);
        }

        public void ShowTitle () => TransitionTo(_titleScreen);
        public void ShowMultiplayer () => TransitionTo(_multiplayerScreen);
        public void ShowJoinWithCode () => TransitionTo(_joinWithCodeScreen);
        public void ShowJoinWithIP () => TransitionTo(_joinWithIPScreen);
        public void ShowGame() => TransitionTo(_gameScreen);
        public void ShowLobby() => TransitionTo(_lobbyScreen);
        public void ShowGameMenu() => TransitionTo(_gameMenuScreen);

        public void ShowLoading(WaitForDone wait = null, Action onCancel = null)
        {
            _loadingScreen.OnBack = onCancel;
            TransitionTo(_loadingScreen, wait);
        }

        private void TransitionTo(UIScreen screen, WaitForDone wait = null)
        {            
            // If the screen requires the main menu and we are not on the main menu
            // then show the main menu and then transition to the screen.
            if (screen.MainMenuOnly && !_mainMenu)
            {
                ShowMainMenu(screen);
                return;
            }

            screen.IsVisible = true;

            var blur = screen.BlurBackground;
            _postProcsUI.SetActive(blur);
            _postProcsGame.SetActive(!blur);

            if (_activeScreen == null)
            {
                _activeScreen = screen;

                screen.OnBeforeTransitionIn();
                screen.OnAfterTransitionIn();

                if (wait != null)
                    wait.IsDone = true;
                return;
            }

            _activeScreen.OnBeforeTransitionOut();
            _activeScreen.Root.pickingMode = PickingMode.Ignore;
            screen.OnBeforeTransitionIn();
            screen.Root.pickingMode = PickingMode.Ignore;
            _activeScreen.GetComponent<UIDocument>().sortingOrder = 0;
            screen.GetComponent<UIDocument>().sortingOrder = 1;

            screen.Root.style.opacity = 0;
            this.TweenGroup()
                .Element(_activeScreen.Root.style.TweenFloat("opacity", new StyleFloat(0.0f)).Duration(0.15f).EaseInOutQuadratic())
                .Element(screen.Root.style.TweenFloat("opacity", new StyleFloat(1.0f)).Duration(0.15f).EaseInOutQuadratic())
                .OnStop(() =>
                {
                    _activeScreen.OnAfterTransitionOut();
                    _activeScreen.IsVisible = false;
                    _activeScreen = screen;
                    screen.OnAfterTransitionIn();
                    screen.Root.pickingMode = PickingMode.Position;

                    if (wait != null)
                        wait.IsDone = true;
                })
                .Play();
        }

        public void AddFloatingText(string text, string className, Vector3 position, float duration = 1.0f)
        {
            _gameScreen.AddFloatingText(text, className, position, duration);
        }

        public void OnAfterSubsystemInitialize() => ShowTitle();

        /// <summary>
        /// Join or create a lobby
        /// </summary>
        public void JoinLobby (string connection, bool create=false)
        {
            IEnumerator JoinLobbyCoroutine(string connection, bool create=false)
            {
                var startTime = Time.time;

                // Leaving the main menu when we join a lobby
                _mainMenu = false;

                // Show loading screen and wait for it to be opaque
                var waitForLoading = new WaitForDone();
                ShowLoading(waitForLoading, onCancel: () => ShowMultiplayer());
                yield return waitForLoading;

                if (create)
                    yield return GameManager.Instance.CreateLobbyAsync(connection);
                else
                    yield return GameManager.Instance.JoinLobbyAsync(connection);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                // If we lost our connection or the returned to the main menu just end the coroutine
                if (!GameManager.Instance.IsInLobby || _mainMenu)
                    yield break;

                TransitionTo(_lobbyScreen);
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

                ShowGame();
            }

            AudioManager.Instance.PlayJoinGame();

            StartCoroutine(StartGameCoroutine());
        }

        private void ShowMainMenu (UIScreen nextScreen)
        {
            IEnumerator ShowMainMenuCoroutine(UIScreen nextScreen)
            {
                var startTime = Time.time;

                // Show loading screen and wait for it to be opaque
                if (!_loadingScreen.IsVisible)
                {
                    var waitForLoading = new WaitForDone();
                    ShowLoading(waitForLoading);
                    yield return waitForLoading;
                }
                else
                {
                    startTime -= MinLoadingTime;
                }

                // Leave previous lobby
                yield return GameManager.Instance.LeaveLobbyAsync();

                _mainMenu = true;

                GenerateBackground();

                GameManager.Instance.CameraOffset = new Vector3(6f, 0, 0);
                GameManager.Instance.FrameCamera(_backgroundIslands.position);

                while (Time.time - startTime < MinLoadingTime)
                    yield return null;

                TransitionTo(nextScreen);
            }

            StartCoroutine(ShowMainMenuCoroutine(nextScreen));
        }

        /// <summary>
        /// Show the main menu
        /// </summary>
        public void ShowMainMenu() => ShowMainMenu(_titleScreen);

        public void ShowLeftPreview (ActorDefinition def, bool playEffect=true) => ShowPreview(_previewLeft, def, playEffect);
        public void ShowRightPreview(ActorDefinition def, bool playEffect=true) => ShowPreview(_previewRight, def, playEffect);

        private void ShowPreview (GameObject preview, ActorDefinition def, bool playEffect)
        {
            if (preview.transform.childCount > 0)
            {
                var old = preview.transform.GetChild(0).gameObject;
                old.transform.SetParent(null);
                Destroy(old);
            }

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

        public void ToggleDebug () => _debugScreen.IsVisible = !_debugScreen.IsVisible;
    }
}

