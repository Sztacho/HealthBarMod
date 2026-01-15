using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace HealthBar.Client.Visibility;

public sealed class RaycastLineOfSightService(ICoreClientAPI capi) : ILineOfSightService
{
    public bool HasLineOfSight(Entity target)
    {
        var player = capi.World.Player;
        if (player?.Entity == null) return false;

        BlockSelection blockSel = null;
        EntitySelection entitySel = null;

        var fromPos = player.Entity.Pos.XYZ;
        var toPos = target.Pos.XYZ;

        Vec3d from = new(fromPos.X, fromPos.Y + 1.6, fromPos.Z);

        var heightRef = target.SelectionBox?.Y2 ?? target.CollisionBox.Y2;
        Vec3d to = new(toPos.X, toPos.Y + heightRef * 0.9, toPos.Z);

        capi.World.RayTraceForSelection(from, to, ref blockSel, ref entitySel);
        return blockSel == null;
    }
}
