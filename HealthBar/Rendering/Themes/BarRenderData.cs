using Vintagestory.API.Common.Entities;

namespace HealthBar.Rendering.Themes;

public readonly struct BarRenderData(
    Entity entity,
    float currentHp,
    float maxHp,
    float percentHp,
    float shownPercent,
    float x,
    float y,
    float width,
    float height,
    float scale,
    float opacity,
    bool isTarget)
{
    public readonly Entity Entity = entity;
    public readonly float CurrentHp = currentHp;
    public readonly float MaxHp = maxHp;
    public readonly float PercentHp = percentHp;
    public readonly float ShownPercent = shownPercent;
    public readonly float X = x;
    public readonly float Y = y;
    public readonly float Width = width;
    public readonly float Height = height;
    public readonly float Scale = scale;
    public readonly float Opacity = opacity;
    public readonly bool IsTarget = isTarget;
}
