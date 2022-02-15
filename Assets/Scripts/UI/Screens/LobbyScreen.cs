using NoZ.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using NoZ.Zisle.UI;

namespace NoZ.Zisle.UI
{
    public class LobbyScreen : ScreenElement
    {
        public const int CountdownSeconds = 5;

        public new class UxmlFactory : UxmlFactory<LobbyScreen, UxmlTraits> { }

        public override bool BlurBackground => true;

        private List<ActorDefinition> _playerClasses;
        private ActorDefinition _localPlayerClass;
        private Coroutine _startGameCountdown;
        private VisualElement _nextClassButton;
        private VisualElement _prevClassButton;
        private Image _localPlayerPreview;
        private Image _remotePlayerPreview;
        private Label _localPlayerName;
        private Label _remotePlayerName;
        private Label _localPlayerHeader;
        private RaisedButton _readyButton;
        private RadioButton[] _lanes;
        private bool _playLaneClick = true;
        private VisualElement _remotePlayer;
        private VisualElement _localPlayerReady;
        private VisualElement _remotePlayerReady;
        private VisualElement _joinCodeContainer;
        private Label _joinCode;
        private Panel _panel;

        public LobbyScreen()
        {
            _panel = this.Add<Panel>().SetTitle("lobby".Localized().ToUpper()).OnClose(OnQuit);

            var players = _panel.AddItem<VisualElement>().AddClass("players");

            // Local player
            var localPlayer = players.Add<VisualElement>(name: "player-left").AddClass("player");
            _localPlayerHeader = localPlayer.Add<Label>().LocalizedText("local-player").AddClass("header");
            var localPlayerPreview = localPlayer.Add<VisualElement>().AddClass("preview");
            _localPlayerPreview = localPlayerPreview.Add<VisualElement>().AddClass("preview-box").Add<Image>().AddClass("player-left-preview");
            _localPlayerReady = localPlayerPreview.Add<VisualElement>().AddClass("ready");
            var localPlayerFooter = localPlayerPreview.Add<VisualElement>().AddClass("preview-footer");
            _prevClassButton = localPlayerFooter.Add<RaisedButton>()
                .AddClass("player-local-class-button")
                .AddClass("player-local-class-prev")
                .SetColor(RaisedButtonColor.Orange)
                .BindClick(OnPrevClass);

            _localPlayerName = localPlayerFooter.Add<Label>().AddClass("preview-name").Text("Name");

            // Next class button
            _nextClassButton = localPlayerFooter.Add<RaisedButton>()
                .AddClass("player-local-class-button")
                .AddClass("player-local-class-next")
                .SetColor(RaisedButtonColor.Orange)
                .BindClick(OnNextClass);

            // Lanes
            var laneContainer = players.Add<VisualElement>().AddClass("lanes");
            laneContainer.Add<Label>().LocalizedText("lanes").AddClass("header");
            var group = laneContainer.Add<GroupBox>();
            _lanes = new RadioButton[4];
            for(int i=0; i<4; i++)
            {
                var laneIndex = i;
                _lanes[laneIndex] = group.Add<RadioButton>().AddClass("lane");
                _lanes[laneIndex].RegisterValueChangedCallback((evt) =>
                {
                    if(_playLaneClick)
                        AudioManager.Instance.PlayButtonClick();
                    if (evt.newValue) GameManager.Instance.Options.StartingLanes = laneIndex + 1;
                });
                _lanes[laneIndex].Add<VisualElement>().AddClass("raised").Add<Label>().Text((laneIndex + 1).ToString());
            }

            _lanes[1].SetEnabled(false);

            // Remote player
            _remotePlayer = players.Add<VisualElement>(name: "player-right").AddClass("player");
            _remotePlayer.Add<Label>().LocalizedText("remote-player").AddClass("header");
            var remotePlayerPreview = _remotePlayer.Add<VisualElement>().AddClass("preview");
            _remotePlayerPreview = remotePlayerPreview.Add<VisualElement>().AddClass("preview-box").Add<Image>().AddClass("player-right-preview");
            var remotePlayerFooter = remotePlayerPreview.Add<VisualElement>().AddClass("preview-footer");
            _remotePlayerName = remotePlayerFooter.Add<Label>().AddClass("preview-name").Text("Name");
            _remotePlayerReady = remotePlayerPreview.Add<VisualElement>().AddClass("ready");

            _readyButton = _panel.AddItem<RaisedButton>(name: "ready").SetColor(RaisedButtonColor.Blue).LocalizedText("ready").BindClick(OnReadyPressed);
            _readyButton.SetEnabled(false);

            // Join Code
            _joinCodeContainer = _remotePlayer.Add<VisualElement>().AddClass("join-code");
            _joinCodeContainer.Add<Label>().LocalizedText("join-code").AddClass("join-code-text");
            _joinCode = _joinCodeContainer.Add<Label>().AddClass("join-code-value").Text("X8B973"); 
        }

        public override void Initialize()
        {
            base.Initialize();

            _playerClasses = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).ToList();
            _localPlayerPreview.image = UIManager.Instance.PreviewLeftTexture;
        }

        private void OnQuit ()
        {
            if (GameManager.Instance.MaxPlayers > 1 && GameManager.Instance.PlayerCount > 1)
                UIManager.Instance.Confirm(
                    title: "leave?".Localized(),
                    message: (NetworkManager.Singleton.IsHost ? "confirm-close-lobby" : "confirm-leave-lobby").Localized(),
                    onYes: () => UIManager.Instance.ShowMainMenu(),
                    onNo: () => UIManager.Instance.ShowLobby());
            else
                UIManager.Instance.ShowMainMenu();    
        }

        private void OnPrevClass()
        {
            var index = _playerClasses.IndexOf(_localPlayerClass);
            if (index == -1)
                index = 0;
            else
                index = (index + _playerClasses.Count - 1) % _playerClasses.Count;

            SetLocalPlayerClass(_playerClasses[index]);

            GameManager.Instance.LocalPlayerController.PlayerClassId = _playerClasses[index].NetworkId;
        }

        private void OnNextClass()
        {
            var index = _playerClasses.IndexOf(_localPlayerClass);
            if (index == -1)
                index = 0;
            else
                index = (index + 1) % _playerClasses.Count;

            SetLocalPlayerClass(_playerClasses[index]);

            GameManager.Instance.LocalPlayerController.PlayerClassId = _playerClasses[index].NetworkId;
        }

        private void OnReadyPressed()
        {
            if (GameManager.Instance.IsSolo)
                UIManager.Instance.StartGame();
            else
            {
                var localClass = GameManager.Instance.LocalPlayerController.PlayerClass;
                var isReady = GameManager.Instance.LocalPlayerController.IsReady;
                if (!isReady && localClass.name != "RandomPlayer" && GameManager.Instance.Players.Count(p => p.IsReady && p.PlayerClassId == localClass.NetworkId) > 0)
                    UIManager.Instance.Confirm(title: "select-another-character".Localized(), message: "same-character".Localized(), yes: "ok".Localized(), onYes: () => UIManager.Instance.ShowLobby());
                else
                {
                    GameManager.Instance.LocalPlayerController.IsReady = !GameManager.Instance.LocalPlayerController.IsReady;
                    UpdateReadyState();
                }
            }
        }

        private IEnumerator StartGameCountdown (int seconds)
        {
            var wait = new WaitForSeconds(1);
            while (seconds > 0 && AreAllPlayersReady)
            {
                UpdateReadyButton(seconds);
                yield return wait;
                seconds--;
            }

            _startGameCountdown = null;

            UpdateReadyButton(CountdownSeconds);

            if(NetworkManager.Singleton.IsHost && AreAllPlayersReady)
                UIManager.Instance.StartGame();
        }

        private bool AreAllPlayersReady => GameManager.Instance.Players.Where(p => p.IsReady).Count() == GameManager.Instance.MaxPlayers;

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            SetLocalPlayerClass(GameManager.Instance.LocalPlayerController.PlayerClass, false);

            UIManager.Instance.GenerateBackground(GameManager.Instance.Options.StartingLanes);

            _playLaneClick = false;
            _lanes[GameManager.Instance.Options.StartingLanes - 1].value = true;
            _playLaneClick = true;

            _remotePlayer.EnableInClassList("hidden", GameManager.Instance.IsSolo);
            _panel.EnableInClassList("zisle-panel-solo", GameManager.Instance.IsSolo);

            if (GameManager.Instance.IsSolo)
            {
                _localPlayerHeader.text = "class".Localized();
                _panel.SetTitle("solo".Localized());
            }
            else
            {
                _localPlayerHeader.text = "local-player".Localized();
                _panel.SetTitle("lobby".Localized());
            }

            GameEvent<GameOptionStartingLanesChanged>.OnRaised += OnStartingLanesChanged;
            GameEvent<PlayerClassChanged>.OnRaised += OnPlayerClassChanged;
            GameEvent<PlayerReadyChanged>.OnRaised += OnPlayerReadyChanged;
            GameEvent<PlayerConnected>.OnRaised += OnPlayerConnected;
            GameEvent<PlayerDisconnected>.OnRaised += OnPlayerDisconnected;
            GameEvent<PlayerSpawned>.OnRaised += OnPlayerSpawned;

            UpdateReadyState();
            UpdateRemotePlayer(false);

            _readyButton.Focus();

            _joinCodeContainer.EnableInClassList("hidden", GameManager.Instance.JoinCode == null);
            _joinCode.text = GameManager.Instance.JoinCode ?? "";
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

            UpdateReadyState();
        }

        private void OnPlayerDisconnected(object sender, PlayerDisconnected evt)
        {
            if (!evt.PlayerController.IsLocalPlayer)
            {
                UpdateRemotePlayer();

                if(GameManager.Instance.LocalPlayerController != null)
                    GameManager.Instance.LocalPlayerController.IsReady = false;
            }

            UpdateReadyState();
        }

        private void UpdateReadyButton (int seconds)
        {
            if (GameManager.Instance.LocalPlayerController == null)
                return;

            var localReady = GameManager.Instance.LocalPlayerController.IsReady;
            _readyButton.SetEnabled(GameManager.Instance.PlayerCount == GameManager.Instance.MaxPlayers);
            _readyButton.SetColor(localReady ? RaisedButtonColor.Orange : RaisedButtonColor.Blue);

            _nextClassButton.SetEnabled(!localReady);
            _prevClassButton.SetEnabled(!localReady);

            for (int i = 0; i < 4; i++)
                _lanes[i].SetEnabled(!localReady);

            var text = "play".Localized();
            if (GameManager.Instance.MaxPlayers > 1)
            {
                if (GameManager.Instance.LocalPlayerController != null && localReady)
                {
                    text = "cancel".Localized();
                    if (seconds >= 0)
                        text += $" ({seconds})";
                }
                else
                    text = "ready".Localized();
            }

            _readyButton.text = text;
        }

        private void UpdateReadyState (int seconds=-1)
        {
            UpdateReadyButton(seconds);

            if (GameManager.Instance.IsSolo)
            {
                _localPlayerReady.AddClass("hidden");
                _remotePlayerReady.AddClass("hidden");
            }
            else
            {
                var remotePlayer = GameManager.Instance.Players.Where(p => !p.IsLocalPlayer).FirstOrDefault();
                _localPlayerReady.EnableInClassList("hidden", GameManager.Instance.LocalPlayerController == null || !GameManager.Instance.LocalPlayerController.IsReady);
                _remotePlayerReady.EnableInClassList("hidden", remotePlayer == null || !remotePlayer.IsReady);
            }

            if (AreAllPlayersReady)
            {
                if (_startGameCountdown != null)
                    UIManager.Instance.StopCoroutine(_startGameCountdown);

                _startGameCountdown = UIManager.Instance.StartCoroutine(StartGameCountdown(CountdownSeconds));
            }
            else if (_startGameCountdown != null)
            {
                UIManager.Instance.StopCoroutine(_startGameCountdown);
                _startGameCountdown = null;
            }
        }

        private void UpdateRemotePlayer(bool playEffect=true)
        {
            if (GameManager.Instance.IsSolo)
            {
                UIManager.Instance.ShowRightPreview(null);
                return;
            }

            _remotePlayerPreview.image = UIManager.Instance.PreviewRightTexture;

            var remotePlayer = GameManager.Instance.Players.Where(p => !p.IsLocalPlayer && !p.IsDisconnecting).FirstOrDefault();
            if (remotePlayer == null || remotePlayer.PlayerClass == null)
            {
                _remotePlayerName.text = (remotePlayer == null ? "waiting-for-player" : "connecting").Localized();
                UIManager.Instance.ShowRightPreview(UIManager.Instance.PreviewNoPlayer, false);
            }
            else
            {
                var remotePlayerClass = remotePlayer.PlayerClass;
                _remotePlayerName.text = remotePlayerClass.DisplayName;
                UIManager.Instance.ShowRightPreview(remotePlayerClass, playEffect);
            }
        }

        private void OnStartingLanesChanged(object sender, GameOptionStartingLanesChanged evt)
        {
            _playLaneClick = false;
            _lanes[evt.NewValue - 1].value = true;
            _playLaneClick = true;
            UIManager.Instance.GenerateBackground(evt.NewValue);
        }

        private void OnPlayerReadyChanged(object sender, PlayerReadyChanged evt) => UpdateReadyState();

        private void OnPlayerClassChanged(object sender, PlayerClassChanged evt)
        {
            if (!evt.PlayerController.IsLocalPlayer)
                UpdateRemotePlayer();
        }

        private void SetLocalPlayerClass(ActorDefinition def, bool playEffect=true)
        {
            _localPlayerName.text = def.DisplayName;
            UIManager.Instance.ShowLeftPreview(def,playEffect);
            _localPlayerClass = def;

            Options.PlayerClass = def.name;
        }
    }
}
