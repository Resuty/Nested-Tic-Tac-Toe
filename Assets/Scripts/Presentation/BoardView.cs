using System;
using UnityEngine;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private BoardSkin skin;
        [SerializeField] private RegionView[] regions;
        [SerializeField] private LineView winningLine;

        public void LocalizeRegions(GameLanguage language)
        {
            for (int i = 0; i < regions.Length; i++)
                regions[i].SetAddress(TextCatalog.Get(TextCatalog.RegionKey(i), language));
        }

        public void Bind(Action<BoardPosition> play, Action<BoardPosition?> hover)
        {
            for (int i = 0; i < regions.Length; i++)
                regions[i].Bind(i, skin, play, hover);
        }

        public void Present(GameState game, bool animate, bool acceptsInput = true)
        {
            foreach (RegionView region in regions)
                region.Present(game, animate, acceptsInput);
            if (game.Line.HasValue)
            {
                WinningLine line = game.Line.Value;
                Vector2 a = Center((RectTransform)regions[line.First].transform);
                Vector2 b = Center((RectTransform)regions[line.Last].transform);
                Vector2 padding = (b - a).normalized * 54;
                winningLine.Show(a - padding, b + padding,
                    game.Result == MatchResult.WonByO ? BoardSkin.LineO : BoardSkin.LineX, 11, animate);
            }
            else
                winningLine.Hide();
        }

        public static Vector2 Center(RectTransform upperLeftRect)
        {
            return upperLeftRect.anchoredPosition
                + Vector2.Scale(upperLeftRect.sizeDelta, new Vector2(0.5f, -0.5f));
        }

        public void Wire(BoardSkin art, RegionView[] squares, LineView line)
        {
            skin = art;
            regions = squares;
            winningLine = line;
        }
    }
}
