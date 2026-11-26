using UnityEngine;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    [CreateAssetMenu(menuName = "井中井/棋盘材质")]
    public sealed class BoardSkin : ScriptableObject
    {
        [SerializeField] private Sprite circle;
        [SerializeField] private Sprite cross;
        [SerializeField] private Sprite draw;
        [SerializeField] private Sprite lastMove;

        public Sprite Piece(Mark mark) => mark == Mark.O ? circle : cross;
        public Sprite Draw => draw;
        public Sprite LastMove => lastMove;

        public static readonly Color Legal = new Color32(143, 155, 111, 143);
        public static readonly Color Focus = new Color32(136, 149, 91, 209);
        public static readonly Color Inactive = new Color32(83, 62, 40, 29);
        public static readonly Color OwnedO = new Color32(143, 72, 43, 80);
        public static readonly Color OwnedX = new Color32(57, 78, 75, 80);
        public static readonly Color Drawn = new Color32(101, 103, 94, 125);
        public static readonly Color LineO = new Color32(123, 44, 26, 244);
        public static readonly Color LineX = new Color32(28, 63, 65, 250);
    }
}
