using System;
using UnityEngine;
using UnityEngine.UI;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    public sealed class RegionView : MonoBehaviour
    {
        [SerializeField] private CellView[] cells;
        [SerializeField] private Image tint;
        [SerializeField] private Image outline;
        [SerializeField] private Image seal;
        [SerializeField] private Text address;
        [SerializeField] private LineView winLine;
        private BoardSkin skin;
        private int index;
        private RegionResult previousResult;

        public void Bind(int region, BoardSkin boardSkin, Action<BoardPosition> play, Action<BoardPosition?> hover)
        {
            index = region;
            skin = boardSkin;
            for (int i = 0; i < cells.Length; i++)
                cells[i].Bind(new BoardPosition(index, i), skin, play, hover);
        }

        public void SetAddress(string value) => address.text = value;

        public void Present(GameState game, bool animate, bool acceptsInput)
        {
            var region = game.Board[index];
            bool legal = GameRules.IsRegionLegal(game, index);
            bool focused = legal && game.RequiredRegion.HasValue;
            bool closed = region.Result != RegionResult.Open;
            tint.color = region.Result switch
            {
                RegionResult.WonByO => BoardSkin.OwnedO,
                RegionResult.WonByX => BoardSkin.OwnedX,
                RegionResult.Draw => BoardSkin.Drawn,
                _ => focused ? BoardSkin.Focus : legal ? BoardSkin.Legal : BoardSkin.Inactive
            };
            outline.enabled = focused;
            seal.enabled = closed;
            if (closed)
            {
                seal.sprite = region.Result == RegionResult.Draw
                    ? skin.Draw
                    : skin.Piece(region.Result == RegionResult.WonByO ? Mark.O : Mark.X);
                seal.color = new Color(1, 1, 1, region.Result == RegionResult.Draw ? 0.63f : 0.39f);
            }

            BoardPosition? last = game.LastMove;
            for (int i = 0; i < cells.Length; i++)
            {
                bool recent = last.HasValue && last.Value.Region == index && last.Value.Cell == i;
                cells[i].Present(region[i], game.Turn, acceptsInput && legal && region[i] == Mark.Empty,
                    recent, recent && animate, closed);
            }

            if (!region.Line.HasValue)
                winLine.Hide();
            else if (region.Result != previousResult || !animate)
            {
                WinningLine line = region.Line.Value;
                Vector2 a = BoardView.Center((RectTransform)cells[line.First].transform);
                Vector2 b = BoardView.Center((RectTransform)cells[line.Last].transform);
                Vector2 padding = (b - a).normalized * 12;
                winLine.Show(a - padding, b + padding,
                    region.Result == RegionResult.WonByO ? BoardSkin.LineO : BoardSkin.LineX, 4, animate);
            }
            previousResult = region.Result;
        }

        public void Wire(CellView[] squares, Image wash, Image border, Image token, Text number, LineView line)
        {
            cells = squares;
            tint = wash;
            outline = border;
            seal = token;
            address = number;
            winLine = line;
        }
    }
}
