using System;

namespace WellWithinWell.Rules
{
    public enum Mark { Empty, O, X }
    public enum RegionResult { Open, WonByO, WonByX, Draw }
    public enum MatchResult { InProgress, WonByO, WonByX, Draw }
    public enum Seat { One, Two }

    public readonly struct BoardPosition : IEquatable<BoardPosition>
    {
        public int Region { get; }
        public int Cell { get; }

        public BoardPosition(int region, int cell)
        {
            if ((uint)region >= BoardState.Size)
                throw new ArgumentOutOfRangeException(nameof(region));
            if ((uint)cell >= BoardState.Size)
                throw new ArgumentOutOfRangeException(nameof(cell));
            Region = region;
            Cell = cell;
        }

        public bool Equals(BoardPosition other) => Region == other.Region && Cell == other.Cell;
        public override bool Equals(object obj) => obj is BoardPosition other && Equals(other);
        public override int GetHashCode() => Region * BoardState.Size + Cell;
    }

    public readonly struct WinningLine
    {
        public int First { get; }
        public int Middle { get; }
        public int Last { get; }

        internal WinningLine(int first, int middle, int last)
        {
            First = first;
            Middle = middle;
            Last = last;
        }
    }
}
