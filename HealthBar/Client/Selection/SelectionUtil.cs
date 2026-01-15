using HealthBar.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace HealthBar.Client.Selection;

internal static class SelectionUtil
{
    public static bool IsEligible(Entity? e, in ModConfigSnapshot cfg)
    {
        if (e is not { Alive: true }) return false;
        if (!cfg.ShowOnPlayer && e is EntityPlayer) return false;
        if (e is not EntityAgent) return false;

        return e.WatchedAttributes?.GetTreeAttribute("health") != null;
    }

    public static bool IsInFov(Vec3d playerPos, Vec3d entityPos, Vec3d forward, double cosHalfFov)
    {
        var to = entityPos.Clone().Sub(playerPos);
        to.Y = 0;
        var len = to.Length();
        if (len < 0.01) return true;
        to.Mul(1.0 / len);

        var dot = to.X * forward.X + to.Z * forward.Z;
        return dot >= cosHalfFov;
    }
}
