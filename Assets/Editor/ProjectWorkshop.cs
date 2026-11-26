using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WellWithinWell.Presentation;

using static WellWithinWell.Editor.UiWorkshop;

namespace WellWithinWell.Editor
{
    public static class ProjectWorkshop
    {
        public const string ScenePath = "Assets/Scenes/WellWithinWell.unity";
        public const string WindowsPath = "Build/Windows/NestedTicTacToe.exe";

        [MenuItem("井中井/生成场景与资源")]
        public static void Assemble()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/NotoSerifSC.ttf");
            if (font == null)
                throw new InvalidOperationException("Missing Assets/Art/NotoSerifSC.ttf");

            Directory.CreateDirectory("Assets/Prefabs");
            Directory.CreateDirectory("Assets/Scenes");
            ArtWorkshop.Generate();
            BoardSkin art = Skin();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Table Camera", typeof(Camera), typeof(AudioListener));
            Camera lens = camera.GetComponent<Camera>();
            lens.clearFlags = CameraClearFlags.SolidColor;
            lens.backgroundColor = new Color32(38, 24, 17, 255);
            lens.orthographic = true;
            lens.cullingMask = 0;
            new GameObject("Pointer Input", typeof(EventSystem), typeof(StandaloneInputModule));

            var table = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            table.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = table.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(1366, 848);

            var tableRect = (RectTransform)table.transform;
            var desk = Image(tableRect, "DarkWoodDesk", 0, 0, 0, 0, Color.white, ArtWorkshop.Sprite("Desk"));
            Fill(desk.rectTransform);

            RectTransform root = Node(tableRect, "TableLayout", 0, 0, 1232, 768);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;

            var sound = new GameObject("Table Audio", typeof(AudioSource), typeof(AudioController));
            var source = sound.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            var audio = sound.GetComponent<AudioController>();
            audio.Wire(source, ArtWorkshop.Sound("ButtonTap"), ArtWorkshop.Sound("PieceSet"),
                ArtWorkshop.Sound("RegionClaim"), ArtWorkshop.Sound("MatchEnd"));
            UIController controller = table.AddComponent<UIController>();

            var ui = new UiWorkshop(font);
            var board = new TableWorkshop(ui, art);
            new ScreenWorkshop(ui, board, art).Create(root, controller, audio);

            PlayerSettings.companyName = "TangYu";
            PlayerSettings.productName = "井中井";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone, "com.tangyu.wellwithinwell");
            PlayerSettings.defaultScreenWidth = 1366;
            PlayerSettings.defaultScreenHeight = 848;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save " + ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("井中井 scene, Prefabs and art saved to Assets.");
        }

        [MenuItem("井中井/构建 Windows 游戏")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath))
                throw new FileNotFoundException(ScenePath);
            Directory.CreateDirectory(Path.GetDirectoryName(WindowsPath));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = BuildTarget.StandaloneWindows64,
                locationPathName = WindowsPath,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Directory.CreateDirectory("Verification");
            File.WriteAllText("Verification/windows-build.txt",
                $"{summary.result}\nPlatform: {summary.platform}\nBytes: {summary.totalSize}\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\n");
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows build: {summary.result} ({summary.totalErrors} errors).");
            File.Copy("NotoSerifSC-OFL.txt",
                Path.Combine(Path.GetDirectoryName(WindowsPath), "NotoSerifSC-OFL.txt"), true);
            Debug.Log($"Windows build saved to {WindowsPath}");
        }

        private static BoardSkin Skin()
        {
            const string path = "Assets/Art/BoardSkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<BoardSkin>(path);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<BoardSkin>();
                AssetDatabase.CreateAsset(skin, path);
            }
            var properties = new SerializedObject(skin);
            properties.FindProperty("circle").objectReferenceValue = ArtWorkshop.Sprite("Circle");
            properties.FindProperty("cross").objectReferenceValue = ArtWorkshop.Sprite("Cross");
            properties.FindProperty("draw").objectReferenceValue = ArtWorkshop.Sprite("Draw");
            properties.FindProperty("lastMove").objectReferenceValue = ArtWorkshop.Sprite("LastMove");
            properties.ApplyModifiedPropertiesWithoutUndo();
            return skin;
        }
    }
}
