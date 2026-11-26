using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WellWithinWell.Presentation;

namespace WellWithinWell.Editor
{
    internal sealed class UiWorkshop
    {
        internal static readonly Color Ink = new Color32(57, 34, 23, 255);
        internal static readonly Color SubtleInk = new Color32(104, 72, 49, 255);
        internal static readonly Color Cream = new Color32(248, 221, 178, 255);
        internal static readonly Color Faded = new Color32(209, 176, 132, 255);
        internal static readonly Color Brass = new Color32(187, 131, 76, 255);
        internal static readonly Color Frame = new Color32(115, 71, 45, 255);
        internal static readonly Color Accent = new Color32(136, 57, 35, 255);
        internal static readonly Color Shadow = new Color32(17, 10, 8, 130);

        private readonly Font font;
        private readonly AcceptButton buttonPrefab;
        private readonly List<LocalizedLabel> localized = new();

        internal LocalizedLabel[] LocalizedLabels => localized.ToArray();

        internal UiWorkshop(Font uiFont)
        {
            font = uiFont;
            var template = Node(null, "Button", 0, 0, 180, 49);
            var wood = Image(template, "Walnut", 0, 0, 180, 49, Color.white, ArtWorkshop.Sprite("Button"));
            Fill(wood.rectTransform);
            wood.raycastTarget = true;
            var edge = Outline(template, "Inlay", 0, 0, 180, 49, new Color32(184, 126, 75, 210));
            Fill(edge, 1);
            Text caption = Label(template, "Caption", "", 0, 0, 180, 49, 23, Cream, TextAnchor.MiddleCenter);
            Fill(caption.rectTransform, 5);
            var control = template.gameObject.AddComponent<Button>();
            control.targetGraphic = wood;
            control.navigation = new Navigation { mode = Navigation.Mode.None };
            control.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.19f, 1.11f, 0.97f),
                pressedColor = new Color(0.78f, 0.72f, 0.65f),
                selectedColor = Color.white,
                disabledColor = new Color(0.62f, 0.58f, 0.52f),
                colorMultiplier = 1,
                fadeDuration = 0.09f
            };
            template.gameObject.AddComponent<AcceptButton>().Wire(control, caption);
            buttonPrefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, "Assets/Prefabs/TableButton.prefab")
                .GetComponent<AcceptButton>();
            Object.DestroyImmediate(template.gameObject);
        }

        internal static RectTransform Node(RectTransform parent, string name, float x, float y, float w, float h)
        {
            var node = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)node.transform;
            rect.SetParent(parent, false);
            Place(rect, x, y, w, h);
            return rect;
        }

        internal static void Place(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
        }

        internal static void Fill(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        internal static Image Image(RectTransform parent, string name, float x, float y, float w, float h,
            Color color, Sprite sprite = null)
        {
            var image = Node(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        internal Text Label(RectTransform parent, string name, string value, float x, float y, float w, float h,
            int size, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = Node(parent, name, x, y, w, h).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.lineSpacing = 1.16f;
            label.text = value;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        internal Text Label(RectTransform parent, string name, TextKey key, float x, float y, float w, float h,
            int size, Color color, TextAnchor alignment = TextAnchor.MiddleLeft, int englishSize = 0)
        {
            Text label = Label(parent, name, TextCatalog.Get(key, GameLanguage.SimplifiedChinese),
                x, y, w, h, size, color, alignment);
            localized.Add(new LocalizedLabel(label, key, englishSize));
            return label;
        }

        internal AcceptButton Button(RectTransform parent, string name, string caption,
            float x, float y, float w, float h, bool pale = false)
        {
            var button = ((GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab.gameObject, parent)).GetComponent<AcceptButton>();
            button.name = name;
            Place((RectTransform)button.transform, x, y, w, h);
            button.Caption = caption;
            if (pale)
            {
                var image = (Image)button.Button.targetGraphic;
                image.sprite = ArtWorkshop.Sprite("Parchment");
                button.GetComponentInChildren<Text>().color = Ink;
            }
            return button;
        }

        internal AcceptButton Button(RectTransform parent, string name, TextKey key,
            float x, float y, float w, float h, bool pale = false)
        {
            AcceptButton button = Button(parent, name, TextCatalog.Get(key, GameLanguage.SimplifiedChinese),
                x, y, w, h, pale);
            localized.Add(new LocalizedLabel(button.GetComponentInChildren<Text>(), key, 0));
            return button;
        }

        internal static RectTransform Outline(RectTransform parent, string name, float x, float y, float w,
            float h, Color color, float weight = 1)
        {
            var frame = Node(parent, name, x, y, w, h);
            Edge("Top", new Vector2(0, 1), Vector2.one, new Vector2(0, weight));
            Edge("Bottom", Vector2.zero, new Vector2(1, 0), new Vector2(0, weight));
            Edge("Left", Vector2.zero, new Vector2(0, 1), new Vector2(weight, 0));
            Edge("Right", new Vector2(1, 0), Vector2.one, new Vector2(weight, 0));
            return frame;

            void Edge(string side, Vector2 min, Vector2 max, Vector2 size)
            {
                RectTransform rect = Image(frame, side, 0, 0, 0, 0, color).rectTransform;
                rect.anchorMin = rect.pivot = min;
                rect.anchorMax = max;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = size;
            }
        }

        internal static RectTransform Paper(RectTransform parent, string name, float x, float y, float w, float h)
        {
            var panel = Node(parent, name, x, y, w, h);
            Image(panel, "Shadow", 4, 6, w, h, Shadow);
            Image(panel, "Paper", 0, 0, w, h, Color.white, ArtWorkshop.Sprite("Parchment"));
            Outline(panel, "Border", 0, 0, w, h, Frame);
            Outline(panel, "Inset", 7, 7, w - 14, h - 14, new Color32(134, 81, 49, 125));
            return panel;
        }
    }
}
