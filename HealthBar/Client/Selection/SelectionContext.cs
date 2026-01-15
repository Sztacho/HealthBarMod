using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace HealthBar.Client.Selection;

public readonly struct SelectionContext
{
    public readonly ICoreClientAPI Api;
    public readonly ModConfigSnapshot Config;
    public readonly EntityPlayer PlayerEntity;
    public readonly Vec3d PlayerPos;
    public readonly Vec3d Forward;
    public readonly double CosHalfFov;
#nullable enable
    public readonly Entity? Target;
#nullable disable

    public SelectionContext(ICoreClientAPI api, in ModConfigSnapshot cfg)
    {
        Api = api;
        Config = cfg;
        PlayerEntity = api.World.Player.Entity;
        PlayerPos = PlayerEntity.Pos.XYZ;

        var yaw = api.World.Player.CameraYaw;
        Forward = new Vec3d(GameMath.Sin(yaw), 0, GameMath.Cos(yaw)).Normalize();
        CosHalfFov = GameMath.Cos((cfg.HorizontalFovDeg * GameMath.DEG2RAD) * 0.5);
        Target = api.World.Player.CurrentEntitySelection?.Entity;
    }
}