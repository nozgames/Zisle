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

namespace NoZ.Zisle
{
    public enum GameState
    {
        None,
        LobbyJoining,
        LobbyLeaving,
        LobbyWait,
        GameStarting,
        GamePlaying,
        GameStopping,
    }

    public class GameManager : Singleton<GameManager>
    {
        private const float UnityServicesRetryDelay = 2.0f;
        private const int MaxPlayers = 2;

        [Header("Camera")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _cameraYaw = 45.0f;
        [SerializeField] private float _cameraPitch = 45.0f;
        [SerializeField] private float _cameraZoom = 10.0f;
        [SerializeField] private float _cameraZoomMin = 10.0f;
        [SerializeField] private float _cameraZoomMax = 40.0f;

        [Space]
        [SerializeField] private ActorDefinition[] _actorDefinitions = null;

        private GameState _state;
        private UnityTransport _transport;
        private Vector3 _cameraTarget;
        private Vector3 _cameraOffset;

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
        /// Yaw value of camera rotation
        /// </summary>
        public float CameraYaw => _cameraYaw;

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
        /// Get the current game state
        /// </summary>
        public GameState State 
        {
            get => _state;
            private set
            {
                if (_state == value)
                    return;

                var old = _state;
                _state = value;
                GameEvent.Raise(this, new GameStateChanged {  OldState = old, NewState = _state});
            }
        }

        private List<PlayerController> _players = new List<PlayerController>();

        public override void Initialize()
        {
            base.Initialize();

            foreach (var def in _actorDefinitions)
                def.RegisterNetworkId();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;

            _transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

            GameEvent<PlayerConnected>.OnRaised += OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised += OnPlayerDisconnected;
        }

        public override void Shutdown()
        {
            base.Shutdown();

            GameEvent<PlayerConnected>.OnRaised -= OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised -= OnPlayerDisconnected;
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

                State = GameState.LobbyLeaving;

                NetworkManager.Singleton.Shutdown();
                while (NetworkManager.Singleton.ShutdownInProgress)
                    yield return null;

                State = GameState.None;
            }

            if (State == GameState.None)
                return null;

            return StartCoroutine(LeaveLobby());
        }

        public Coroutine StartGameAsync (GameOptions options)
        {
            if (State != GameState.LobbyWait)
                return null;

            IEnumerator StartGame (GameOptions options)
            {
                yield return null;

                IslandManager.Instance.SpawnIslands(options);

                // Spawn all of the players
                foreach (var player in _players)
                    player.SpawnPlayer();

                State = GameState.GamePlaying;

                Debug.Log("Game Started");
            }

            State = GameState.GameStarting;

            return StartCoroutine(StartGame(options));
        }

        private IEnumerator StopGame(bool changeState=true, Action onComplete = null)
        {
            // Nothing to do if a game isnt starting or already started
            if (State != GameState.GameStarting && State != GameState.GamePlaying)
                yield break;

            // TODO: different if host            

            IslandManager.Instance.ClearIslands();

            // Return to the waiting for ready screen
            if (changeState)
                State = GameState.LobbyWait;

            onComplete?.Invoke();
        }

        /// <summary>
        /// Stop the current game and return to the lobby
        /// </summary>
        public void StopGameAsync ()
        {
            StartCoroutine(StopGame(true));
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

                NetworkManager.Singleton.StartHost();

                // Wait for the local player to connect
                while (LocalPlayer == null)
                    yield return null;

                Debug.Log("Local Player Connected");

                State = GameState.LobbyWait;

                if (wait != null)
                    wait.IsDone = true;
            }

            return StartCoroutine(CreateLobby(connection));
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

                // Wait until we see ourself join
                while (NetworkManager.Singleton.IsClient && LocalPlayer == null)
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
