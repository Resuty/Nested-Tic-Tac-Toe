using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WellWithinWell.Presentation;

using static WellWithinWell.Editor.UiWorkshop;

namespace WellWithinWell.Editor
{
    internal sealed class TableWorkshop
    {
        internal const int Width = 664;
        private const int RegionSize = 206;
        private const int RegionStep = 212;
        private const int SurfaceSize = RegionStep * 2 + RegionSize;
        private const int CellSize = 66;
        private const int CellStep = 68;
        private readonly BoardSkin skin;
        private readonly RegionView regionPrefab;

        internal TableWorkshop(UiWorkshop ui, BoardSkin art)
        {
            skin = art;
            var cell = Node(null, "Cell", 0, 0, CellSize, CellSize);
            var hit = cell.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;
            var number = ui.Label(cell, "CellNumber", "1", 0, 24, CellSize, 27, 15,
                new Color32(91, 59, 33, 202), TextAnchor.MiddleCenter);
            var token = Image(cell, "Token", 5, 5, 56, 56, Color.white);
            var marker = Image(cell, "LastMoveMarker", 2, 2, 62, 62, Color.white, ArtWorkshop.Sprite("LastMove"));
            token.enabled = marker.enabled = false;
            cell.gameObject.AddComponent<CellView>().Wire(hit, token, marker, number);
            var cellPrefab = PrefabUtility.SaveAsPrefabAsset(cell.gameObject, "Assets/Prefabs/BoardCell.prefab")
                .GetComponent<CellView>();
            Object.DestroyImmediate(cell.gameObject);

            var region = Node(null, "Region", 0, 0, RegionSize, RegionSize);
            var tint = Image(region, "RegionTint", 0, 0, RegionSize, RegionSize, Color.clear);
            var seal = Image(region, "OwnershipSeal", 17, 17, 172, 172, Color.white);
            seal.enabled = false;
            for (int i = 1; i <= 2; i++)
            {
                float p = CellStep * i;
                Color thin = new Color32(109, 68, 42, 156);
                Image(region, $"Horizontal{i}", 0, p, RegionSize, 1, thin);
                Image(region, $"Vertical{i}", p, 0, 1, RegionSize, thin);
            }
            var edge = Image(region, "LegalBorder", 0, 0, RegionSize, RegionSize,
                new Color32(92, 103, 51, 235), BorderSprite());
            edge.enabled = false;
            var cells = new CellView[9];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = ((GameObject)PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, region)).GetComponent<CellView>();
                cells[i].name = $"Cell_{i + 1}";
                Place((RectTransform)cells[i].transform, 2 + i % 3 * CellStep, 2 + i / 3 * CellStep, CellSize, CellSize);
            }
            Image(region, "NumberPlate", 2, 2, 22, 20, new Color32(103, 65, 38, 237));
            var heading = ui.Label(region, "RegionAddress", "一", 2, 2, 22, 20, 12, Cream, TextAnchor.MiddleCenter);
            var line = BuildLine(region, "LocalWin");
            region.gameObject.AddComponent<RegionView>().Wire(cells, tint, edge, seal, heading, line);
            regionPrefab = PrefabUtility.SaveAsPrefabAsset(region.gameObject, "Assets/Prefabs/BoardRegion.prefab")
                .GetComponent<RegionView>();
            Object.DestroyImmediate(region.gameObject);
        }

        internal BoardView Build(RectTransform parent, string name, float x, float y, float scale = 1)
        {
            var table = Node(parent, name, x, y, Width, Width);
            table.localScale = Vector3.one * scale;
            Image(table, "OuterShadow", 5, 8, Width, Width, UiWorkshop.Shadow);
            Image(table, "WalnutFrame", 0, 0, Width, Width, Color.white, ArtWorkshop.Sprite("Walnut"));
            Outline(table, "ExposedEdge", 0, 0, Width, Width, new Color32(176, 112, 63, 255), 2);
            Outline(table, "Inlay", 7, 7, Width - 14, Width - 14, new Color32(169, 104, 55, 232));
            Outline(table, "Recess", 14, 14, Width - 28, Width - 28, Ink, 3);
            float inset = (Width - SurfaceSize) / 2f;
            var surface = Node(table, "Surface", inset, inset, SurfaceSize, SurfaceSize);
            Image(surface, "Maple", 0, 0, SurfaceSize, SurfaceSize, Color.white, ArtWorkshop.Sprite("Maple"));
            for (int i = 0; i < 2; i++)
            {
                float p = RegionSize + i * RegionStep;
                Image(surface, $"DividerH{i}", 0, p, SurfaceSize, 6, Frame);
                Image(surface, $"DividerV{i}", p, 0, 6, SurfaceSize, Frame);
                Image(surface, $"DividerLightH{i}", 0, p + 5, SurfaceSize, 1, Brass);
                Image(surface, $"DividerLightV{i}", p + 5, 0, 1, SurfaceSize, Brass);
            }
            var regions = new RegionView[9];
            for (int i = 0; i < regions.Length; i++)
            {
                regions[i] = ((GameObject)PrefabUtility.InstantiatePrefab(regionPrefab.gameObject, surface)).GetComponent<RegionView>();
                regions[i].name = $"Region_{i + 1}";
                Place((RectTransform)regions[i].transform, i % 3 * RegionStep, i / 3 * RegionStep, RegionSize, RegionSize);
            }
            LineView winningLine = BuildLine(surface, "GlobalWin");
            var board = table.gameObject.AddComponent<BoardView>();
            board.Wire(skin, regions, winningLine);
            return board;
        }

        private static LineView BuildLine(RectTransform parent, string name)
        {
            var root = Node(parent, name, 0, 0, 0, 0);
            root.pivot = new Vector2(0, 0.5f);
            var pale = Image(root, "Highlight", 0, 0, 0, 0, new Color32(255, 223, 178, 233));
            var ink = Image(root, "LineInk", 0, 0, 0, 0, Color.white);
            Fill(pale.rectTransform);
            pale.rectTransform.offsetMin = new Vector2(-2, -2);
            pale.rectTransform.offsetMax = new Vector2(2, 2);
            Fill(ink.rectTransform);
            pale.enabled = ink.enabled = false;
            var line = root.gameObject.AddComponent<LineView>();
            line.Wire(ink, pale);
            return line;
        }

        private static Sprite BorderSprite()
        {
            const string path = "Assets/Art/RegionOutline.asset";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;
            var texture = new Texture2D(206, 206, TextureFormat.RGBA32, false);
            texture.name = "Region outline texture";
            var pixels = new Color32[206 * 206];
            for (int y = 0; y < 206; y++)
                for (int x = 0; x < 206; x++)
                    if (x < 3 || x > 202 || y < 3 || y > 202)
                        pixels[y * 206 + x] = Color.white;
            texture.SetPixels32(pixels);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, 206, 206), new Vector2(0.5f, 0.5f), 100,
                0, SpriteMeshType.FullRect);
            sprite.name = "Region outline";
            AssetDatabase.CreateAsset(sprite, path);
            AssetDatabase.AddObjectToAsset(texture, path);
            return sprite;
        }
    }
}
