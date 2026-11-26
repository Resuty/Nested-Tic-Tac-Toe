using NUnit.Framework;
using WellWithinWell.Rules;

namespace WellWithinWell.Tests
{
    public sealed class ComputerPlayerTests
    {
        [Test]
        public void SearchRespectsTheForcedRegionAndLeavesTheLivePositionIntact()
        {
            var state = new GameState();
            var first = new BoardPosition(1, 7);
            Assert.That(GameRules.TryPlay(state, first), Is.True);
            BoardPosition? reply = ComputerPlayer.ChooseMove(state);
            Assert.That(reply.HasValue, Is.True);
            Assert.That(reply.Value.Region, Is.EqualTo(7));
            Assert.That(GameRules.IsLegal(state, reply.Value), Is.True);
            Assert.That(state.Turn, Is.EqualTo(Mark.X));
            Assert.That(state.LastMove, Is.EqualTo(first));
            Assert.That(state.MoveCount, Is.EqualTo(1));
            for (int i = 0; i < BoardState.Size; i++)
                for (int j = 0; j < BoardState.Size; j++)
                    Assert.That(state.Board[i][j], Is.EqualTo(i == 1 && j == 7 ? Mark.O : Mark.Empty));
        }

        [Test]
        public void ComputerTakesAnImmediateGlobalWin()
        {
            var board = PrepareTwoRegionsAndAThreat(Mark.O);
            var state = new GameState(board, Mark.O) { RequiredRegion = 2 };
            Assert.That(ComputerPlayer.ChooseMove(state), Is.EqualTo(new BoardPosition(2, 2)));
        }

        [Test]
        public void ComputerAvoidsSendingAnOpponentToAnImmediateGlobalWin()
        {
            var board = PrepareTwoRegionsAndAThreat(Mark.X);
            var state = new GameState(board, Mark.O) { RequiredRegion = 4 };
            BoardPosition? move = ComputerPlayer.ChooseMove(state);
            Assert.That(move.HasValue, Is.True);
            Assert.That(move.Value.Region, Is.EqualTo(4));
            Assert.That(move.Value.Cell, Is.Not.EqualTo(2));
        }

        [Test]
        public void BothSeatsCanPlayALegalGameFromStartToFinish()
        {
            var state = new GameState();
            while (state.Result == MatchResult.InProgress)
            {
                BoardPosition? move = ComputerPlayer.ChooseMove(state);
                Assert.That(move.HasValue, Is.True);
                Assert.That(GameRules.TryPlay(state, move.Value), Is.True);
                Assert.That(state.MoveCount, Is.LessThanOrEqualTo(81));
            }
            Assert.That(ComputerPlayer.ChooseMove(state), Is.Null);
        }

        private static BoardState PrepareTwoRegionsAndAThreat(Mark mark)
        {
            var board = new BoardState();
            for (int i = 0; i < 3; i++)
            {
                board[i].Place(0, mark);
                board[i].Place(1, mark);
                if (i < 2)
                    board[i].Place(2, mark);
            }
            return board;
        }
    }
}
