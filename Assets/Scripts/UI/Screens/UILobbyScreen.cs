using NoZ.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UILobbyScreen : UIScreen
    {
        public const int CountdownSeconds = 5;

        public override bool BlurBackground => true;

        private List<ActorDefinition> _playerClasses;

        private Panel _panel;

        private Image _localPlayerPreview;
        private Label _localPlayerName;
        private Label _localPlayerHeader;
        private ActorDefinition _localPlayerClass;
        private VisualElement _localPlayerReady;

        private VisualElement _remotePlayer;
        private VisualElement _remotePlayerReady;
        private Image _remotePlayerPreview;
        private Label _remotePlayerName;

        private VisualElement _localPlayerClassNext;
        private VisualElement _localPlayerClassPrev;
        private RaisedButton _readyButton;
        private RadioButton[] _lanes;

        private VisualElement _joinCodeContainer;
        private Label _joinCode;

        private bool _playLaneClick = true;
        private Coroutine _startGameCountdown;

        protected override void Awake()
        {
            base.Awake();

            //if (GameManager.Instance == null && GameManager.Instance.LocalPlayerController != null)
            //  return;

            _panel = Q<Panel>("panel");
            _panel.OnClose(OnQuit);
            _localPlayerPreview = Q<Image>("local-player-preview");
            _localPlayerName = Q<Label>("local-player-name");
            _localPlayerHeader = Q<Label>("local-player-header");
            _localPlayerClassNext = Q("local-player-class-next").BindClick(OnNextClass);
            _localPlayerClassPrev = Q("local-player-class-prev").BindClick(OnPrevClass);
            _localPlayerReady = Q("local-player-ready");
            _localPlayerPreview.image = UIManager.Instance.PreviewLeftTexture;

            _remotePlayer = Q("remote-player");
            _remotePlayerPreview = Q<Image>("remote-player-preview");
            _remotePlayerName = Q<Label>("remote-player-name");
            _remotePlayerReady = Q("remote-player-ready");

            _readyButton = BindClick<RaisedButton>("ready", OnReadyPressed);
            _joinCodeContainer = Q("join-code");
            _joinCode = Q<Label>("join-code-value");

            _lanes = new RadioButton[4];
            for (int i = 1; i <= 4; i++)
            {
                var laneIndex = i;
                _lanes[laneIndex - 1] = Q<RadioButton>($"lane-{laneIndex}");
                _lanes[laneIndex - 1].RegisterValueChangedCallback((evt) =>
                {
                    if (!evt.newValue)
                        return;

                    if (_playLaneClick)
                        AudioManager.Instance.PlayButtonClick();
                    GameManager.Instance.Options.StartingLanes = laneIndex;
                });
            }

            _playerClasses = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).ToList();
        }

        protected override void OnShow()
        {
            base.OnShow();
    
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
            
            _joinCode.text = GameManager.Instance.JoinCode ?? "";
        }

        public override void OnAfterTransitionIn()
        {
            base.OnAfterTransitionIn();
            _readyButton.Focus();
        }

        private void OnQuit ()
        {
            if (GameManager.Instance.MaxPlayers > 1 && GameManager.Instance.PlayerCount > 1)
                UIManager.Instance.Confirm(
                    title: "leave?".Localized(),
                    message: (NetworkManager.Singleton.IsHost ? "confirm-close-lobby" : "confirm-leave-lobby").Localized(),
                    onYes: () => UIManager.Instance.ShowTitle(),
                    onNo: () => UIManager.Instance.ShowLobby());
            else
                UIManager.Instance.ShowTitle();    
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
                AudioManager.Instance.PlayTimerTick();
                seconds--;
            }

            _startGameCountdown = null;

            UpdateReadyButton(0);

            if(NetworkManager.Singleton.IsHost && AreAllPlayersReady)
                UIManager.Instance.StartGame();
        }

        private bool AreAllPlayersReady => GameManager.Instance.Players.Where(p => p.IsReady).Count() == GameManager.Instance.MaxPlayers;


        private void OnPlayerSpawned(object sender, PlayerSpawned evt)
        {
            if (evt.Player.IsOwner && !NetworkManager.Singleton.IsHost)
                UIManager.Instance.StartGame();
        }

        protected override void OnHide ()
        {
            base.OnHide();

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
            _readyButton.SetEnabled(GameManager.Instance.PlayerCount == GameManager.Instance.MaxPlayers && seconds != 0);
            _readyButton.EnableInClassList(USS.ButtonOrange, localReady);
            _readyButton.EnableInClassList(USS.ButtonBlue, !localReady);

            _localPlayerClassNext.SetEnabled(!localReady);
            _localPlayerClassPrev.SetEnabled(!localReady);

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
                _joinCodeContainer.EnableInClassList(USS.Hidden, string.IsNullOrEmpty(GameManager.Instance.JoinCode));
                _remotePlayerName.text = (remotePlayer == null ? "waiting-for-player" : "connecting").Localized();
                UIManager.Instance.ShowRightPreview(UIManager.Instance.PreviewNoPlayer, false);
            }
            else
            {
                var remotePlayerClass = remotePlayer.PlayerClass;
                _remotePlayerName.text = remotePlayerClass.DisplayName;
                _joinCodeContainer.EnableInClassList(USS.Hidden, true);
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

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnQuit();
        }
    }
}
