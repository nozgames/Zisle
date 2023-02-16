using System.Linq;
using UnityEngine;
using NoZ.Events;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using NoZ.Zisle.UI;
using Unity.Netcode.Transports.UTP;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
        private const float UnityServicesRetryDelay = 2.0f;

        [Header("General")]
        [SerializeField] private Game _gamePrefab = null;
        [SerializeField] private GameOptions _optionsPrefab = null;
        [SerializeField] private LayerMask _groundLayer = 0;
        [SerializeField] private GlobalShaderProperties _globalShaderProperties = null;
        [SerializeField] private WaterMesh _waterMesh;
        [SerializeField] private AudioListener _audioListener = null;
        [SerializeField] private NoZ.Animations.AnimationEvent _abilityBeginEvent = null;
        [SerializeField] private NoZ.Animations.AnimationEvent _abilityEndEvent = null;
        [SerializeField] private Effect _defaultActorEffect = null;

        [Space]
        [SerializeField] private Biome[] _biomes = null;

        [Space]
        [SerializeField] private ActorDefinition[] _actorDefinitions = null;

        private UnityTransport _transport;
        private List<PlayerController> _players = new List<PlayerController>();
        private GameOptions _options = null;
        private string _connection;

        public NoZ.Animations.AnimationEvent AbilityBeginEvent => _abilityBeginEvent;
        public NoZ.Animations.AnimationEvent AbilityEndEvent => _abilityEndEvent;
        public Effect DefaultActorEffect => _defaultActorEffect;

        /// <summary>
        /// Optional join code if the connection was to a relay server
        /// </summary>
        public string JoinCode { get; private set; }

        /// <summary>
        /// True if there is a join code available
        /// </summary>
        public bool HasJoinCode => !string.IsNullOrEmpty(JoinCode);

        /// <summary>
        /// Controller for local player connected to the lobby
        /// </summary>
        public PlayerController LocalPlayerController { get; private set; }

        /// <summary>
        /// Connection string used to join the lobby
        /// </summary>
        public string Connection => _connection ?? "";

        /// <summary>
        /// Local player in the game
        /// </summary>
        public Player LocalPlayer => LocalPlayerController == null ? null : LocalPlayerController.Player;

        public GlobalShaderProperties GlobalShaderProperties => _globalShaderProperties;

        /// <summary>
        /// Maximum number of players allowed on the server
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// True if max playes is set to 1
        /// </summary>
        public bool IsSolo => MaxPlayers == 1;

        /// <summary>
        /// Get the current options object.  Will be null until joining a lobby
        /// </summary>
        public GameOptions Options => _options;

        /// <summary>
        /// Get / Set the current game
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Layer used to determine what is the ground
        /// </summary>
        public LayerMask GroundLayer => _groundLayer;

        /// <summary>
        /// Return the available biomes
        /// </summary>
        public IEnumerable<Biome> Biomes => _biomes;

        /// <summary>
        /// Return the actor definitions
        /// </summary>
        public IEnumerable<ActorDefinition> ActorDefinitions => _actorDefinitions;

        /// <summary>
        /// Return the current list of players
        /// </summary>
        public IEnumerable<PlayerController> Players => _players;

        /// <summary>
        /// Get the number of connected players
        /// </summary>
        public int PlayerCount => _players.Count;

        public override void Initialize()
        {
            base.Initialize();

            _actorDefinitions.RegisterNetworkIds();
            _biomes.RegisterNetworkIds();
            _defaultActorEffect.RegisterNetworkId();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;

            _transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            GameEvent<PlayerConnected>.OnRaised += OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised += OnPlayerDisconnected;
            GameEvent<GameOptionsSpawned>.OnRaised += OnGameOptionsSpawned;
        }

        public override void Shutdown()
        {
            base.Shutdown();

            GameEvent<PlayerConnected>.OnRaised -= OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised -= OnPlayerDisconnected;
            GameEvent<GameOptionsSpawned>.OnRaised -= OnGameOptionsSpawned;
        }

        private void OnGameOptionsSpawned(object sender, GameOptionsSpawned evt)
        {
            _options = evt.Options;
        }

        private void OnPlayerConnected(object sender, PlayerConnected evt)
        {
            if (evt.PlayerController.IsLocalPlayer)
                LocalPlayerController = evt.PlayerController;

            _players.Add(evt.PlayerController);
        }

        private void OnPlayerDisconnected(object sender, PlayerDisconnected evt)
        {
            if (evt.PlayerController.IsLocalPlayer)
                LocalPlayerController = null;

            _players.Remove(evt.PlayerController);
        }

        private void OnPlayerMenu()
        {
            UIManager.Instance.ShowGameMenu();
        }

        /// <summary>
        /// Leave the current lobby and stop the game if the player is the host
        /// </summary>
        public Coroutine LeaveLobbyAsync ()
        {
            IEnumerator LeaveLobby ()
            {
                // Stop the game first
                yield return StopGame();

                ListenAt(transform);

                if (IsInLobby)
                    NetworkManager.Singleton.Shutdown();

                while (NetworkManager.Singleton.ShutdownInProgress)
                    yield return null;
            }

            return StartCoroutine(LeaveLobby());
        }

        public Coroutine StartGameAsync ()
        {
            IEnumerator StartGame ()
            {
                // Host will spawn the game object
                if(NetworkManager.Singleton.IsHost)
                    Instantiate(_gamePrefab).NetworkObject.Spawn(); ;

                // Wait for the game to spawn
                while (Game == null && !Game.HasIslands)
                    yield return null;

                Game.Play();

                Debug.Log("Game Started");
            }

            return StartCoroutine(StartGame());
        }

        public bool IsInLobby => !NetworkManager.Singleton.ShutdownInProgress && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient);


        private IEnumerator StopGame()
        {
            if (!IsInLobby)
                yield break;

            if (Game != null)
            {
                if(NetworkManager.Singleton.IsHost)
                    Game.NetworkObject.Despawn();
                Game = null;
            }

            yield break;
        }

        /// <summary>
        /// Stop the current game and return to the lobby
        /// </summary>
        public void StopGameAsync ()
        {
            StartCoroutine(StopGame());
        }

        private void SetInitialPlayerClass()
        {
            // Set initial player class
            var def = Instance.ActorDefinitions.Where(d => d.name == Zisle.Options.PlayerClass && d.ActorType == ActorType.Player).FirstOrDefault();
            if (def == null)
                def = Instance.ActorDefinitions.Where(d => d.name == "RandomPlayer" && d.ActorType == ActorType.Player).FirstOrDefault();
            if (def == null)
                def = Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).FirstOrDefault();

            Instance.LocalPlayerController.PlayerClassId = def.NetworkId;
        }

        /// <summary>
        /// Create a new lobby with the given connection string, null to use relay
        /// </summary>
        public Coroutine CreateLobbyAsync (string connection, WaitForDone wait = null) 
        {
            IEnumerator CreateLobby (string connection, WaitForDone wait = null)
            {
                yield return ConfigureTransportAsync(connection);
                if (_connection == null)
                {
                    UIManager.Instance.Confirm(message: "failed-to-create-lobby", title: "error", yes: "ok", onYes: () =>
                    {
                        UIManager.Instance.ShowMultiplayer();
                    });
                    yield break;
                }

                Debug.Log($"Creating Lobby: {connection}");

                NetworkManager.Singleton.StartHost();

                // Wait for the local player to connect
                while (LocalPlayerController == null)
                    yield return null;

                SetInitialPlayerClass();

                // Spawn the game options and wait for it 
                Instantiate(_optionsPrefab).GetComponent<NetworkObject>().Spawn();
                while (_options == null)
                    yield return null;

                Debug.Log("Local Player Connected");

                if (wait != null)
                    wait.IsDone = true;
            }

            return StartCoroutine(CreateLobby(connection));
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = _players.Count < MaxPlayers;
        }

        /// <summary>
        /// Joint a lobby with the given connection string
        /// </summary>
        public Coroutine JoinLobbyAsync (string connection)
        {
            IEnumerator JoinLobby (string connection)
            {
                // Configure the transport using the connection string and a temporary game options instance
                yield return ConfigureTransportAsync(connection);
                if(_connection == null)
                {
                    UIManager.Instance.Confirm(message: "failed-to-join-lobby", title: "error", yes: "ok", onYes: () =>
                      {
                          UIManager.Instance.ShowMultiplayer();
                      });
                    yield break;
                }

                Debug.Log($"Connecting to Lobby: {connection}");

                // Connect                
                NetworkManager.Singleton.StartClient();

                // Wait until we see ourself join and the options spawns
                while (NetworkManager.Singleton.IsClient && (LocalPlayerController == null || _options == null) && !NetworkManager.Singleton.ShutdownInProgress)
                    yield return null;

                if (!IsInLobby)
                    yield break;

                SetInitialPlayerClass();

                while (Instance.LocalPlayerController.PlayerClass == null)
                    yield return null;

                Debug.Log("Local Player Connected");
            }

            return StartCoroutine(JoinLobby(connection));
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.ShutdownInProgress)
                return;

            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                UIManager.Instance.Confirm(message:"Connection to host has been lost", title:"error", yes: "ok".Localized(), onYes: () => UIManager.Instance.ShowTitle());
        }

        /// <summary>
        /// Configure the transport to either an IP based connection or to a relay server
        /// </summary>
        private IEnumerator ConfigureTransportAsync (string connection)
        {
            // If connection is null then it means we are connecting to a relay server as the host
            if (connection == null)
            {
                yield return InitializeUnityServices();

                _connection = null;

                // Create an allocation
                var waitCreate = new WaitForTask<Allocation>(Relay.Instance.CreateAllocationAsync(MaxPlayers));
                yield return waitCreate;
                if(!waitCreate.IsSuccessful)
                    yield break;

                /// Get the join code
                var allocation = waitCreate.Result;
                var waitJoinCode = new WaitForTask<string>(Relay.Instance.GetJoinCodeAsync(allocation.AllocationId));
                yield return waitJoinCode;

                // Configure the transport
                JoinCode = waitJoinCode.Result;
                _transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);

                _connection = JoinCode;

                yield break;
            }

            // IP address and optional port
            if (connection != null && connection.Contains("."))
            {
                var colon = connection.IndexOf(":");
                var ip = colon == -1 ? connection : connection.Substring(0, colon);
                var port = colon == -1 ? 7777 : (int.TryParse(connection.Substring(colon + 1), out var parsed) ? parsed : 7777);
                _transport.SetConnectionData(ip, (ushort)port);

                _connection = connection;
            }
            // Join code
            else
            {
                yield return InitializeUnityServices();

                JoinCode = connection;

                // Join the allocation
                var waitJoin = new WaitForTask<JoinAllocation>(Relay.Instance.JoinAllocationAsync(connection));
                yield return waitJoin;
                if (!waitJoin.IsSuccessful)
                {
                    JoinCode = null;
                    _connection = null;
                    yield break;
                }

                // Configure the transport
                var allocation = waitJoin.Result;
                _transport.SetClientRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.HostConnectionData);

                _connection = JoinCode;
            }
        }

        private IEnumerator InitializeUnityServices ()
        {
            // Connect to UnityServices first
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                while (true)
                {
                    yield return new WaitForTask(UnityServices.InitializeAsync());
                    if (UnityServices.State == ServicesInitializationState.Initialized)
                        break;

                    Debug.Log("UnityServices failed to initialize, retrying");

                    yield return new WaitForSeconds(UnityServicesRetryDelay);
                }

                Debug.Log("UnityServices Initialized");
            }

            // Authenticate if needed
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                while (true)
                {
                    yield return new WaitForTask(AuthenticationService.Instance.SignInAnonymouslyAsync());
                    if (AuthenticationService.Instance.IsSignedIn)
                        break;

                    Debug.Log("UnityServices failed to authenticate, retrying");

                    yield return new WaitForSeconds(UnityServicesRetryDelay);
                }

                Debug.Log("UnityServices Authenticated");
            }
        }

        public void Resume ()
        {
            InputManager.Instance.EnableMenuActions(false);
            InputManager.Instance.EnablePlayerActions(true);
        }

        public void Pause ()
        {
            InputManager.Instance.EnableMenuActions(true);
            InputManager.Instance.EnablePlayerActions(false);
        }

        public void GenerateWater (Transform islandTransform)
        {
            _waterMesh.Generate(islandTransform);
        }

        public void ShakeCamera(float intensity, float duration) { }// => _cameraShake.Shake(intensity, duration);

        public void ListenAt (Transform transform)
        {
            _audioListener.transform.position = transform.position;
            _audioListener.transform.rotation = transform.rotation;
        }
    }
}

