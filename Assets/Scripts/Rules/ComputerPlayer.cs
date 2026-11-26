using System;
using System.Collections.Generic;

namespace WellWithinWell.Rules
{
    public static class ComputerPlayer
    {
        private const int WinScore = 100_000;

        public static BoardPosition? ChooseMove(GameState state)
        {
            Mark player = state.Turn;
            BoardPosition? best = null;
            int bestScore = int.MinValue;
            foreach (BoardPosition move in LegalMoves(state))
            {
                GameState afterMove = state.Copy();
                GameRules.TryPlay(afterMove, move);
                int score = Evaluate(afterMove, player);
                if (score == WinScore)
                    return move;

                if (afterMove.Result == MatchResult.InProgress)
                {
                    score = int.MaxValue;
                    foreach (BoardPosition reply in LegalMoves(afterMove))
                    {
                        GameState afterReply = afterMove.Copy();
                        GameRules.TryPlay(afterReply, reply);
                        score = Math.Min(score, Evaluate(afterReply, player));
                        if (score <= bestScore)
                            break;
                    }
                }

                if (score > bestScore)
                {
                    best = move;
                    bestScore = score;
                }
            }
            return best;
        }

        private static IEnumerable<BoardPosition> LegalMoves(GameState state)
        {
            for (int region = 0; region < BoardState.Size; region++)
            {
                if (!GameRules.IsRegionLegal(state, region))
                    continue;
                for (int cell = 0; cell < BoardState.Size; cell++)
                    if (state.Board[region][cell] == Mark.Empty)
                        yield return new BoardPosition(region, cell);
            }
        }

        private static int Evaluate(GameState state, Mark player)
        {
            if (state.Result == MatchResult.Draw)
                return 0;
            if (state.Result != MatchResult.InProgress)
                return state.Turn == player ? WinScore : -WinScore;

            int score = 0;
            for (int i = 0; i < BoardState.Size; i++)
            {
                RegionState region = state.Board[i];
                if (region.Result == RegionResult.Open)
                {
                    score += 2 * Signed(region[4], player);
                    foreach (WinningLine line in GameRules.Lines)
                        score += LineScore(Signed(region[line.First], player),
                            Signed(region[line.Middle], player), Signed(region[line.Last], player));
                }
                else if (region.Result != RegionResult.Draw)
                {
                    score += (i == 4 ? 85 : 65) * Owned(region.Result, player);
                }
            }
            foreach (WinningLine line in GameRules.Lines)
            {
                RegionResult first = state.Board[line.First].Result;
                RegionResult middle = state.Board[line.Middle].Result;
                RegionResult last = state.Board[line.Last].Result;
                if (first != RegionResult.Draw && middle != RegionResult.Draw && last != RegionResult.Draw)
                    score += 13 * LineScore(Owned(first, player), Owned(middle, player), Owned(last, player));
            }
            return score;
        }

        private static int LineScore(int first, int middle, int last)
        {
            int sum = first + middle + last;
            if (Math.Abs(sum) != Math.Abs(first) + Math.Abs(middle) + Math.Abs(last))
                return 0;
            return sum * Math.Abs(sum);
        }

        private static int Signed(Mark mark, Mark player) => mark == Mark.Empty ? 0 : mark == player ? 1 : -1;

        private static int Owned(RegionResult result, Mark player) => result switch
        {
            RegionResult.WonByO => Signed(Mark.O, player),
            RegionResult.WonByX => Signed(Mark.X, player),
            _ => 0
        };
    }
}
