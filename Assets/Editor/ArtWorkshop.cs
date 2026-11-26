using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WellWithinWell.Editor
{
    internal static class ArtWorkshop
    {
        internal const string Folder = "Assets/Art";
        private const int SampleRate = 44100;

        internal static void Generate()
        {
            Directory.CreateDirectory(Folder);
            Wood("Desk", 1100, 720, new Color32(46, 31, 25, 255), 1, true);
            Wood("Maple", 720, 720, new Color32(218, 176, 119, 255), 2);
            Wood("Walnut", 720, 720, new Color32(92, 51, 31, 255), 3);
            Wood("Parchment", 520, 620, new Color32(238, 212, 170, 255), 4);
            Wood("Button", 420, 112, new Color32(93, 47, 28, 255), 7);
            Paint("Circle", 180, 180, (x, y) => Token(x, y, true));
            Paint("Cross", 180, 180, (x, y) => Token(x, y, false));
            Paint("Draw", 180, 180, (x, y) =>
            {
                float distance = Mathf.Min(Segment(x, y, -0.56f, -0.14f, 0.56f, -0.14f),
                    Segment(x, y, -0.56f, 0.14f, 0.56f, 0.14f));
                return Ink(x, y, distance - 0.051f, new Color32(61, 64, 55, 255));
            });
            Paint("LastMove", 112, 112, (x, y) =>
            {
                x = Mathf.Abs(x);
                y = Mathf.Abs(y);
                float edge = Mathf.Min(Segment(x, y, 0.89f, 0.49f, 0.89f, 0.89f),
                    Segment(x, y, 0.49f, 0.89f, 0.89f, 0.89f));
                return Ink(x, y, edge - 0.035f, new Color32(133, 59, 29, 255));
            });
            WoodTap("ButtonTap", 490f, 0.42f, 0f);
            WoodTap("PieceSet", 304f, 0.90f, 0f);
            WoodTap("RegionClaim", 266f, 0.72f, 0f, 0.067f);
            WoodTap("MatchEnd", 218f, 0.78f, 0f, 0.079f, 0.17f);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string path in Directory.GetFiles(Folder, "*.png"))
            {
                if (AssetImporter.GetAtPath(path.Replace('\\', '/')) is not TextureImporter importer)
                    continue;
                if (importer.textureType == TextureImporterType.Sprite && importer.mipmapEnabled == false)
                    continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void Wood(string name, int width, int height, Color basis, int mode, bool planks = false)
        {
            Paint(name, width, height, (x, y) =>
            {
                float u = (x + 1) * 0.5f;
                float v = (y + 1) * 0.5f;
                float flow = v + (Noise(u * 3.9f, v * 7 + mode) - 0.5f) * 0.027f
                    + 0.004f * Mathf.Sin(u * 10 + v * 15);
                float ribbons = Noise(u * 4, flow * 31 + 7 * mode) - 0.5f;
                float silk = Noise(u * 9, flow * 94 + mode) - 0.5f;
                float pores = Noise(u * 31, flow * 535 + mode);
                float light = 0.041f * ribbons + 0.027f * silk - 0.06f * Mathf.Pow(pores, 7)
                    + 0.016f * (Noise(u * 174, v * 210) - 0.5f);
                if (mode == 4)
                    light *= 0.40f;
                if (planks)
                {
                    light += 0.054f * Mathf.Exp(-1.8f * (x * x + y * y))
                        - 0.023f * (x * x + y * y) + 0.011f * ribbons;
                    float board = (v * 4.4f + 0.1f) % 1f;
                    light -= 0.064f * Mathf.Exp(-board * 155);
                    light += 0.012f * Mathf.Exp(-Mathf.Abs(board - 0.016f) * 158);
                }
                else
                    light += 0.019f * (v - u) + 0.013f * (1 - x * x - y * y);
                return new Color(basis.r + light, basis.g + light * 0.88f, basis.b + light * 0.72f, 1);
            });
        }

        private static Color Token(float x, float y, bool circle)
        {
            float theta = Mathf.Atan2(y, x);
            float radius = Mathf.Sqrt(x * x + y * y);
            float rough = 0.014f * (Noise(x * 24 + 24, y * 24 + 24) - 0.5f);
            float distance;
            if (circle)
            {
                float profile = 0.061f + 0.027f * Mathf.Sin(theta + 0.75f) + 0.015f * Mathf.Sin(theta * 3 - 0.6f);
                distance = Mathf.Abs(radius - 0.688f - 0.021f * Mathf.Sin(theta * 3) + 0.02f * Mathf.Cos(theta * 2))
                    - profile + rough;
            }
            else
            {
                float first = Segment(x, y, -0.65f, -0.60f, 0.64f, 0.67f) - (0.075f + 0.026f * x);
                float second = Segment(x, y, -0.65f, 0.63f, 0.62f, -0.59f) - (0.079f - 0.033f * x);
                distance = Mathf.Min(first, second) + rough;
            }
            return Ink(x, y, distance, circle ? new Color32(132, 55, 33, 255) : new Color32(35, 58, 58, 255));
        }

        private static Color Ink(float x, float y, float distance, Color color)
        {
            float fiber = Noise(x * 52 + 110, y * 82 + 110);
            float grain = (fiber - 0.5f) * 0.15f;
            float edge = 0.09f * Mathf.Clamp01(1 - Mathf.Abs(distance + 0.009f) * 68);
            float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-0.009f, 0.013f, distance));
            return new Color(color.r + grain + edge, color.g + grain + edge, color.b + grain + edge, alpha);
        }

        private static float Segment(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float projection = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy));
            return new Vector2(x - ax - projection * dx, y - ay - projection * dy).magnitude;
        }

        private static float Noise(float x, float y)
        {
            int a = Mathf.FloorToInt(x);
            int b = Mathf.FloorToInt(y);
            float u = Mathf.SmoothStep(0, 1, x - a);
            float v = Mathf.SmoothStep(0, 1, y - b);
            return Mathf.Lerp(Mathf.Lerp(Hash(a, b), Hash(a + 1, b), u),
                Mathf.Lerp(Hash(a, b + 1), Hash(a + 1, b + 1), u), v);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                uint seed = (uint)x * 42026339u ^ (uint)y * 92845913u + 8267219u;
                seed = (seed ^ (seed >> 13)) * 1274126177u;
                return (seed & 65535) / 65535f;
            }
        }

        private static void Paint(string name, int width, int height, Func<float, float, Color> shader)
        {
            string path = $"{Folder}/{name}.png";
            if (File.Exists(path))
                return;

            var pixels = new Color32[width * height];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    pixels[i * width + j] = shader((j + 0.5f) * 2 / width - 1, (i + 0.5f) * 2 / height - 1);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static void WoodTap(string name, float tone, float gain, params float[] impacts)
        {
            string path = $"{Folder}/{name}.wav";
            float duration = impacts[impacts.Length - 1] + 0.13f;
            int length = Mathf.RoundToInt(duration * SampleRate);
            using var file = new BinaryWriter(File.Create(path));
            file.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            file.Write(36 + 2 * length);
            file.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            file.Write(16);
            file.Write((ushort)1);
            file.Write((ushort)1);
            file.Write(SampleRate);
            file.Write(SampleRate * 2);
            file.Write((ushort)2);
            file.Write((ushort)16);
            file.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            file.Write(length * 2);

            float noise = 0;
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float raw = Hash(i, 513) * 2 - 1;
                noise = 0.69f * noise + 0.31f * raw;
                float sample = 0;
                for (int j = 0; j < impacts.Length; j++)
                {
                    float age = t - impacts[j];
                    if (age < 0)
                        continue;
                    float phase = tone * age * (1 - j * 0.045f) * (2 * Mathf.PI);
                    float contact = (0.37f * noise + 0.25f * raw) * Mathf.Exp(-age * 520);
                    float board = 0.53f * Mathf.Sin(phase) * Mathf.Exp(-age * 101);
                    float piece = 0.25f * Mathf.Sin(phase * 2.49f) * Mathf.Exp(-age * 190);
                    float attack = Mathf.Min(1, age * 4400);
                    sample += attack * (contact + board + piece) / (1 + j * 0.22f);
                }
                float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(duration * 0.76f, duration, t));
                file.Write((short)(32767 * Mathf.Clamp(sample * gain * fade, -1, 1)));
            }
        }

        internal static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Folder}/{name}.png");
        internal static AudioClip Sound(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{Folder}/{name}.wav");
    }
}
