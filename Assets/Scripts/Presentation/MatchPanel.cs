using UnityEngine;
using UnityEngine.UI;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    public sealed class MatchPanel : MonoBehaviour
    {
        [SerializeField] private BoardView board;
        [SerializeField] private BoardSkin art;
        [SerializeField] private Text modeLabel;
        [SerializeField] private Text turnHeader;
        [SerializeField] private Text turnLabel;
        [SerializeField] private Image turnPiece;
        [SerializeField] private Text legalHeading;
        [SerializeField] private Text legalLabel;
        [SerializeField] private Text reason;
        [SerializeField] private Text seatO;
        [SerializeField] private Text seatX;
        [SerializeField] private Text capturedO;
        [SerializeField] private Text capturedX;
        [SerializeField] private Text turnNumber;
        [SerializeField] private Text lastMove;
        [SerializeField] private AcceptButton restart;
        [SerializeField] private AcceptButton home;

        public BoardView Board => board;
        public Button Restart => restart.Button;
        public Button Home => home.Button;

        public void Present(GameController game, bool againstComputer, bool computerTurn, GameLanguage language)
        {
            GameState state = game.State;
            bool playing = state.Result == MatchResult.InProgress;
            bool draw = state.Result == MatchResult.Draw;
            string seat = SeatName(game.SeatFor(state.Turn), againstComputer, language);
            modeLabel.text = TextCatalog.Get(againstComputer ? TextKey.Computer : TextKey.Local, language);
            restart.Caption = TextCatalog.Get(playing ? TextKey.Restart : TextKey.PlayAgain, language);
            turnHeader.text = TextCatalog.Get(playing ? TextKey.CurrentTurn : TextKey.Result, language);
            legalHeading.text = TextCatalog.Get(playing ? TextKey.LegalHeading : TextKey.FinalBoard, language);
            turnPiece.sprite = draw ? art.Draw : art.Piece(state.Turn);
            turnLabel.text = state.Result switch
            {
                MatchResult.Draw => TextCatalog.Get(TextKey.Draw, language),
                MatchResult.InProgress when computerTurn => TextCatalog.Get(TextKey.Thinking, language),
                MatchResult.InProgress when againstComputer => TextCatalog.Get(TextKey.YourTurn, language),
                MatchResult.InProgress => TextCatalog.Get(TextKey.PlayerTurn, language, seat),
                _ => TextCatalog.Get(TextKey.Win, language, seat)
            };
            turnNumber.text = TextCatalog.Get(playing ? TextKey.MoveNumber : TextKey.MoveTotal, language,
                playing ? state.MoveCount + 1 : state.MoveCount);
            seatO.text = SeatName(game.SeatFor(Mark.O), againstComputer, language);
            seatX.text = SeatName(game.SeatFor(Mark.X), againstComputer, language);
            capturedO.text = state.CountRegions(RegionResult.WonByO).ToString();
            capturedX.text = state.CountRegions(RegionResult.WonByX).ToString();

            if (state.LastMove.HasValue)
            {
                BoardPosition move = state.LastMove.Value;
                lastMove.text = TextCatalog.Get(TextKey.LastMove, language, RegionName(move.Region, language), move.Cell + 1);
            }
            else
                lastMove.text = TextCatalog.Get(TextKey.NoMove, language);

            legalLabel.text = state.Result switch
            {
                MatchResult.Draw => TextCatalog.Get(TextKey.AllClosed, language),
                MatchResult.WonByO or MatchResult.WonByX => TextCatalog.Get(TextKey.ThreeBoards, language),
                _ => state.RequiredRegion.HasValue
                    ? TextCatalog.Get(TextKey.Board, language, RegionName(state.RequiredRegion.Value, language))
                    : TextCatalog.Get(TextKey.FreeMove, language)
            };
            if (computerTurn)
                reason.text = TextCatalog.Get(TextKey.ComputerReason, language);
            else
                Explain(state, null, language);
        }

        public void Explain(GameState state, BoardPosition? hover, GameLanguage language)
        {
            if (state.Result == MatchResult.Draw)
            {
                reason.text = TextCatalog.Get(TextKey.DrawReason, language);
                return;
            }
            if (state.Line.HasValue)
            {
                WinningLine line = state.Line.Value;
                reason.text = TextCatalog.Get(TextKey.WinReason, language, RegionName(line.First, language),
                    RegionName(line.Middle, language), RegionName(line.Last, language));
                return;
            }

            if (hover.HasValue)
            {
                int to = hover.Value.Cell;
                TextKey key = state.Board[to].Result == RegionResult.Open ? TextKey.OpenHover : TextKey.ClosedHover;
                reason.text = TextCatalog.Get(key, language, to + 1, RegionName(to, language));
                return;
            }

            if (!state.LastMove.HasValue)
                reason.text = TextCatalog.Get(TextKey.FirstReason, language);
            else if (state.RequiredRegion.HasValue)
                reason.text = TextCatalog.Get(TextKey.ForcedReason, language, state.LastMove.Value.Cell + 1);
            else
                reason.text = TextCatalog.Get(TextKey.FreeReason, language, RegionName(state.LastMove.Value.Cell, language));
        }

        private static string RegionName(int index, GameLanguage language) =>
            TextCatalog.Get(TextCatalog.RegionKey(index), language);

        private static string SeatName(Seat seat, bool againstComputer, GameLanguage language)
        {
            if (againstComputer)
                return TextCatalog.Get(seat == Seat.One ? TextKey.You : TextKey.Cpu, language);
            return TextCatalog.Get(seat == Seat.One ? TextKey.PlayerOne : TextKey.PlayerTwo, language);
        }

        public void Wire(BoardView playingBoard, BoardSkin skin, Text mode, Text heading, Text player, Image piece,
            Text targetHeading, Text target, Text explanation, Text playerO, Text playerX, Text countO, Text countX,
            Text hand, Text recent, AcceptButton replay, AcceptButton leave)
        {
            board = playingBoard;
            art = skin;
            modeLabel = mode;
            turnHeader = heading;
            turnLabel = player;
            turnPiece = piece;
            legalHeading = targetHeading;
            legalLabel = target;
            reason = explanation;
            seatO = playerO;
            seatX = playerX;
            capturedO = countO;
            capturedX = countX;
            turnNumber = hand;
            lastMove = recent;
            restart = replay;
            home = leave;
        }
    }
}
