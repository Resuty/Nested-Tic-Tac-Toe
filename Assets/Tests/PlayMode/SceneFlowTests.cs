using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace WellWithinWell.Tests
{
    public sealed class SceneFlowTests
    {
        private const string Win =
            "11 16 64 45 52 25 58 85 55 75 17 71 14 41 74 47 73 31 95 39 92 27 76 68 81 87 79 91 65 69 98";
        private const string Draw =
            "39 93 37 73 34 47 76 65 51 17 79 99 97 72 26 61 11 18 85 59 91 19 92 22 29 98 87 "
            + "71 55 57 31 25 53 49 95 58 81 46 63 67 89 94 45 69 96 21 48 24 43 27 42";

        private Transform table;
        private Transform board;
        private const string LanguageKey = "WellWithinWell.Language";
        private const string LanguageButtons = "Settings/AudioCard/LanguageChoices/";
        private bool hadLanguage;
        private int originalLanguage;

        [UnitySetUp]
        public IEnumerator OpenScene()
        {
            hadLanguage = PlayerPrefs.HasKey(LanguageKey);
            originalLanguage = PlayerPrefs.GetInt(LanguageKey);
            PlayerPrefs.DeleteKey(LanguageKey);
            yield return LoadScene();
        }

        [TearDown]
        public void RestoreLanguage()
        {
            if (hadLanguage)
                PlayerPrefs.SetInt(LanguageKey, originalLanguage);
            else
                PlayerPrefs.DeleteKey(LanguageKey);
            PlayerPrefs.Save();
        }

        private IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("WellWithinWell");
            // Newly loaded graphics become raycastable after their first canvas render.
            yield return null;
            table = GameObject.Find("TableLayout").transform;
            board = table.Find("Match/Board/Surface");
            Assert.That(EventSystem.current, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator JapaneseUpdatesRulesPromptsAndLiveMatchText()
        {
            yield return ClickAndRender(table.Find("Menu/SettingsButton"));
            Assert.That(Label("Settings/AudioCard/LanguageTitle"), Is.EqualTo("语言"));
            yield return ClickAndRender(table.Find(LanguageButtons + "Japanese"));
            Assert.That(Label("Settings/AudioCard/LanguageTitle"), Is.EqualTo("言語"));
            Assert.That(table.Find(LanguageButtons + "Japanese/Selected").GetComponent<Image>().enabled, Is.True);
            AssertFontCoversVisibleText("Settings");

            yield return ClickAndRender(table.Find("Settings/Back"));
            Assert.That(Label("Menu/Start/Caption"), Is.EqualTo("対局する"));
            yield return ClickAndRender(table.Find("Menu/RulesButton"));
            Assert.That(Label("Rules/RulesCard/TargetCaption"), Is.EqualTo("次にXは八番の盤に打つ"));
            AssertFontCoversVisibleText("Rules");
            AssertRulesFit();
            yield return ClickAndRender(table.Find("Rules/Back"));
            yield return ClickAndRender(table.Find("Menu/Quit"));
            Assert.That(Label("Confirmation/Prompt/ConfirmTitle"), Is.EqualTo("ゲームを終了しますか？"));
            Assert.That(Label("Confirmation/Prompt/ConfirmDetail"), Is.Empty);
            yield return ClickAndRender(table.Find("Confirmation/Prompt/Cancel"));

            yield return ClickAndRender(table.Find("Menu/Start"));
            yield return ClickAndRender(table.Find("Mode/ModeCard/Computer"));
            Assert.That(Label("OrderChoice/Prompt/OrderTitle"), Is.EqualTo("先に打ちますか？"));
            AssertLineCount("OrderChoice/Prompt/OrderTitle", 1);
            yield return ClickAndRender(table.Find("OrderChoice/Prompt/First"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("あなたの番"));
            Hover(Cell("28"));
            Assert.That(MatchLabel("Target/Reason"), Does.Contain("八番の盤"));
            AssertLineCount("Match/MatchCard/Target/Reason", 2);
            AssertFontCoversVisibleText("Match");
            Click(Cell("28"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("CPUの番"));
            Assert.That(MatchLabel("Target/LegalLabel"), Is.EqualTo("八番の盤"));
            yield return WaitForTurn("第3手");
        }

        [UnityTest]
        public IEnumerator EnglishSurvivesSceneReloadAndLocalizesTheResult()
        {
            yield return ClickAndRender(table.Find("Menu/SettingsButton"));
            yield return ClickAndRender(table.Find(LanguageButtons + "English"));
            Assert.That(Label("Settings/PageTitle"), Is.EqualTo("Settings"));
            Assert.That(Label("Settings/AudioCard/LanguageTitle"), Is.EqualTo("Language"));
            yield return LoadScene();
            Assert.That(Label("Menu/GameTitle"), Is.EqualTo("Nested Tic-Tac-Toe"));
            AssertLineCount("Menu/GameTitle", 1);
            AssertLineCount("Menu/SettingsButton/Caption", 1);
            AssertLineCount("Menu/RulesButton/Caption", 1);
            Assert.That(Label("Menu/PreviewBoard/Surface/Region_8/RegionAddress"), Is.EqualTo("8"));

            yield return ClickAndRender(table.Find("Menu/RulesButton"));
            Assert.That(Label("Rules/RulesCard/TargetCaption"), Is.EqualTo("X plays next on board 8"));
            Assert.That(Label("Rules/RulesCard/TargetExample/Example_8/Number"), Is.EqualTo("8"));
            AssertLineCount("Rules/RulesCard/RuleTitle", 1);
            AssertRulesFit();
            yield return ClickAndRender(table.Find("Rules/Back"));
            yield return StartMatch();
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("Player 1's turn"));
            Assert.That(Label("Match/Title"), Is.EqualTo("Nested Tic-Tac-Toe"));
            AssertLineCount("Match/Title", 1);
            Hover(Cell("11"));
            Assert.That(MatchLabel("Target/Reason"), Does.Contain("board 1"));
            AssertLineCount("Match/MatchCard/Target/Reason", 2);
            foreach (string move in Win.Split(' '))
                Click(Cell(move));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("Player 1 wins"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("31 moves"));
            Assert.That(MatchLabel("Restart/Caption"), Is.EqualTo("Play again"));
            Assert.That(MatchLabel("Target/LegalHeading"), Is.EqualTo("Final board"));
            Assert.That(MatchLabel("Target/Reason"), Is.EqualTo("Boards 1, 5 and 9 made a line."));
            AssertLineCount("Match/MatchCard/Target/LegalLabel", 1);
            AssertFontCoversVisibleText("Match");
            yield return ClickAndRender(table.Find("Match/MatchCard/MatchHome"));
            yield return ClickAndRender(table.Find("Menu/SettingsButton"));
            yield return ClickAndRender(table.Find(LanguageButtons + "Chinese"));
            Assert.That(Label("Settings/AudioCard/LanguageTitle"), Is.EqualTo("语言"));
            Assert.That(Label("Menu/PreviewBoard/Surface/Region_8/RegionAddress"), Is.EqualTo("八"));
        }

        [UnityTest]
        public IEnumerator LocalMatchStartsAndRestartsWithPlayerOne()
        {
            yield return StartMatch();
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到玩家一"));

            Click(Cell("11"));
            yield return ClickAndRender(table.Find("Match/MatchCard/Restart"));
            Assert.That(table.Find("Confirmation").gameObject.activeInHierarchy, Is.True);
            yield return ClickAndRender(table.Find("Confirmation/Prompt/Cancel"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第2手"));
            yield return ClickAndRender(table.Find("Match/MatchCard/Restart"));
            yield return ClickAndRender(table.Find("Confirmation/Prompt/Confirm"));
            Assert.That(table.Find("OrderChoice").gameObject.activeInHierarchy, Is.False);
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到玩家一"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第1手"));
            Assert.That(Cell("11").Find("Token").GetComponent<Image>().enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator ComputerMatchOffersBothOrdersAndPlaysOnlyItsOwnTurn()
        {
            yield return ClickAndRender(table.Find("Menu/Start"));
            yield return ClickAndRender(table.Find("Mode/ModeCard/Computer"));
            Assert.That(Label("OrderChoice/Prompt/OrderTitle"), Is.EqualTo("你先下，还是电脑先下？"));
            yield return ClickAndRender(table.Find("OrderChoice/Prompt/Cancel"));
            Assert.That(table.Find("OrderChoice").gameObject.activeInHierarchy, Is.False);
            yield return ClickAndRender(table.Find("Mode/ModeCard/Computer"));
            yield return ClickAndRender(table.Find("OrderChoice/Prompt/First"));
            Assert.That(Label("Match/Mode"), Is.EqualTo("和电脑下"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到你了"));
            Assert.That(MatchLabel("Seats/ScoreO/Seat"), Is.EqualTo("你"));
            Assert.That(MatchLabel("Seats/ScoreX/Seat"), Is.EqualTo("电脑"));

            Click(Cell("28"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("电脑回合"));
            Click(Cell("83"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第2手"));
            yield return WaitForTurn("第3手");
            Assert.That(MatchLabel("LastMoveLabel"), Does.Contain("八号大区"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到你了"));

            yield return ClickAndRender(table.Find("Match/MatchCard/Restart"));
            yield return ClickAndRender(table.Find("Confirmation/Prompt/Confirm"));
            Assert.That(table.Find("OrderChoice").gameObject.activeInHierarchy, Is.True);
            Click(table.Find("OrderChoice/Prompt/Second"));
            Assert.That(MatchLabel("Seats/ScoreO/Seat"), Is.EqualTo("电脑"));
            Assert.That(MatchLabel("Seats/ScoreX/Seat"), Is.EqualTo("你"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("电脑回合"));
            Assert.That(Cell("28").Find("Token").GetComponent<Image>().enabled, Is.False);
            Click(Cell("11"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第1手"));
            yield return WaitForTurn("第2手");
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到你了"));
        }

        [UnityTest]
        public IEnumerator LegalClicksAnimateAndFinishWithTheWinningBoardStillVisible()
        {
            yield return StartMatch();
            for (int region = 1; region <= 9; region++)
                for (int cell = 1; cell <= 9; cell++)
                    Hit(Cell($"{region}{cell}"), out _);

            Transform first = Cell("11");
            var token = first.Find("Token").GetComponent<Image>();
            GameObject target = Hit(first, out var pointer);
            ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerEnterHandler);
            Assert.That(token.enabled, Is.True);
            Assert.That(token.color.a, Is.InRange(0.01f, 0.99f));
            Click(first);
            Assert.That(token.color.a, Is.EqualTo(1f));
            Assert.That(token.rectTransform.localScale.x, Is.GreaterThan(1f));
            Assert.That(first.Find("LastMoveMarker").GetComponent<Image>().enabled, Is.True);
            Assert.That(board.Find("Region_1/LegalBorder").GetComponent<Image>().enabled, Is.True);
            Assert.That(MatchLabel("Target/LegalLabel"), Is.EqualTo("一号大区"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到玩家二"));

            Click(Cell("21"));
            Click(first);
            Click(Cell("16"), PointerEventData.InputButton.Right);
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第2手"));
            yield return new WaitUntil(() => token.rectTransform.localScale == Vector3.one,
                System.TimeSpan.FromSeconds(2), () => Assert.Fail("Piece animation did not settle."));

            Replay(Win, 1);
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("玩家一赢了"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("共31手"));
            Assert.That(MatchLabel("Seats/ScoreO/Count"), Is.EqualTo("4"));
            Assert.That(Cell("98").Find("LastMoveMarker").GetComponent<Image>().enabled, Is.True);
            Assert.That(board.Find("Region_9/OwnershipSeal").GetComponent<Image>().enabled, Is.True);
            Assert.That(board.Find("Region_9/LocalWin/LineInk").GetComponent<Image>().enabled, Is.True);
            Transform line = board.Find("GlobalWin");
            Assert.That(line.Find("LineInk").GetComponent<Image>().enabled, Is.True);
            Assert.That(line.localScale.x, Is.LessThan(1f));
            yield return new WaitUntil(() => line.localScale == Vector3.one,
                System.TimeSpan.FromSeconds(2), () => Assert.Fail("Winning line animation did not settle."));
            Click(Cell("21"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("共31手"));
            Assert.That(Cell("21").Find("Token").GetComponent<Image>().enabled, Is.False);
            Assert.That(MatchLabel("Restart/Caption"), Is.EqualTo("再来一局"));
            Click(table.Find("Match/MatchCard/MatchHome"));
            Assert.That(table.Find("Menu").gameObject.activeInHierarchy, Is.True);
        }

        [UnityTest]
        public IEnumerator DrawCountsAllNineClosedRegionsDespiteUnequalOwnership()
        {
            yield return StartMatch();
            Replay(Draw);
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("和棋"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("共51手"));
            Assert.That(MatchLabel("Seats/ScoreO/Count"), Is.EqualTo("3"));
            Assert.That(MatchLabel("Seats/ScoreX/Count"), Is.EqualTo("5"));
            Assert.That(board.Find("GlobalWin/LineInk").GetComponent<Image>().enabled, Is.False);
            var seal = board.Find("Region_9/OwnershipSeal").GetComponent<Image>();
            Assert.That(seal.enabled && seal.sprite.name == "Draw", Is.True);
            Click(table.Find("Match/MatchCard/Restart"));
            Assert.That(MatchLabel("TurnLabel"), Is.EqualTo("轮到玩家一"));
            Assert.That(MatchLabel("TurnNumber"), Is.EqualTo("第1手"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RulesAndAudioSettingsAreReachableByPointer()
        {
            AudioSource source = GameObject.Find("Table Audio").GetComponent<AudioSource>();
            Click(table.Find("Menu/RulesButton"));
            Assert.That(source.isPlaying, Is.True, "The button should start its one-shot clip.");
            yield return null;
            Assert.That(Label("Rules/RulesCard/TargetCaption"), Does.Contain("八号大区"));
            AssertRulesFit();
            yield return ClickAndRender(table.Find("Rules/Back"));
            yield return ClickAndRender(table.Find("Menu/SettingsButton"));
            float previousVolume = source.volume;
            bool previousMute = source.mute;
            const string volumeKey = "WellWithinWell.Volume";
            const string muteKey = "WellWithinWell.Mute";
            bool hasVolume = PlayerPrefs.HasKey(volumeKey);
            bool hasMute = PlayerPrefs.HasKey(muteKey);
            try
            {
                Transform card = table.Find("Settings/AudioCard");
                Transform rail = card.Find("Volume");
                var slider = rail.GetComponent<Slider>();
                var center = (RectTransform)rail;
                Hit(rail, out var pointer);
                pointer.position = RectTransformUtility.WorldToScreenPoint(null,
                    center.TransformPoint(center.rect.center + new Vector2(124, 0)));
                ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                Assert.That(slider.value, Is.InRange(0.70f, 0.80f));
                Assert.That(source.volume, Is.EqualTo(slider.value));
                Click(card.Find("Mute"));
                Assert.That(source.mute, Is.EqualTo(!previousMute));
                Click(card.Find("Mute"));
                Assert.That(source.mute, Is.EqualTo(previousMute));
                Click(table.Find("Settings/Back"));
            }
            finally
            {
                source.volume = previousVolume;
                source.mute = previousMute;
                if (hasVolume)
                    PlayerPrefs.SetFloat(volumeKey, previousVolume);
                else
                    PlayerPrefs.DeleteKey(volumeKey);
                if (hasMute)
                    PlayerPrefs.SetInt(muteKey, previousMute ? 1 : 0);
                else
                    PlayerPrefs.DeleteKey(muteKey);
                PlayerPrefs.Save();
            }
            yield return null;
        }

        private IEnumerator StartMatch()
        {
            yield return ClickAndRender(table.Find("Menu/Start"));
            yield return ClickAndRender(table.Find("Mode/ModeCard/Local"));
        }

        private WaitUntil WaitForTurn(string expected) => new WaitUntil(
            () => MatchLabel("TurnNumber") == expected, System.TimeSpan.FromSeconds(3),
            () => Assert.Fail("Did not reach turn " + expected));

        private Transform Cell(string move) => board.Find($"Region_{move[0]}/Cell_{move[1]}");
        private string MatchLabel(string path) => Label("Match/MatchCard/" + path);
        private string Label(string path) => table.Find(path).GetComponent<Text>().text;

        private void AssertLineCount(string path, int maxLines)
        {
            AssertLineCount(table.Find(path).GetComponent<Text>(), maxLines);
        }

        private static void AssertLineCount(Text label, int maxLines)
        {
            using var generator = new TextGenerator();
            Assert.That(generator.Populate(label.text, label.GetGenerationSettings(label.rectTransform.rect.size)), Is.True);
            Assert.That(generator.lineCount, Is.InRange(1, maxLines), label.name + ": " + label.text);
        }

        private void AssertRulesFit()
        {
            foreach (Text label in table.Find("Rules/RulesCard").GetComponentsInChildren<Text>())
            {
                if (label.name == "RuleName")
                    AssertLineCount(label, 1);
                else if (label.name == "RuleText")
                    AssertLineCount(label, 2);
            }
        }

        private void AssertFontCoversVisibleText(string page)
        {
            foreach (Text label in table.Find(page).GetComponentsInChildren<Text>())
                foreach (char character in label.text)
                    if (!char.IsWhiteSpace(character))
                        Assert.That(label.font.HasCharacter(character), Is.True, $"{label.name}: missing {character}");
        }

        private static void Hover(Transform cell)
        {
            GameObject target = Hit(cell, out var pointer);
            ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerEnterHandler);
        }

        private void Replay(string moves, int start = 0)
        {
            string[] path = moves.Split(' ');
            for (int i = start; i < path.Length; i++)
            {
                Click(Cell(path[i]));
                string expected = i == path.Length - 1 ? $"共{i + 1}手" : $"第{i + 2}手";
                Assert.That(MatchLabel("TurnNumber"), Is.EqualTo(expected), $"Move {i + 1}: {path[i]}");
            }
        }

        private static void Click(Transform expected, PointerEventData.InputButton button = PointerEventData.InputButton.Left)
        {
            GameObject target = Hit(expected, out var pointer);
            pointer.button = button;
            Assert.That(ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerClickHandler),
                Is.EqualTo(expected.gameObject));
        }

        private static IEnumerator ClickAndRender(Transform expected)
        {
            Click(expected);
            // Newly enabled graphics receive their raycast depth during the canvas render.
            yield return null;
        }

        private static GameObject Hit(Transform expected, out PointerEventData pointer)
        {
            Assert.That(expected, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)expected;
            pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty, $"No hit at {expected.name}");
            Transform hit = hits[0].gameObject.transform;
            Assert.That(hit == expected || hit.IsChildOf(expected), Is.True, $"{expected.name} was covered by {hit.name}");
            pointer.pointerPressRaycast = hits[0];
            return hit.gameObject;
        }
    }
}
