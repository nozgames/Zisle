using UnityEngine;
using NoZ.Events;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using static Unity.Netcode.NetworkManager;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
        private const float UnityServicesRetryDelay = 2.0f;

        [Header("General")]
        [SerializeField] private GameOptions _optionsPrefab = null;

        [Header("Camera")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _cameraYaw = 45.0f;
        [SerializeField] private float _cameraPitch = 45.0f;
        [SerializeField] private float _cameraZoom = 10.0f;
        [SerializeField] private float _cameraZoomMin = 10.0f;
        [SerializeField] private float _cameraZoomMax = 40.0f;

        [Space]
        [SerializeField] private ActorDefinition[] _actorDefinitions = null;

        private UnityTransport _transport;
        private Vector3 _cameraTarget;
        private Vector3 _cameraOffset;
        private List<PlayerController> _players = new List<PlayerController>();
        private GameOptions _options = null;

        /// <summary>
        /// Optional join code if the connection was to a relay server
        /// </summary>
        public string JoinCode { get; private set; }

        /// <summary>
        /// True if there is a join code available
        /// </summary>
        public bool HasJoinCode => !string.IsNullOrEmpty(JoinCode);

        /// <summary>
        /// Local player connected to the lobby
        /// </summary>
        public PlayerController LocalPlayer { get; private set; }

        /// <summary>
        /// Get the game camera
        /// </summary>
        public Camera Camera => _camera;

        /// <summary>
        /// Maximum number of players allowed on the server
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// True if max playes is set to 1
        /// </summary>
        public bool IsSolo => MaxPlayers == 1;

        /// <summary>
        /// Yaw value of camera rotation
        /// </summary>
        public float CameraYaw => _cameraYaw;

        /// <summary>
        /// Get the current options object.  Will be null until joining a lobby
        /// </summary>
        public GameOptions Options => _options;

        public Vector2 CameraOffset
        {
            get => _cameraOffset;
            set
            {
                _cameraOffset = value;
                FrameCamera(_cameraTarget);
            }
        }

        /// <summary>
        /// Current camera zoom
        /// </summary>
        public float CameraZoom
        {
            get => _cameraZoom;
            set
            {
                _cameraZoom = Mathf.Clamp(value, _cameraZoomMin, _cameraZoomMax);
                FrameCamera(_cameraTarget);
            }
        }

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

            foreach (var def in _actorDefinitions)
                def.RegisterNetworkId();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;

            _transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

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
                LocalPlayer = evt.PlayerController;

            _players.Add(evt.PlayerController);
        }

        private void OnPlayerDisconnected(object sender, PlayerDisconnected evt)
        {
            if (evt.PlayerController.IsLocalPlayer)
                LocalPlayer = null;

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

                if(IsInLobby)
                {
                    NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                    NetworkManager.Singleton.Shutdown();
                }

                while (NetworkManager.Singleton.ShutdownInProgress)
                    yield return null;
            }

            return StartCoroutine(LeaveLobby());
        }

        public Coroutine StartGameAsync ()
        {
            IEnumerator StartGame ()
            {
                yield return null;

                //IslandManager.Instance.SpawnIslands();

                // Spawn all of the players
                if(NetworkManager.Singleton.IsHost)
                {
                    foreach (var player in _players)
                        player.SpawnPlayer();
                }

                Debug.Log("Game Started");
            }

            return StartCoroutine(StartGame());
        }

        public bool IsInLobby => !NetworkManager.Singleton.ShutdownInProgress && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient);


        private IEnumerator StopGame()
        {
            if (!IsInLobby)
                yield break;

            // TODO: different if host            

            IslandManager.Instance.ClearIslands();

            yield break;
        }

        /// <summary>
        /// Stop the current game and return to the lobby
        /// </summary>
        public void StopGameAsync ()
        {
            StartCoroutine(StopGame());
        }

        /// <summary>
        /// Create a new lobby with the given connection string, null to use relay
        /// </summary>
        public Coroutine CreateLobbyAsync (string connection, WaitForDone wait = null) 
        {
            IEnumerator CreateLobby (string connection, WaitForDone wait = null)
            {
                yield return ConfigureTransportAsync(connection);

                Debug.Log($"Creating Lobby: {connection}");

                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.StartHost();

                // Wait for the local player to connect
                while (LocalPlayer == null)
                    yield return null;

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

        private void ApprovalCheck(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
        {
            if (_players.Count >= MaxPlayers)
                callback(false, null, false, null, null);
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

                Debug.Log($"Connecting to Lobby: {connection}");

                // Connect
                NetworkManager.Singleton.StartClient();

                // Wait until we see ourself join and the options spawns
                while (NetworkManager.Singleton.IsClient && (LocalPlayer == null || _options == null))
                    yield return null;

                Debug.Log("Local Player Connected");
            }

            return StartCoroutine(JoinLobby(connection));
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

                // Create an allocation
                var waitCreate = new WaitForTask<Allocation>(Relay.Instance.CreateAllocationAsync(MaxPlayers));
                yield return waitCreate;

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

                yield break;
            }

            // IP address and optional port
            if (connection != null && connection.Contains("."))
            {
                var colon = connection.IndexOf(":");
                var ip = colon == -1 ? connection : connection.Substring(0, colon);
                var port = colon == -1 ? 7777 : (int.TryParse(connection.Substring(colon + 1), out var parsed) ? parsed : 7777);
                _transport.SetConnectionData(ip, (ushort)port);
            }
            // Join code
            else
            {
                yield return InitializeUnityServices();

                JoinCode = connection;

                // Join the allocation
                var waitJoin = new WaitForTask<JoinAllocation>(Relay.Instance.JoinAllocationAsync(connection));
                yield return waitJoin;

                // Configure the transport
                var allocation = waitJoin.Result;
                _transport.SetClientRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.HostConnectionData);
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

        public void FrameCamera (Vector3 target)
        {
            _cameraTarget = target;
            _cameraTarget += Quaternion.Euler(0, _cameraYaw, 0) * _cameraOffset;
            _camera.transform.position = _cameraTarget + Quaternion.Euler(_cameraPitch, _cameraYaw, 0) * new Vector3(0, 0, 1) * _cameraZoom;
            _camera.transform.LookAt(_cameraTarget, Vector3.up);
        }
    }
}

