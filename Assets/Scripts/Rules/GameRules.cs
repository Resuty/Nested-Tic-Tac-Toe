using System;

namespace WellWithinWell.Rules
{
    public static class GameRules
    {
        private static readonly WinningLine[] lines =
        {
            new WinningLine(0, 1, 2), new WinningLine(3, 4, 5), new WinningLine(6, 7, 8),
            new WinningLine(0, 3, 6), new WinningLine(1, 4, 7), new WinningLine(2, 5, 8),
            new WinningLine(0, 4, 8), new WinningLine(2, 4, 6)
        };

        internal static ReadOnlySpan<WinningLine> Lines => lines;

        public static bool IsRegionLegal(GameState state, int region)
        {
            return state.Result == MatchResult.InProgress
                && state.Board[region].Result == RegionResult.Open
                && (!state.RequiredRegion.HasValue || state.RequiredRegion.Value == region);
        }

        public static bool IsLegal(GameState state, BoardPosition position)
        {
            return IsRegionLegal(state, position.Region)
                && state.Board[position.Region][position.Cell] == Mark.Empty;
        }

        public static bool TryPlay(GameState state, BoardPosition position)
        {
            if (!IsLegal(state, position))
                return false;

            RegionState region = state.Board[position.Region];
            Mark mover = state.Turn;
            region.Place(position.Cell, mover);
            state.LastMove = position;
            state.MoveCount++;

            if (region.Line.HasValue)
            {
                RegionResult owned = region.Result;
                state.Line = FindLine(index => state.Board[index].Result == owned);
                if (state.Line.HasValue)
                    state.Result = mover == Mark.O ? MatchResult.WonByO : MatchResult.WonByX;
            }

            if (state.Result == MatchResult.InProgress && state.CountRegions(RegionResult.Open) == 0)
                state.Result = MatchResult.Draw;

            state.RequiredRegion = null;
            if (state.Result == MatchResult.InProgress)
            {
                state.Turn = mover == Mark.O ? Mark.X : Mark.O;
                if (state.Board[position.Cell].Result == RegionResult.Open)
                    state.RequiredRegion = position.Cell;
            }
            return true;
        }

        internal static WinningLine? FindLine(Func<int, bool> belongsToPlayer)
        {
            foreach (WinningLine line in lines)
                if (belongsToPlayer(line.First) && belongsToPlayer(line.Middle) && belongsToPlayer(line.Last))
                    return line;
            return null;
        }
    }
}
