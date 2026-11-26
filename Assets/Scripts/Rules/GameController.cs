using System;

namespace WellWithinWell.Rules
{
    public sealed class GameController
    {
        public GameState State { get; private set; } = new GameState();
        public Seat FirstSeat { get; private set; }

        public event Action<GameState> StateChanged;

        public void StartMatch(Seat firstSeat)
        {
            FirstSeat = firstSeat;
            State = new GameState();
            StateChanged?.Invoke(State);
        }

        public Seat SeatFor(Mark mark)
        {
            if (mark == Mark.O)
                return FirstSeat;
            if (mark == Mark.X)
                return FirstSeat == Seat.One ? Seat.Two : Seat.One;
            throw new ArgumentOutOfRangeException(nameof(mark));
        }

        public bool TryPlay(BoardPosition position)
        {
            if (!GameRules.TryPlay(State, position))
                return false;

            StateChanged?.Invoke(State);
            return true;
        }
    }
}
