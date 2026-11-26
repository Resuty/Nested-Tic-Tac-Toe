using System;
using System.Collections.Generic;
using NUnit.Framework;
using WellWithinWell.Rules;

namespace WellWithinWell.Tests
{
    public sealed class GameRulesTests
    {
        private static BoardPosition At(int region, int cell) => new BoardPosition(region - 1, cell - 1);

        [Test]
        public void EveryOneOfThe81CellsIsAValidFirstMove()
        {
            for (int region = 1; region <= 9; region++)
                for (int cell = 1; cell <= 9; cell++)
                {
                    var state = new GameState();
                    var position = At(region, cell);
                    Assert.That(GameRules.TryPlay(state, position), Is.True);
                    Assert.That(state.Board[region - 1][cell - 1], Is.EqualTo(Mark.O));
                    Assert.That(state.RequiredRegion, Is.EqualTo(cell - 1));
                    Assert.That(state.LastMove, Is.EqualTo(position));
                    Assert.That(state.Turn, Is.EqualTo(Mark.X));
                }
        }

        [Test]
        public void MovesSendTheOtherPlayerAndAlternateMarks()
        {
            var state = new GameState();
            Assert.That(GameRules.TryPlay(state, At(2, 8)), Is.True);
            Assert.That(state.RequiredRegion, Is.EqualTo(7));
            Assert.That(state.Turn, Is.EqualTo(Mark.X));
            Assert.That(GameRules.TryPlay(state, At(8, 3)), Is.True);
            Assert.That(state.RequiredRegion, Is.EqualTo(2));
            Assert.That(state.Board[7][2], Is.EqualTo(Mark.X));
            Assert.That(state.Turn, Is.EqualTo(Mark.O));
        }

        [TestCase(5, 5)]
        [TestCase(2, 5)]
        public void InvalidMoveLeavesEveryObservableFieldIntact(int region, int cell)
        {
            var state = new GameState();
            GameRules.TryPlay(state, At(5, 5));
            Assert.That(GameRules.TryPlay(state, At(region, cell)), Is.False);
            Assert.That(state.LastMove, Is.EqualTo(At(5, 5)));
            Assert.That(state.Turn, Is.EqualTo(Mark.X));
            Assert.That(state.MoveCount, Is.EqualTo(1));
            Assert.That(state.RequiredRegion, Is.EqualTo(4));
            Assert.That(state.Result, Is.EqualTo(MatchResult.InProgress));
            Assert.That(state.Line, Is.Null);
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    Assert.That(state.Board[i][j], Is.EqualTo(i == 4 && j == 4 ? Mark.O : Mark.Empty));
        }

        [TestCase(1, 2, 3)]
        [TestCase(1, 4, 7)]
        [TestCase(1, 5, 9)]
        [TestCase(3, 5, 7)]
        public void LocalWinLocksRegionAndRetainsMarks(int a, int b, int c)
        {
            var board = new BoardState();
            BoardPosition move = At(2, c);
            PrepareWin(board[move.Region], Mark.X, a, b);
            var state = new GameState(board, Mark.X);
            Assert.That(GameRules.TryPlay(state, move), Is.True);
            Assert.That(board[1].Result, Is.EqualTo(RegionResult.WonByX));
            AssertLine(board[1].Line, a, b, c);
            Assert.That(board[1][a - 1], Is.EqualTo(Mark.X));
            Assert.That(board[1].Filled, Is.EqualTo(3));
            foreach (BoardPosition option in Coordinates())
                if (option.Region == 1)
                    Assert.That(GameRules.IsLegal(state, option), Is.False);
        }

        [Test]
        public void DrawOccursOnNinthCellAndTakesNoOwnership()
        {
            var board = new BoardState();
            PrepareDraw(board[8], false);
            var state = new GameState(board, Mark.O);
            Assert.That(board[8].Result, Is.EqualTo(RegionResult.Open));
            Assert.That(GameRules.TryPlay(state, At(9, 9)), Is.True);
            Assert.That(board[8].Filled, Is.EqualTo(9));
            Assert.That(board[8].Result, Is.EqualTo(RegionResult.Draw));
            Assert.That(board[8].Line, Is.Null);
            Assert.That(state.CountRegions(RegionResult.WonByO), Is.Zero);
            Assert.That(state.CountRegions(RegionResult.WonByX), Is.Zero);
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(GameRules.IsRegionLegal(state, 8), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SendingToClosedRegionFreesAllOpenRegions(bool drawn)
        {
            var board = new BoardState();
            if (drawn)
                PrepareDraw(board[4]);
            else
                WinRegion(board[4], Mark.O);
            var state = new GameState(board, Mark.X);
            Assert.That(GameRules.TryPlay(state, At(2, 5)), Is.True);
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(state.Turn, Is.EqualTo(Mark.O));
            Assert.That(GameRules.TryPlay(state, At(5, 4)), Is.False);
            Assert.That(GameRules.IsLegal(state, At(1, 1)), Is.True);
            Assert.That(GameRules.IsLegal(state, At(8, 3)), Is.True);
        }

        [Test]
        public void TargetIsEvaluatedAfterTheMoveClosesItsOwnRegion()
        {
            var board = new BoardState();
            PrepareWin(board[2], Mark.O, 1, 2);
            var state = new GameState(board, Mark.O) { RequiredRegion = 2 };
            Assert.That(GameRules.TryPlay(state, At(3, 3)), Is.True);
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(GameRules.IsLegal(state, At(8, 8)), Is.True);
            Assert.That(GameRules.TryPlay(state, At(3, 7)), Is.False);
        }

        [TestCase(1, 2, 3, Mark.O)]
        [TestCase(1, 4, 7, Mark.X)]
        [TestCase(1, 5, 9, Mark.O)]
        [TestCase(3, 5, 7, Mark.X)]
        public void GlobalLineStopsPlayBeforeSchedulingAnotherTurn(int a, int b, int c, Mark winner)
        {
            var board = new BoardState();
            WinRegion(board[a - 1], winner);
            WinRegion(board[b - 1], winner);
            PrepareWin(board[c - 1], winner, 1, 2);
            var state = new GameState(board, winner) { RequiredRegion = c - 1 };
            Assert.That(GameRules.TryPlay(state, At(c, 3)), Is.True);
            Assert.That(state.Result, Is.EqualTo(winner == Mark.O ? MatchResult.WonByO : MatchResult.WonByX));
            AssertLine(state.Line, a, b, c);
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(state.Turn, Is.EqualTo(winner));
            foreach (BoardPosition option in Coordinates())
                Assert.That(GameRules.TryPlay(state, option), Is.False);
            Assert.That(state.MoveCount, Is.EqualTo(1));
        }

        [Test]
        public void AGlobalLineOnTheFinalRegionTakesPriorityOverDraw()
        {
            var board = new BoardState();
            WinRegion(board[0], Mark.O);
            WinRegion(board[1], Mark.O);
            PrepareWin(board[2], Mark.O, 1, 2);
            for (int i = 3; i < 9; i++)
                PrepareDraw(board[i]);
            var state = new GameState(board, Mark.O);
            Assert.That(GameRules.TryPlay(state, At(3, 3)), Is.True);
            Assert.That(state.CountRegions(RegionResult.Open), Is.Zero);
            Assert.That(state.Result, Is.EqualTo(MatchResult.WonByO));
        }

        [Test]
        public void FinalDrawIgnoresUnequalOwnershipAndWaitsForLastMove()
        {
            var board = new BoardState();
            WinRegion(board[0], Mark.O);
            WinRegion(board[1], Mark.O);
            WinRegion(board[4], Mark.X);
            for (int i = 2; i <= 7; i++)
                if (i != 4)
                    PrepareDraw(board[i]);
            PrepareDraw(board[8], false, skip: 5);
            var state = new GameState(board, Mark.X);
            Assert.That(GameRules.TryPlay(state, At(9, 6)), Is.True);
            Assert.That(state.Result, Is.EqualTo(MatchResult.InProgress));
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(GameRules.TryPlay(state, At(9, 9)), Is.True);
            Assert.That(state.Result, Is.EqualTo(MatchResult.Draw));
            Assert.That(state.RequiredRegion, Is.Null);
            Assert.That(state.CountRegions(RegionResult.WonByO), Is.EqualTo(2));
            Assert.That(state.CountRegions(RegionResult.WonByX), Is.EqualTo(1));
            Assert.That(state.CountRegions(RegionResult.Draw), Is.EqualTo(6));
        }

        [Test]
        public void RestartResetsTheBoardAndOnlyAcceptedMovesPublish()
        {
            var game = new GameController();
            var events = new List<GameState>();
            game.StateChanged += events.Add;
            game.StartMatch(Seat.Two);
            Assert.That(game.SeatFor(Mark.O), Is.EqualTo(Seat.Two));
            Assert.That(game.SeatFor(Mark.X), Is.EqualTo(Seat.One));
            game.TryPlay(At(5, 5));
            Assert.That(game.TryPlay(At(5, 5)), Is.False);
            GameState old = game.State;
            game.StartMatch(Seat.One);
            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events[2], Is.SameAs(game.State));
            Assert.That(game.State, Is.Not.SameAs(old));
            Assert.That(game.State.Turn, Is.EqualTo(Mark.O));
            Assert.That(game.State.MoveCount, Is.Zero);
            Assert.That(game.State.LastMove, Is.Null);
            Assert.That(game.State.RequiredRegion, Is.Null);
            Assert.That(game.State.CountRegions(RegionResult.Open), Is.EqualTo(9));
            foreach (BoardPosition option in Coordinates())
                Assert.That(GameRules.IsLegal(game.State, option), Is.True);
        }

        [Test]
        public void FullLegalGamesTerminateWithNoResidualLegalCell()
        {
            BoardPosition[] candidates = new List<BoardPosition>(Coordinates()).ToArray();
            var random = new Random(1729);
            bool reachedWin = false;
            bool reachedDraw = false;
            for (int match = 0; match < 40; match++)
            {
                var state = new GameState();
                while (state.Result == MatchResult.InProgress)
                {
                    var legal = Array.FindAll(candidates, move => GameRules.IsLegal(state, move));
                    Assert.That(legal, Is.Not.Empty);
                    Assert.That(GameRules.TryPlay(state, legal[random.Next(legal.Length)]), Is.True);
                    Assert.That(state.MoveCount, Is.LessThanOrEqualTo(81));
                }
                reachedWin |= state.Line.HasValue;
                reachedDraw |= state.Result == MatchResult.Draw;
                Assert.That(Array.Exists(candidates, move => GameRules.IsLegal(state, move)), Is.False);
            }
            Assert.That(reachedWin && reachedDraw, Is.True);
        }

        private static IEnumerable<BoardPosition> Coordinates()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    yield return new BoardPosition(i, j);
        }

        private static void PrepareWin(RegionState region, Mark mark, int a, int b)
        {
            region.Place(a - 1, mark);
            region.Place(b - 1, mark);
        }

        private static void WinRegion(RegionState region, Mark mark)
        {
            PrepareWin(region, mark, 1, 2);
            region.Place(2, mark);
        }

        private static void PrepareDraw(RegionState region, bool complete = true, int skip = -1)
        {
            const string draw = "OXOOXXXOO";
            int limit = complete ? 9 : 8;
            for (int i = 0; i < limit; i++)
                if (i != skip)
                    region.Place(i, draw[i] == 'O' ? Mark.O : Mark.X);
        }

        private static void AssertLine(WinningLine? line, int a, int b, int c)
        {
            Assert.That(line.HasValue, Is.True);
            Assert.That(line.Value.First, Is.EqualTo(a - 1));
            Assert.That(line.Value.Middle, Is.EqualTo(b - 1));
            Assert.That(line.Value.Last, Is.EqualTo(c - 1));
        }
    }
}
