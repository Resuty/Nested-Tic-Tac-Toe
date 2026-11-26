using UnityEngine;
using UnityEngine.UI;
using WellWithinWell.Presentation;
using WellWithinWell.Rules;

using static WellWithinWell.Editor.UiWorkshop;

namespace WellWithinWell.Editor
{
    internal sealed class ScreenWorkshop
    {
        private readonly UiWorkshop ui;
        private readonly TableWorkshop tables;
        private readonly BoardSkin skin;
        private const int Width = 1232;
        private const int Height = 768;

        internal ScreenWorkshop(UiWorkshop workshop, TableWorkshop boardWorkshop, BoardSkin art)
        {
            ui = workshop;
            tables = boardWorkshop;
            skin = art;
        }

        internal void Create(RectTransform root, UIController controller, AudioController audio)
        {
            RectTransform menu = Page(root, "Menu");
            RectTransform mode = Page(root, "Mode");
            RectTransform match = Page(root, "Match");
            RectTransform rules = Page(root, "Rules");
            RectTransform settings = Page(root, "Settings");

            Menu(menu, out var cover, out var start, out var learn, out var preferences, out var quit);
            Button modeBack = Mode(mode, out var local, out var computer);
            MatchPanel hud = Match(match);
            Button rulesBack = Rules(rules);
            Button settingsBack = Settings(settings, out var volume, out var value, out var mute,
                out var languages, out var markers);
            Confirm(root, out var dialog, out var heading, out var detail, out var yes, out var no);
            GameObject order = OrderPrompt(root, out var first, out var second, out var cancel);
            controller.Wire(new[] { menu.gameObject, mode.gameObject, match.gameObject, rules.gameObject, settings.gameObject },
                start, learn, preferences, quit, new[] { modeBack, rulesBack, settingsBack },
                local, computer, order, first, second, cancel, hud, cover,
                audio, volume, value, mute, dialog, heading, detail, yes, no);
            controller.WireLanguage(ui.LocalizedLabels, languages, markers);
            mode.gameObject.SetActive(false);
            match.gameObject.SetActive(false);
            rules.gameObject.SetActive(false);
            settings.gameObject.SetActive(false);
        }

        private static RectTransform Page(RectTransform root, string name) => Node(root, name, 0, 0, Width, Height);

        private void Menu(RectTransform page, out BoardView board, out Button start,
            out Button rules, out Button settings, out Button quit)
        {
            Image(page, "TitleSeal", 58, 191, 45, 45, Accent);
            Outline(page, "SealRim", 60, 193, 41, 41, Brass);
            ui.Label(page, "Seal", "井", 58, 191, 45, 45, 27, Cream, TextAnchor.MiddleCenter);
            ui.Label(page, "GameTitle", TextKey.Title, 53, 235, 471, 136, 107, Cream, englishSize: 46);
            Image(page, "TitleRule", 59, 385, 42, 1, Brass);

            start = ui.Button(page, "Start", TextKey.Play, 58, 429, 358, 64, true).Button;
            rules = ui.Button(page, "RulesButton", TextKey.Rules, 58, 510, 136, 49).Button;
            settings = ui.Button(page, "SettingsButton", TextKey.Settings, 207, 510, 111, 49).Button;
            quit = ui.Button(page, "Quit", TextKey.Quit, 331, 510, 85, 49).Button;

            board = tables.Build(page, "PreviewBoard", 527, 63, 0.962f);
        }

        private Button Mode(RectTransform page, out Button local, out Button computer)
        {
            Button back = Header(page, TextKey.Play);
            RectTransform card = Paper(page, "ModeCard", 233, 248, 766, 272);
            ui.Label(card, "ModeTitle", TextKey.ChooseMode, 54, 47, 658, 66, 44, Ink, TextAnchor.MiddleCenter);
            Image(card, "Divider", 72, 133, 622, 1, new Color32(154, 106, 67, 145));
            local = ui.Button(card, "Local", TextKey.Local, 72, 164, 299, 68).Button;
            computer = ui.Button(card, "Computer", TextKey.Computer, 395, 164, 299, 68).Button;
            return back;
        }

        private GameObject OrderPrompt(RectTransform parent, out Button first, out Button second, out Button cancel)
        {
            RectTransform layer = Page(parent, "OrderChoice");
            var dim = Image(layer, "Scrim", 0, 0, Width, Height, new Color32(24, 16, 11, 198));
            dim.raycastTarget = true;
            RectTransform card = Paper(layer, "Prompt", 313, 245, 606, 278);
            ui.Label(card, "OrderTitle", TextKey.ChooseOrder, 40, 34, 526, 63, 34, Ink, TextAnchor.MiddleCenter);
            first = ui.Button(card, "First", TextKey.First, 49, 119, 242, 60).Button;
            second = ui.Button(card, "Second", TextKey.Second, 315, 119, 242, 60).Button;
            cancel = ui.Button(card, "Cancel", TextKey.Cancel, 215, 201, 176, 48, true).Button;
            layer.gameObject.SetActive(false);
            return layer.gameObject;
        }

        private MatchPanel Match(RectTransform page)
        {
            ui.Label(page, "Title", TextKey.Title, 28, 3, 280, 62, 39, Cream, englishSize: 27);
            var mode = ui.Label(page, "Mode", "", 322, 24, 210, 30, 17, Faded);
            Image(page, "HeaderRule", 28, 75, 1176, 1, new Color32(183, 126, 78, 130));

            var board = tables.Build(page, "Board", 27, 92);
            var side = Paper(page, "MatchCard", 719, 92, 485, 664);
            var header = ui.Label(side, "TurnHeading", "", 29, 22, 293, 28, 18, Accent);
            var hand = ui.Label(side, "TurnNumber", "", 326, 22, 131, 30, 19, SubtleInk, TextAnchor.MiddleRight);
            var piece = Image(side, "TurnPiece", 25, 56, 67, 67, Color.white, skin.Piece(Mark.O));
            var title = ui.Label(side, "TurnLabel", "", 101, 53, 353, 70, 39, Ink);
            Divider(side, 142);

            var targetBox = Node(side, "Target", 26, 168, 433, 146);
            Image(targetBox, "Wash", 0, 0, 433, 146, new Color32(120, 135, 74, 43));
            Outline(targetBox, "RuleBorder", 0, 0, 433, 146, new Color32(106, 112, 64, 149));
            var targetHeading = ui.Label(targetBox, "LegalHeading", "", 15, 9, 400, 25, 16, SubtleInk);
            var target = ui.Label(targetBox, "LegalLabel", "", 15, 31, 400, 43, 31, Ink);
            var reason = ui.Label(targetBox, "Reason", "", 15, 76, 404, 59, 19, Ink);

            ui.Label(side, "RegionHeading", TextKey.Captured, 29, 331, 423, 29, 16, SubtleInk);
            var seats = Node(side, "Seats", 26, 366, 433, 63);
            Score(seats, Mark.O, 0, out var playerO, out var countO);
            Score(seats, Mark.X, 223, out var playerX, out var countX);
            Divider(side, 459);
            ui.Label(side, "PreviousHeading", TextKey.LastHeading, 29, 478, 168, 30, 16, SubtleInk);
            var previous = ui.Label(side, "LastMoveLabel", "", 29, 514, 427, 36, 24, Ink);

            var restart = ui.Button(side, "Restart", "", 26, 583, 210, 52);
            var home = ui.Button(side, "MatchHome", TextKey.Menu, 249, 583, 210, 52, true);
            var panel = page.gameObject.AddComponent<MatchPanel>();
            panel.Wire(board, skin, mode, header, title, piece, targetHeading, target, reason, playerO, playerX,
                countO, countX, hand, previous, restart, home);
            return panel;
        }

        private void Score(RectTransform seats, Mark mark, int x, out Text player, out Text count)
        {
            var card = Node(seats, "Score" + mark, x, 0, 210, 63);
            Image(card, "Wash", 0, 0, 210, 63, new Color32(126, 79, 47, 20));
            Outline(card, "Edge", 0, 0, 210, 63, new Color32(150, 106, 65, 126));
            Image(card, "Piece", 9, 10, 43, 43, Color.white, skin.Piece(mark));
            player = ui.Label(card, "Seat", "", 54, 10, 102, 43, 18, Ink);
            count = ui.Label(card, "Count", "", 157, 7, 40, 48, 30, Ink, TextAnchor.MiddleRight);
        }

        private static void Divider(RectTransform side, int y) =>
            Image(side, "Divider", 28, y, 429, 1, new Color32(161, 113, 73, 144));

        private Button Rules(RectTransform page)
        {
            Button back = Header(page, TextKey.Rules);
            RectTransform card = Paper(page, "RulesCard", 145, 105, 942, 623);
            ui.Label(card, "RuleTitle", TextKey.RulesTitle, 40, 28, 862, 56, 36, Ink);
            ui.Label(card, "RuleIntro", TextKey.RulesIntro, 42, 86, 860, 34, 22, SubtleInk);

            Diagram(card, "LocalExample", 195, 143, false, 7);
            Diagram(card, "TargetExample", 568, 143, true, 7);
            ui.Label(card, "Arrow", "→", 424, 188, 93, 73, 52, Accent, TextAnchor.MiddleCenter);
            ui.Label(card, "SourceCaption", TextKey.ExampleMove, 114, 315, 319, 36, 21, Ink, TextAnchor.MiddleCenter);
            ui.Label(card, "TargetCaption", TextKey.ExampleTarget, 488, 315, 320, 36, 21, Ink, TextAnchor.MiddleCenter);
            Image(card, "RulesDivider", 42, 372, 858, 1, new Color32(155, 107, 70, 172));

            RuleRow(card, 386, TextKey.PlayRule, TextKey.PlayRuleBody);
            RuleRow(card, 451, TextKey.CaptureRule, TextKey.CaptureRuleBody);
            RuleRow(card, 516, TextKey.EndRule, TextKey.EndRuleBody);
            return back;
        }

        private void RuleRow(RectTransform card, int y, TextKey title, TextKey content)
        {
            ui.Label(card, "RuleName", title, 43, y, 111, 50, 25, Accent);
            ui.Label(card, "RuleText", content, 158, y, 740, 55, 20, Ink);
        }

        private void Diagram(RectTransform parent, string name, int x, int y, bool regional, int highlight)
        {
            var grid = Node(parent, name, x, y, 164, 164);
            Image(grid, "Frame", 0, 0, 164, 164, Frame);
            for (int i = 0; i < 9; i++)
            {
                int left = 3 + (i % 3) * 53;
                int top = 3 + (i / 3) * 53;
                var cell = Node(grid, "Example_" + (i + 1), left, top, 52, 52);
                Color color = i == highlight
                    ? regional ? new Color32(158, 162, 97, 255) : new Color32(239, 197, 138, 255)
                    : Color.white;
                Image(cell, "Square", 0, 0, 52, 52, color, ArtWorkshop.Sprite("Maple"));
                if (i == highlight && !regional)
                {
                    Image(cell, "ExamplePiece", 5, 5, 42, 42, Color.white, skin.Piece(Mark.O));
                    ui.Label(cell, "ExampleNum", "8", 1, 1, 14, 18, 11, Ink, TextAnchor.MiddleCenter);
                }
                else if (regional)
                    ui.Label(cell, "Number", TextCatalog.RegionKey(i), 0, 0, 52, 52, 24, Ink, TextAnchor.MiddleCenter);
                else
                    ui.Label(cell, "Number", (i + 1).ToString(), 0, 0, 52, 52, 24, Ink, TextAnchor.MiddleCenter);
            }
        }

        private Button Settings(RectTransform page, out Slider slider, out Text value, out AcceptButton mute,
            out Button[] languages, out Image[] markers)
        {
            Button back = Header(page, TextKey.Settings);
            RectTransform card = Paper(page, "AudioCard", 300, 131, 632, 506);
            ui.Label(card, "AudioTitle", TextKey.Volume, 48, 39, 440, 64, 43, Ink);
            value = ui.Label(card, "AudioValue", "72%", 447, 65, 136, 34, 23, Ink, TextAnchor.MiddleRight);
            var rail = Node(card, "Volume", 51, 133, 526, 43);
            var hit = rail.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            Image(rail, "TrackShadow", 0, 17, 526, 9, new Color32(95, 54, 32, 145));
            Image(rail, "Track", 0, 19, 526, 5, new Color32(210, 160, 107, 255));
            var fillArea = Node(rail, "FillArea", 4, 19, 518, 5);
            var fill = Image(fillArea, "Fill", 0, 0, 518, 5, Accent);
            Fill(fill.rectTransform);
            var thumbArea = Node(rail, "ThumbArea", 14, 9, 498, 25);
            var thumb = Image(thumbArea, "Thumb", 0, 0, 24, 25, Color.white, ArtWorkshop.Sprite("Button"));
            thumb.rectTransform.anchorMin = Vector2.zero;
            thumb.rectTransform.anchorMax = new Vector2(0, 1);
            thumb.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            thumb.rectTransform.anchoredPosition = Vector2.zero;
            thumb.rectTransform.sizeDelta = new Vector2(24, 0);
            Outline(thumb.rectTransform, "Metal", 0, 0, 24, 25, Brass);
            slider = rail.gameObject.AddComponent<Slider>();
            slider.targetGraphic = thumb;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = thumb.rectTransform;
            slider.value = 0.72f;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            mute = ui.Button(card, "Mute", "", 50, 201, 264, 54);
            Image(card, "LanguageDivider", 50, 289, 532, 1, new Color32(161, 113, 73, 144));
            ui.Label(card, "LanguageTitle", TextKey.Language, 48, 310, 530, 55, 34, Ink);

            string[] languageNames = { "简体中文", "日本語", "English" };
            string[] optionNames = { "Chinese", "Japanese", "English" };
            languages = new Button[languageNames.Length];
            markers = new Image[languageNames.Length];
            var choices = Node(card, "LanguageChoices", 50, 384, 532, 56);
            for (int i = 0; i < languageNames.Length; i++)
            {
                var option = ui.Button(choices, optionNames[i], languageNames[i], i * 181, 0, 170, 56);
                languages[i] = option.Button;
                markers[i] = Image((RectTransform)option.transform, "Selected", 14, 48, 142, 3, Brass);
                markers[i].enabled = i == 0;
            }
            return back;
        }

        private Button Header(RectTransform page, TextKey heading)
        {
            ui.Label(page, "PageTitle", heading, 27, 4, 780, 63, 40, Cream);
            Image(page, "HeaderRule", 28, 75, 1176, 1, new Color32(183, 126, 78, 130));
            return ui.Button(page, "Back", TextKey.Menu, 1017, 12, 187, 48).Button;
        }

        private void Confirm(RectTransform parent, out GameObject dialog, out Text title, out Text detail,
            out AcceptButton yes, out Button no)
        {
            RectTransform layer = Page(parent, "Confirmation");
            var dim = Image(layer, "Scrim", 0, 0, Width, Height, new Color32(24, 16, 11, 198));
            dim.raycastTarget = true;
            RectTransform card = Paper(layer, "Prompt", 313, 235, 606, 295);
            title = ui.Label(card, "ConfirmTitle", "", 40, 33, 526, 69, 41, Ink, TextAnchor.MiddleCenter);
            detail = ui.Label(card, "ConfirmDetail", "", 40, 113, 526, 54, 23, SubtleInk, TextAnchor.MiddleCenter);
            no = ui.Button(card, "Cancel", TextKey.Cancel, 49, 196, 236, 55, true).Button;
            yes = ui.Button(card, "Confirm", "", 321, 196, 236, 55);
            dialog = layer.gameObject;
            dialog.SetActive(false);
        }
    }
}
