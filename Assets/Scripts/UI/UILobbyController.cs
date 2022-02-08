using NoZ.Events;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UILobbyController : UIController
    {
        public new class UxmlFactory : UxmlFactory<UILobbyController, UxmlTraits> { }

        public override bool BlurBackground => true;

        private List<ActorDefinition> _playerClasses;
        private ActorDefinition _playerClassLeft;

        public override void Initialize()
        {
            base.Initialize();

            _playerClasses = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).ToList();

            this.Q<Image>("player-left-preview").image = UIManager.Instance.PreviewLeftTexture;

            BindClick("quit", () =>
            {
                if (GameManager.Instance.MaxPlayers == 1)
                    UIManager.Instance.ShowMainMenu();
                else
                {
                    UIManager.Instance.ShowConfirmationPopup(
                        message: "Are you sure you want to leave the lobby?",
                        onYes: () => UIManager.Instance.ShowMainMenu());
                }
            });

            BindClick("player-class-prev", () =>
            {
                var index = _playerClasses.IndexOf(_playerClassLeft);
                if (index == -1)
                    index = 0;
                else
                    index = (index + _playerClasses.Count - 1) % _playerClasses.Count;

                SetPlayerClassLeft(_playerClasses[index]);

                GameManager.Instance.LocalPlayer.PlayerClassId = _playerClasses[index].NetworkId;
            });

            BindClick("player-class-next", () =>
            {
                var index = _playerClasses.IndexOf(_playerClassLeft);
                if (index == -1)
                    index = 0;
                else
                    index = (index + 1) % _playerClasses.Count;

                SetPlayerClassLeft(_playerClasses[index]);

                GameManager.Instance.LocalPlayer.PlayerClassId = _playerClasses[index].NetworkId;
            });

            BindClick("ready", () =>
            {
                if (GameManager.Instance.IsSolo)
                    UIManager.Instance.StartGame();
                else
                {
                    GameManager.Instance.LocalPlayer.IsReady = !GameManager.Instance.LocalPlayer.IsReady;
                    this.Q<Button>("ready").text = GameManager.Instance.LocalPlayer.IsReady ? "Cancel" : "Ready";
                }
            });
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            SetPlayerClassLeft(GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).FirstOrDefault());

            GameEvent<PlayerClassChanged>.OnRaised += OnPlayerClassChanged;
            GameEvent<PlayerReadyChanged>.OnRaised += OnPlayerReadyChanged;
            GameEvent<GameOptionStartingLanesChanged>.OnRaised += OnStartingLanesChanged;
            GameEvent<PlayerConnected>.OnRaised += OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised += OnPlayerDisconnected;
            GameEvent<PlayerSpawned>.OnRaised += OnPlayerSpawned;

            // Show "Play" for ready button if solo
            this.Q<Button>("ready").text = GameManager.Instance.IsSolo ? "Play" : "Ready";

            this.Q("join-code-container").EnableInClassList("hidden", GameManager.Instance.JoinCode == null);
            this.Q<Label>("join-code").text = GameManager.Instance.JoinCode ?? "";

            UIManager.Instance.GenerateBackground(GameManager.Instance.Options.StartingLanes);

            this.Q<RadioButton>($"lane{GameManager.Instance.Options.StartingLanes}").value = true;
            this.Q<RadioButton>($"lane1").RegisterValueChangedCallback((evt) => { if (evt.newValue) GameManager.Instance.Options.StartingLanes = 1; });
            this.Q<RadioButton>($"lane2").RegisterValueChangedCallback((evt) => { if (evt.newValue) GameManager.Instance.Options.StartingLanes = 2; });
            this.Q<RadioButton>($"lane3").RegisterValueChangedCallback((evt) => { if (evt.newValue) GameManager.Instance.Options.StartingLanes = 3; });
            this.Q<RadioButton>($"lane4").RegisterValueChangedCallback((evt) => { if (evt.newValue) GameManager.Instance.Options.StartingLanes = 4; });

            this.Q("player-left-ready").EnableInClassList("hidden", GameManager.Instance.IsSolo);

            UpdateReadyButton();
            UpdateRemotePlayer();
        }

        private void OnPlayerSpawned(object sender, PlayerSpawned evt)
        {
            if (evt.Player.IsOwner && !NetworkManager.Singleton.IsHost)
                UIManager.Instance.StartGame();
        }

        public override void OnBeforeTransitionOut()
        {
            base.OnBeforeTransitionOut();

            UIManager.Instance.ShowLeftPreview(null);
            UIManager.Instance.ShowRightPreview(null);

            GameEvent<PlayerClassChanged>.OnRaised -= OnPlayerClassChanged;
            GameEvent<PlayerReadyChanged>.OnRaised -= OnPlayerReadyChanged;
            GameEvent<GameOptionStartingLanesChanged>.OnRaised -= OnStartingLanesChanged;
            GameEvent<PlayerConnected>.OnRaised -= OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised -= OnPlayerDisconnected;
            GameEvent<PlayerSpawned>.OnRaised -= OnPlayerSpawned;
        }

        private void OnPlayerConnected(object sender, PlayerConnected evt)
        {
            if (!evt.PlayerController.IsLocalPlayer)
                UpdateRemotePlayer();

            UpdateReadyButton();
        }

        private void OnPlayerDisconnected(object sender, PlayerDisconnected evt)
        {
            if (!evt.PlayerController.IsLocalPlayer)
            {
                UpdateRemotePlayer();

                if(GameManager.Instance.LocalPlayer != null)
                    GameManager.Instance.LocalPlayer.IsReady = false;
            }

            UpdateReadyButton();
        }

        private void UpdateReadyButton ()
        {
            this.Q("ready").SetEnabled(GameManager.Instance.PlayerCount == GameManager.Instance.MaxPlayers);
        }

        private void UpdateRemotePlayer()
        {
            if (GameManager.Instance.IsSolo)
            {
                this.Q("ghost").AddToClassList("hidden");
                this.Q("not").RemoveFromClassList("hidden");
                this.Q("player-right-ready").AddToClassList("hidden");
                this.Q("player-right-name").AddToClassList("hidden");
                this.Q<Image>("player-right-preview").image = null;
                UIManager.Instance.ShowRightPreview(null);
                return;
            }

            this.Q("not").AddToClassList("hidden");
            this.Q("player-right-name").RemoveFromClassList("hidden");

            var remotePlayer = GameManager.Instance.Players.Where(p => !p.IsLocalPlayer && !p.IsDisconnecting).FirstOrDefault();
            if (remotePlayer == null || remotePlayer.PlayerClass == null)
            {
                this.Q("ghost").RemoveFromClassList("hidden");
                this.Q("player-right-ready").AddToClassList("hidden");
                this.Q<Label>("player-right-name").text = remotePlayer == null ? "Waiting for Player" : "Connecting";

                this.Q<Image>("player-right-preview").image = null;
                UIManager.Instance.ShowRightPreview(null);
            }
            else
            {
                var playerClass = remotePlayer.PlayerClass;

                this.Q("ghost").AddToClassList("hidden");
                this.Q("player-right-ready").RemoveFromClassList("hidden");
                this.Q<Label>("player-right-name").text = playerClass.DisplayName;

                this.Q<Image>("player-right-preview").image = UIManager.Instance.PreviewRightTexture;
                UIManager.Instance.ShowRightPreview(playerClass);
            }
        }

        private void OnStartingLanesChanged(object sender, GameOptionStartingLanesChanged evt)
        {
            this.Q<RadioButton>($"lane{GameManager.Instance.Options.StartingLanes}").value = true;

            UIManager.Instance.GenerateBackground(evt.NewValue);
        }

        private void OnPlayerReadyChanged(object sender, PlayerReadyChanged evt)
        {
            if (evt.PlayerController.IsLocalPlayer)
                this.Q("player-left-ready").EnableInClassList("ready", evt.PlayerController.IsReady);
            else
                this.Q("player-right-ready").EnableInClassList("ready", evt.PlayerController.IsReady);

            if(NetworkManager.Singleton.IsHost && GameManager.Instance.Players.Where(p => p.IsReady).Count() == GameManager.Instance.MaxPlayers)
                UIManager.Instance.StartGame();
        }

        private void OnPlayerClassChanged(object sender, PlayerClassChanged evt)
        {
            if (!evt.PlayerController.IsLocalPlayer)
                UpdateRemotePlayer();
        }

        private void SetPlayerClassLeft(ActorDefinition def)
        {
            this.Q<Label>("player-left-name").text = def.DisplayName;
            UIManager.Instance.ShowLeftPreview(def);
            _playerClassLeft = def;
        }
    }
}
