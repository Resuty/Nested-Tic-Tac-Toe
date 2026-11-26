using System;

namespace WellWithinWell.Rules
{
    public sealed class BoardState
    {
        public const int Size = 9;

        private readonly RegionState[] regions;

        public BoardState()
        {
            regions = new RegionState[Size];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new RegionState();
        }

        public RegionState this[int region] => regions[region];

        private BoardState(RegionState[] regions) => this.regions = regions;

        internal BoardState Copy() => new BoardState(Array.ConvertAll(regions, region => region.Copy()));
    }

    public sealed class RegionState
    {
        private readonly Mark[] cells = new Mark[BoardState.Size];

        public Mark this[int cell] => cells[cell];
        public int Filled { get; private set; }
        public RegionResult Result { get; private set; }
        public WinningLine? Line { get; private set; }

        internal RegionState() { }

        internal RegionState Copy()
        {
            var copy = new RegionState { Filled = Filled, Result = Result, Line = Line };
            Array.Copy(cells, copy.cells, cells.Length);
            return copy;
        }

        // The caller validates the move before entering this method.
        internal void Place(int cell, Mark mark)
        {
            cells[cell] = mark;
            Filled++;
            Line = GameRules.FindLine(index => cells[index] == mark);
            if (Line.HasValue)
                Result = mark == Mark.O ? RegionResult.WonByO : RegionResult.WonByX;
            else if (Filled == BoardState.Size)
                Result = RegionResult.Draw;
        }
    }
}
