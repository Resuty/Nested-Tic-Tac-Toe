namespace WellWithinWell.Rules
{
    public sealed class GameState
    {
        public BoardState Board { get; }
        public Mark Turn { get; internal set; }
        public int MoveCount { get; internal set; }
        public int? RequiredRegion { get; internal set; }
        public BoardPosition? LastMove { get; internal set; }
        public MatchResult Result { get; internal set; }
        public WinningLine? Line { get; internal set; }

        public GameState() : this(new BoardState(), Mark.O) { }

        internal GameState(BoardState board, Mark turn)
        {
            Board = board;
            Turn = turn;
        }

        internal GameState Copy() => new GameState(Board.Copy(), Turn)
        {
            MoveCount = MoveCount,
            RequiredRegion = RequiredRegion,
            LastMove = LastMove,
            Result = Result,
            Line = Line
        };

        public int CountRegions(RegionResult result)
        {
            int count = 0;
            for (int i = 0; i < BoardState.Size; i++)
                if (Board[i].Result == result)
                    count++;
            return count;
        }
    }
}
