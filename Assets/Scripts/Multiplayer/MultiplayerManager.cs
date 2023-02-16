using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace NoZ.Zisle
{
    public class MultiplayerManager : Singleton<MultiplayerManager>
    {
        [SerializeField] private float _retryDelay = 5.0f;
        [SerializeField] private int _maxPlayers = 2;

        public event Action OnConnected;
        //public event Action OnDisconnected;

        /// <summary>
        /// Join code that was used to create or join the current game
        /// </summary>
        public string JoinCode { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
        }

        private IEnumerator InitializeAsync ()
        {
            yield return InitializeUnityServices();
            Debug.Log("UnityServices initialized");

            yield return Authenticate();
            Debug.Log("Authenticated");
        }

        private IEnumerator InitializeUnityServices ()
        {
            while (true)
            {
                yield return new WaitForTask(UnityServices.InitializeAsync());
                if (UnityServices.State == ServicesInitializationState.Initialized)
                    yield break;

                Debug.Log("UnityServices failed to initialize, retrying");

                yield return new WaitForSeconds(_retryDelay);
            }
        }

        private IEnumerator Authenticate ()
        {
            while(true)
            {
                yield return new WaitForTask(AuthenticationService.Instance.SignInAnonymouslyAsync());
                if (AuthenticationService.Instance.IsSignedIn)
                    yield break;

                Debug.Log("UnityServices failed to authenticate, retrying");

                yield return new WaitForSeconds(_retryDelay);
            }
        }

        public void Host ()
        {
            StartCoroutine(CreateGameAsync());
        }

        public void HostLocal()
        {
            StartCoroutine(CreateLocalGameAsync());
        }

        public void Join(string joinCode)
        {
            StartCoroutine(JoinGameAsync(joinCode));
        }

        public void JoinLocal()
        {
            StartCoroutine(JoinLocalGameAsync());
        }

        public void LeaveGame ()
        {
            StartCoroutine(LeaveGameAsync());
        }

        private IEnumerator CreateGameAsync ()
        {
            var waitCreate = new WaitForTask<Allocation>(Relay.Instance.CreateAllocationAsync(_maxPlayers));
            yield return waitCreate;

            var allocation = waitCreate.Result;

            var waitJoinCode = new WaitForTask<string>(Relay.Instance.GetJoinCodeAsync(allocation.AllocationId));
            yield return waitJoinCode;

            var joinCode = waitJoinCode.Result;

            UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.OnServerStarted += () =>
            {
                JoinCode = joinCode;
                OnConnected?.Invoke();
            };
            NetworkManager.Singleton.StartHost();
        }

        private IEnumerator CreateLocalGameAsync ()
        {
            UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);
            NetworkManager.Singleton.OnClientConnectedCallback += (c) =>
            {
                JoinCode = "";
            };
            NetworkManager.Singleton.OnServerStarted += () =>
            {
                JoinCode = "";
            };
            NetworkManager.Singleton.StartHost();
            yield return new WaitForSeconds(1.0f);
            OnConnected?.Invoke();
        }

        private IEnumerator JoinGameAsync (string joinCode)
        {
            var waitJoin = new WaitForTask<JoinAllocation>(Relay.Instance.JoinAllocationAsync(joinCode));
            yield return waitJoin;

            var allocation = waitJoin.Result;

            UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData, 
                allocation.HostConnectionData);

            NetworkManager.Singleton.OnClientConnectedCallback += (c) =>
            {
                JoinCode = joinCode;
                OnConnected?.Invoke();
            };
            NetworkManager.Singleton.StartClient();
        }

        private IEnumerator JoinLocalGameAsync()
        {
            var transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);

            NetworkManager.Singleton.OnClientConnectedCallback += (c) => { Debug.Log("Local");  JoinCode = ""; };
            NetworkManager.Singleton.StartClient();
            yield return new WaitForSeconds(1.0f);
            OnConnected?.Invoke();
        }

        private IEnumerator LeaveGameAsync ()
        {
            NetworkManager.Singleton.Shutdown();
            JoinCode = null;
            yield return null;
        }
    }
}
