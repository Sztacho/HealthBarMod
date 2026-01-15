using System;
using Vintagestory.API.MathTools;

namespace HealthBar.Config;

public readonly struct ModConfigSnapshot(ModConfig cfg)
{
    public readonly bool Enabled = cfg.Enabled;
    public readonly bool TargetOnly = cfg.TargetOnlyHealthBar;
    public readonly int DisplayRange = cfg.DisplayRange;
    public readonly bool ShowOnPlayer = cfg.ShowOnPlayer;
    public readonly float HorizontalFovDeg = cfg.HorizontalFOV;
    public readonly bool ShowHpText = cfg.ShowHpText;
    public readonly int MaxBarsDisplayed = cfg.MaxBarsDisplayed;
    public readonly int MaxTargetBarsDisplayed = cfg.MaxTargetBarsDisplayed;
    public readonly bool AlwaysShowTargetInAround = cfg.AlwaysShowTargetInAround;

    public readonly string ThemeId = cfg.Theme == 1 ? "pixel" : "basic";

    public readonly int BarWidth = cfg.BarWidth;
    public readonly int BarHeight = cfg.BarHeight;
    public readonly float MinScale = cfg.MinScale;
    public readonly float MaxScale = cfg.MaxScale;

    public readonly int VerticalOffset = cfg.VerticalOffset;
    public readonly float MinOffsetScale = cfg.MinOffsetScale;
    public readonly float MaxOffsetScale = cfg.MaxOffsetScale;

    public readonly float FadeInSpeed = Math.Max(0.001f, cfg.FadeInSpeed);
    public readonly float FadeOutSpeed = Math.Max(0.001f, cfg.FadeOutSpeed);

    public readonly int FullHealthColorArgb = ColorUtil.Hex2Int(cfg.FullHealthColor ?? "#44FF44");
    public readonly int MidHealthColorArgb = ColorUtil.Hex2Int(cfg.MidHealthColor ?? "#FFCC00");
    public readonly int LowHealthColorArgb = ColorUtil.Hex2Int(cfg.LowHealthColor ?? "#FF4444");
    public readonly int FrameColorArgb = ColorUtil.Hex2Int(cfg.FrameColor ?? "#CCCCCC");

    public readonly int MidHealthThreshold = cfg.MidHealthThreshold;
    public readonly int LowHealthThreshold = cfg.LowHealthThreshold;
}