using System.Collections.Generic;
using System.Globalization;

namespace WellWithinWell.Presentation
{
    public enum GameLanguage { SimplifiedChinese, Japanese, English }

    public enum TextKey
    {
        Title, Play, Rules, Settings, Quit, Menu, Back, Cancel,
        ChooseMode, Local, Computer, ChooseOrder, First, Second,
        CurrentTurn, Result, MoveNumber, MoveTotal, PlayerTurn, Thinking, YourTurn, Win, Draw,
        PlayerOne, PlayerTwo, You, Cpu, LegalHeading, Captured, LastHeading, LastMove, NoMove,
        AllClosed, ThreeBoards, Board, FreeMove, FinalBoard, ComputerReason, DrawReason, WinReason,
        OpenHover, ClosedHover, FirstReason, ForcedReason, FreeReason,
        RulesTitle, RulesIntro, ExampleMove, ExampleTarget, PlayRule, PlayRuleBody,
        CaptureRule, CaptureRuleBody, EndRule, EndRuleBody,
        Volume, Mute, Unmute, Language, Restart, PlayAgain, EndMatch, QuitTitle,
        Region1, Region2, Region3, Region4, Region5, Region6, Region7, Region8, Region9
    }

    public static class TextCatalog
    {
        private static readonly Dictionary<TextKey, (string Chinese, string Japanese, string English)> texts = new()
        {
            [TextKey.Title] = ("井中井", "井中井", "Nested Tic-Tac-Toe"),
            [TextKey.Play] = ("开始", "対局する", "Play"),
            [TextKey.Rules] = ("怎么玩", "遊び方", "How to play"),
            [TextKey.Settings] = ("设置", "設定", "Settings"),
            [TextKey.Quit] = ("退出", "終了", "Quit"),
            [TextKey.Menu] = ("回主菜单", "メニューへ", "Main menu"),
            [TextKey.Back] = ("返回", "戻る", "Go back"),
            [TextKey.Cancel] = ("取消", "キャンセル", "Cancel"),
            [TextKey.ChooseMode] = ("选择对手", "対局相手を選ぶ", "Choose an opponent"),
            [TextKey.Local] = ("本地双人", "ふたりで対局", "Two players"),
            [TextKey.Computer] = ("和电脑下", "CPUと対局", "Vs. CPU"),
            [TextKey.ChooseOrder] = ("你先下，还是电脑先下？", "先に打ちますか？", "Who goes first?"),
            [TextKey.First] = ("我先下（O）", "先手（O）", "Go first (O)"),
            [TextKey.Second] = ("我后下（X）", "後手（X）", "Go second (X)"),
            [TextKey.CurrentTurn] = ("对局中", "対局中", "Match in progress"),
            [TextKey.Result] = ("对局结束", "対局終了", "Match over"),
            [TextKey.MoveNumber] = ("第{0}手", "第{0}手", "Move {0}"),
            [TextKey.MoveTotal] = ("共{0}手", "全{0}手", "{0} moves"),
            [TextKey.PlayerTurn] = ("轮到{0}", "{0}の番", "{0}'s turn"),
            [TextKey.Thinking] = ("电脑回合", "CPUの番", "CPU's turn"),
            [TextKey.YourTurn] = ("轮到你了", "あなたの番", "Your turn"),
            [TextKey.Win] = ("{0}赢了", "{0}の勝ち", "{0} wins"),
            [TextKey.Draw] = ("和棋", "引き分け", "Draw"),
            [TextKey.PlayerOne] = ("玩家一", "プレイヤー1", "Player 1"),
            [TextKey.PlayerTwo] = ("玩家二", "プレイヤー2", "Player 2"),
            [TextKey.You] = ("你", "あなた", "You"),
            [TextKey.Cpu] = ("电脑", "CPU", "CPU"),
            [TextKey.LegalHeading] = ("落子范围", "打てる場所", "Where to play"),
            [TextKey.Captured] = ("已拿下的大区", "取った盤", "Boards won"),
            [TextKey.LastHeading] = ("上一手", "前の手", "Last move"),
            [TextKey.LastMove] = ("{0}号大区 · {1}号格", "{0}番の盤・マス{1}", "Board {0}, cell {1}"),
            [TextKey.NoMove] = ("还没落子", "まだ打っていません", "No moves yet"),
            [TextKey.AllClosed] = ("九个大区都已封盘", "九つの盤がすべて終了", "All nine boards closed"),
            [TextKey.ThreeBoards] = ("大区三连", "盤が三つ並びました", "Three in a row"),
            [TextKey.Board] = ("{0}号大区", "{0}番の盤", "Board {0}"),
            [TextKey.FreeMove] = ("大区任选", "盤を選べます", "Pick any open board"),
            [TextKey.FinalBoard] = ("盘面", "盤面", "Final board"),
            [TextKey.ComputerReason] = ("等电脑下完这一手。", "CPUが打ち終わるまでお待ちください。", "Wait for the CPU to move."),
            [TextKey.DrawReason] = ("两边都没拿下连成一线的三个大区。", "どちらも取った盤を三つ並べられませんでした。", "Neither player won three boards in a line."),
            [TextKey.WinReason] = ("{0}、{1}、{2}号大区连成一线。", "{0}・{1}・{2}番の盤が一列に並びました。", "Boards {0}, {1} and {2} made a line."),
            [TextKey.OpenHover] = ("下在{0}号格，对手就去{1}号大区。\n那个大区封盘了，对手才能另选。", "マス{0}に打つと、相手は{1}番の盤へ。\nその盤が終われば、別の盤を選べます。", "Cell {0} sends your opponent to board {1}.\nIf it closes, they can choose another."),
            [TextKey.ClosedHover] = ("{0}号格对应的{1}号大区已经封盘。\n对手可以另选一个没封盘的大区。", "マス{0}の行き先、{1}番の盤は終了済み。\n相手はまだ打てる盤を選べます。", "Cell {0} points to closed board {1}.\nYour opponent can pick any open board."),
            [TextKey.FirstReason] = ("第一手可以下在任意空格。\n你下在几号格，对手就去几号大区。", "初手はどの空きマスにも打てます。\nマスの番号が相手の行き先です。", "Start in any empty cell. Its number tells your opponent where to play."),
            [TextKey.ForcedReason] = ("上一手下在{0}号格，所以这手得下在这里。", "前の手がマス{0}だったので、この盤に打ちます。", "The last move was in cell {0}, so play here."),
            [TextKey.FreeReason] = ("{0}号大区已经封盘了，这手可以另选大区。", "{0}番の盤は終了済みです。まだ打てる盤を選んでください。", "Board {0} is closed. Pick any open board."),
            [TextKey.RulesTitle] = ("下一手下在哪？", "次はどこに打つ？", "Where do I play next?"),
            [TextKey.RulesIntro] = ("大区和小格都从左上角开始，先横着数，再数下一行。", "盤もマスも、左上から一段ずつ、左から右へ数えます。", "Number the boards and cells from the top left, one row at a time."),
            [TextKey.ExampleMove] = ("O 下在二号大区的8号格", "Oが二番の盤のマス8に打つ", "O plays cell 8 on board 2"),
            [TextKey.ExampleTarget] = ("X 下一手去八号大区", "次にXは八番の盤に打つ", "X plays next on board 8"),
            [TextKey.PlayRule] = ("落子", "打ち方", "Play"),
            [TextKey.PlayRuleBody] = ("O 先下，第一手任选空格。\n之后按上一手的格号去对应大区。那区封盘了，就换个还能下的。", "Oが先手です。最初はどの空きマスでも打てます。次からは前の手のマスと同じ番号の盤へ。その盤が終わっていたら、まだ打てる盤を選べます。", "O starts in any empty cell. After that, play on the board matching the last cell number. If it's closed, pick any open board."),
            [TextKey.CaptureRule] = ("拿下大区", "盤を取る", "Boards"),
            [TextKey.CaptureRuleBody] = ("横、竖、斜连成三个，就拿下这个大区。\n下满还没三连，这区就算和棋。两种情况都封盘。", "縦・横・斜めに三つ並べると、その盤を取れます。埋まってもそろわなければ引き分けです。どちらの場合も、その盤にはもう打てません。", "Three in a row wins the board. If it fills up with no winner, it's a draw. Either way, no more moves there."),
            [TextKey.EndRule] = ("整局胜负", "勝ち方", "Winning"),
            [TextKey.EndRuleBody] = ("拿下的大区横、竖或斜连成三个，就赢了。\n九个大区都封盘还没人连成线，这局就和了。", "取った盤が縦・横・斜めに三つ並べば勝ちです。九つの盤が終わっても、どちらもそろわなければ引き分けです。", "Take three boards in a line to win the match. If all nine boards close and nobody has that line, it's a draw."),
            [TextKey.Volume] = ("音量", "音量", "Volume"),
            [TextKey.Mute] = ("静音", "ミュート", "Mute"),
            [TextKey.Unmute] = ("解除静音", "ミュート解除", "Unmute"),
            [TextKey.Language] = ("语言", "言語", "Language"),
            [TextKey.Restart] = ("重新开始", "やり直す", "Restart"),
            [TextKey.PlayAgain] = ("再来一局", "もう一局", "Play again"),
            [TextKey.EndMatch] = ("这局还没下完，要放弃吗？", "この対局を途中で終えますか？", "End this match early?"),
            [TextKey.QuitTitle] = ("要退出游戏吗？", "ゲームを終了しますか？", "Quit the game?"),
            [TextKey.Region1] = ("一", "一", "1"),
            [TextKey.Region2] = ("二", "二", "2"),
            [TextKey.Region3] = ("三", "三", "3"),
            [TextKey.Region4] = ("四", "四", "4"),
            [TextKey.Region5] = ("五", "五", "5"),
            [TextKey.Region6] = ("六", "六", "6"),
            [TextKey.Region7] = ("七", "七", "7"),
            [TextKey.Region8] = ("八", "八", "8"),
            [TextKey.Region9] = ("九", "九", "9")
        };

        public static string Get(TextKey key, GameLanguage language, params object[] arguments)
        {
            var row = texts[key];
            string value = language switch
            {
                GameLanguage.Japanese => row.Japanese,
                GameLanguage.English => row.English,
                _ => row.Chinese
            };
            return arguments.Length == 0 ? value : string.Format(CultureInfo.InvariantCulture, value, arguments);
        }

        public static TextKey RegionKey(int index) => TextKey.Region1 + index;
    }
}
