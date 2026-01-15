using Vintagestory.API.Common.Entities;

namespace HealthBar.Client;

public sealed class BarState(Entity entity)
{
    public Entity Entity = entity;

    public bool DesiredVisible;
    public bool VisibleThisFrame;
    public bool IsTarget;

    public bool HasLineOfSight = true;
    public float NextLosCheckAt;

    public float Opacity;

    public float ShownPercent = 1f;
    public bool FirstShow = true;
    public bool IsDead;

    public void MarkDead() => IsDead = true;
}
