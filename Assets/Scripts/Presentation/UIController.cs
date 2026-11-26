using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    public sealed class UIController : MonoBehaviour
    {
        private enum Screen { Menu, Mode, Match, Rules, Settings }
        private const string LanguagePreference = "WellWithinWell.Language";

        [SerializeField] private GameObject[] screens;
        [SerializeField] private Button start;
        [SerializeField] private Button rules;
        [SerializeField] private Button settings;
        [SerializeField] private Button exit;
        [SerializeField] private Button[] backButtons;
        [SerializeField] private Button local;
        [SerializeField] private Button computer;
        [SerializeField] private GameObject orderPrompt;
        [SerializeField] private Button first;
        [SerializeField] private Button second;
        [SerializeField] private Button cancelOrder;
        [SerializeField] private MatchPanel match;
        [SerializeField] private BoardView menuBoard;
        [SerializeField] private AudioController audioOutput;
        [SerializeField] private Slider volume;
        [SerializeField] private Text volumeValue;
        [SerializeField] private AcceptButton mute;
        [SerializeField] private GameObject confirm;
        [SerializeField] private Text confirmTitle;
        [SerializeField] private Text confirmDetail;
        [SerializeField] private AcceptButton confirmYes;
        [SerializeField] private Button confirmNo;
        [SerializeField] private LocalizedLabel[] localizedLabels;
        [SerializeField] private Button[] languageChoices;
        [SerializeField] private Image[] languageMarkers;

        private readonly GameController game = new GameController();
        private Screen screen;
        private Action pending;
        private bool againstComputer;
        private Coroutine computerMove;
        private GameLanguage language;

        private void Awake()
        {
            match.Board.Bind(Play, hover => match.Explain(game.State, hover, language));
            menuBoard.Bind(null, null);
            var preview = new GameState();
            int[] path = { 4, 4, 4, 0, 0, 4, 4, 1, 1, 4, 4, 2, 2, 4, 4, 3, 3, 5, 5, 2, 2, 5 };
            for (int i = 0; i < path.Length; i += 2)
                GameRules.TryPlay(preview, new BoardPosition(path[i], path[i + 1]));
            menuBoard.Present(preview, false);

            Bind(start, () => Show(Screen.Mode));
            Bind(rules, () => Show(Screen.Rules));
            Bind(settings, OpenSettings);
            Bind(exit, () => Ask(TextKey.QuitTitle, null, TextKey.Quit, Application.Quit));
            foreach (Button button in backButtons)
                Bind(button, () =>
                {
                    if (screen == Screen.Settings)
                        audioOutput.Save();
                    Show(Screen.Menu);
                });
            Bind(local, () => StartMatch(Seat.One, false));
            Bind(computer, OpenOrder);
            Bind(first, () => StartMatch(Seat.One, true));
            Bind(second, () => StartMatch(Seat.Two, true));
            Bind(cancelOrder, () =>
            {
                ClearSelection();
                orderPrompt.SetActive(false);
                QueueComputerMove();
            });
            Bind(match.Restart, () =>
            {
                if (game.State.Result == MatchResult.InProgress && game.State.MoveCount != 0)
                    Ask(TextKey.Restart, TextKey.EndMatch, TextKey.Restart, Replay);
                else
                    Replay();
            });
            Bind(match.Home, () =>
            {
                if (game.State.Result == MatchResult.InProgress && game.State.MoveCount != 0)
                    Ask(TextKey.Menu, TextKey.EndMatch, TextKey.Back, () => Show(Screen.Menu));
                else
                    Show(Screen.Menu);
            });
            Bind(confirmNo, CloseConfirmation);
            Bind(confirmYes.Button, () =>
            {
                Action accept = pending;
                CloseConfirmation();
                accept?.Invoke();
            });
            Bind(mute.Button, () =>
            {
                audioOutput.SetMuted(!audioOutput.Muted);
                RefreshAudio();
            });
            volume.onValueChanged.AddListener(value =>
            {
                audioOutput.SetVolume(value);
                RefreshAudio();
            });
            for (int i = 0; i < languageChoices.Length; i++)
            {
                GameLanguage choice = (GameLanguage)i;
                Bind(languageChoices[i], () =>
                {
                    ApplyLanguage(choice);
                    RefreshAudio();
                    PlayerPrefs.SetInt(LanguagePreference, (int)choice);
                    PlayerPrefs.Save();
                });
            }
            int storedLanguage = PlayerPrefs.GetInt(LanguagePreference, 0);
            ApplyLanguage((uint)storedLanguage < languageChoices.Length
                ? (GameLanguage)storedLanguage : GameLanguage.SimplifiedChinese);
            Show(Screen.Menu);
        }

        private void OnEnable()
        {
            game.StateChanged += RenderMatch;
            QueueComputerMove();
        }

        private void OnDisable()
        {
            game.StateChanged -= RenderMatch;
            CancelComputerMove();
        }

        private void Bind(Button button, UnityAction action)
        {
            button.onClick.AddListener(() =>
            {
                audioOutput.Button();
                action();
            });
        }

        private void Show(Screen target)
        {
            CancelComputerMove();
            screen = target;
            ClearSelection();
            for (int i = 0; i < screens.Length; i++)
                screens[i].SetActive(i == (int)target);
        }

        private void OpenOrder()
        {
            CancelComputerMove();
            ClearSelection();
            orderPrompt.SetActive(true);
        }

        private void Replay()
        {
            if (againstComputer)
                OpenOrder();
            else
                StartMatch(Seat.One, false);
        }

        private void StartMatch(Seat openingSeat, bool computerOpponent)
        {
            againstComputer = computerOpponent;
            orderPrompt.SetActive(false);
            Show(Screen.Match);
            game.StartMatch(openingSeat);
        }

        private void Play(BoardPosition position)
        {
            if (screen == Screen.Match && !confirm.activeSelf && !orderPrompt.activeSelf && !ComputerIsNext())
                Commit(position);
        }

        private void Commit(BoardPosition position)
        {
            if (!game.TryPlay(position))
                return;

            audioOutput.Stone();
            GameState state = game.State;
            if (state.Board[position.Region].Line.HasValue)
                audioOutput.Claim();
            if (state.Result != MatchResult.InProgress)
                audioOutput.End();
        }

        private void RenderMatch(GameState state)
        {
            bool computerTurn = ComputerIsNext();
            match.Board.Present(state, state.LastMove.HasValue, !computerTurn);
            match.Present(game, againstComputer, computerTurn, language);
            QueueComputerMove();
        }

        private bool ComputerIsNext() => againstComputer && game.State.Result == MatchResult.InProgress
            && game.SeatFor(game.State.Turn) == Seat.Two;

        private void QueueComputerMove()
        {
            if (screen == Screen.Match && !confirm.activeSelf && !orderPrompt.activeSelf
                && computerMove == null && ComputerIsNext())
                computerMove = StartCoroutine(PlayComputerMove());
        }

        private IEnumerator PlayComputerMove()
        {
            yield return new WaitForSecondsRealtime(0.32f);
            computerMove = null;
            BoardPosition? move = ComputerPlayer.ChooseMove(game.State);
            if (move.HasValue)
                Commit(move.Value);
        }

        private void CancelComputerMove()
        {
            if (computerMove == null)
                return;
            StopCoroutine(computerMove);
            computerMove = null;
        }

        private void Ask(TextKey title, TextKey? detail, TextKey accept, Action action)
        {
            CancelComputerMove();
            pending = action;
            confirmTitle.text = TextCatalog.Get(title, language);
            confirmDetail.text = detail.HasValue ? TextCatalog.Get(detail.Value, language) : "";
            confirmYes.Caption = TextCatalog.Get(accept, language);
            ClearSelection();
            confirm.SetActive(true);
        }

        private void CloseConfirmation()
        {
            pending = null;
            ClearSelection();
            confirm.SetActive(false);
            QueueComputerMove();
        }

        private void OpenSettings()
        {
            volume.SetValueWithoutNotify(audioOutput.Volume);
            RefreshAudio();
            Show(Screen.Settings);
        }

        private void RefreshAudio()
        {
            volumeValue.text = Mathf.RoundToInt(audioOutput.Volume * 100) + "%";
            mute.Caption = TextCatalog.Get(audioOutput.Muted ? TextKey.Unmute : TextKey.Mute, language);
        }

        private void ApplyLanguage(GameLanguage choice)
        {
            language = choice;
            foreach (LocalizedLabel label in localizedLabels)
                label.Apply(language);
            for (int i = 0; i < languageMarkers.Length; i++)
                languageMarkers[i].enabled = i == (int)language;
            menuBoard.LocalizeRegions(language);
            match.Board.LocalizeRegions(language);
        }

        private static void ClearSelection()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void Wire(GameObject[] pages, Button play, Button learn, Button preferences, Button quit,
            Button[] backs, Button localMatch, Button computerMatch, GameObject order, Button goFirst,
            Button goSecond, Button dismissOrder, MatchPanel hud, BoardView cover, AudioController sound,
            Slider master, Text percentage, AcceptButton silence, GameObject dialog, Text promptTitle,
            Text promptText, AcceptButton confirmButton, Button cancel)
        {
            screens = pages;
            start = play;
            rules = learn;
            settings = preferences;
            exit = quit;
            backButtons = backs;
            local = localMatch;
            computer = computerMatch;
            orderPrompt = order;
            first = goFirst;
            second = goSecond;
            cancelOrder = dismissOrder;
            match = hud;
            menuBoard = cover;
            audioOutput = sound;
            volume = master;
            volumeValue = percentage;
            mute = silence;
            confirm = dialog;
            confirmTitle = promptTitle;
            confirmDetail = promptText;
            confirmYes = confirmButton;
            confirmNo = cancel;
        }

        public void WireLanguage(LocalizedLabel[] labels, Button[] choices, Image[] markers)
        {
            localizedLabels = labels;
            languageChoices = choices;
            languageMarkers = markers;
        }
    }
}
