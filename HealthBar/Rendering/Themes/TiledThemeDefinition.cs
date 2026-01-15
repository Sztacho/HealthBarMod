using System;
using System.Text.Json;

namespace HealthBar.Rendering.Themes;

public sealed class TiledThemeDefinition
{
    public required string Texture { get; init; }

    public int Tile { get; init; } = 32;
    public int TexW { get; init; } = 256;
    public int TexH { get; init; } = 32;

    public int BarStartOffset { get; init; } = 24;
    public int InnerPad { get; init; } = 6;

    public int MinMidTiles { get; init; } = 2;
    public int MaxMidTiles { get; init; } = 16;

    public float UvInsetPx { get; init; } = 0.5f;

    public Tileset Tiles { get; init; } = new();

    public sealed class Tileset
    {
        public int FrameLeft { get; init; } = 0;
        public int FrameMid { get; init; } = 1;
        public int FrameRight { get; init; } = 2;

        public int BgLeft { get; init; } = 3;
        public int BgMid { get; init; } = 4;
        public int BgRight { get; init; } = 5;

        public int? FrameLeftOuter { get; init; }
        public int? FrameLeftInner { get; init; }
        public int? FrameRightInner { get; init; }
        public int? FrameRightOuter { get; init; }

        public int? BgLeftOuter { get; init; }
        public int? BgLeftInner { get; init; }
        public int? BgRightInner { get; init; }
        public int? BgRightOuter { get; init; }

        public int Fill { get; init; } = 6;
        public int Heart { get; init; } = 7;
        public bool HasHeart { get; init; } = true;
    }


    public int BaseUnitsNoMid => BarStartOffset + (2 * Tile);

    public static TiledThemeDefinition FromJson(string json)
    {
        var opt = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var def = JsonSerializer.Deserialize<TiledThemeDefinition>(json, opt);
        if (def == null || string.IsNullOrWhiteSpace(def.Texture))
            throw new ArgumentException("Theme JSON must define 'texture'.");
        return def;
    }
}
