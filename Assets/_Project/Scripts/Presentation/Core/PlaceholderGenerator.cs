using System.Collections.Generic;
using UnityEngine;

namespace CatCatGo.Presentation.Core
{
    public static class PlaceholderGenerator
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite CreateRect(int width, int height, Color color, string label = null)
        {
            string key = $"rect_{width}_{height}_{ColorUtility.ToHtmlStringRGBA(color)}_{label ?? ""}";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            if (label != null && label.Length > 0)
            {
                float borderRatio = 0.08f;
                int borderX = Mathf.Max(1, (int)(width * borderRatio));
                int borderY = Mathf.Max(1, (int)(height * borderRatio));
                Color borderColor = new Color(
                    Mathf.Min(color.r + 0.2f, 1f),
                    Mathf.Min(color.g + 0.2f, 1f),
                    Mathf.Min(color.b + 0.2f, 1f),
                    color.a
                );

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (x < borderX || x >= width - borderX || y < borderY || y >= height - borderY)
                            pixels[y * width + x] = borderColor;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite CreateCircle(int radius, Color color)
        {
            string key = $"circle_{radius}_{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int diameter = radius * 2;
            var texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color[diameter * diameter];
            var transparent = new Color(0, 0, 0, 0);
            float centerX = radius - 0.5f;
            float centerY = radius - 0.5f;
            float radiusSq = radius * radius;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= radiusSq)
                    {
                        float edgeDist = radius - Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(edgeDist);
                        pixels[y * diameter + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                    }
                    else
                    {
                        pixels[y * diameter + x] = transparent;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
            Cache[key] = sprite;
            return sprite;
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }
    }
}
