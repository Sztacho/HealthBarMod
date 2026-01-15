#nullable enable
namespace HealthBar.Config;

public sealed class ModConfig
{
    public static string ConfigName = "HealthBar.json";
    public static ModConfig Instance = new();

    // --- General
    public bool Enabled { get; set; } = true;
    public bool TargetOnlyHealthBar { get; set; } = false;
    public int DisplayRange { get; set; } = 14;
    public bool ShowOnPlayer { get; set; } = true;

    public float HorizontalFOV { get; set; } = 60f;

    public bool ShowHpText { get; set; } = true;
    public int MaxBarsDisplayed { get; set; } = 7;
    public int MaxTargetBarsDisplayed { get; set; } = 1;
    public bool AlwaysShowTargetInAround { get; set; } = true;

    public int Theme { get; set; } = 0;

    public int BarWidth { get; set; } = 66;
    public int BarHeight { get; set; } = 7;
    public float MinScale { get; set; } = 1.0f;
    public float MaxScale { get; set; } = 8.0f;

    public int VerticalOffset { get; set; } = 22;
    public float MinOffsetScale { get; set; } = 0.3f;
    public float MaxOffsetScale { get; set; } = 3.5f;

    public float FadeInSpeed { get; set; } = 0.3f;
    public float FadeOutSpeed { get; set; } = 0.5f;

    public string FullHealthColor { get; set; } = "#44FF44";
    public string MidHealthColor { get; set; } = "#FFCC00";
    public string LowHealthColor { get; set; } = "#FF4444";
    public string FrameColor { get; set; } = "#cccccc";

    public int MidHealthThreshold { get; set; } = 60;
    public int LowHealthThreshold { get; set; } = 25;
}