using System;
using System.Collections.Generic;
using HealthBar.Config;
using HealthBar.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace HealthBar.Behaviors;
public sealed class HealthBarBehavior : EntityBehavior {
	private readonly Dictionary<long, HealthBarRenderer> bars = new();
	private readonly List<long> _toDrop = new();
	private static ICoreClientAPI Capi => ModSystem.Api as ICoreClientAPI;
	private static ModConfig config => ModConfig.Instance;

	public HealthBarBehavior(Entity entity) : base(entity) {
		ModSystem.SettingsChanged += () => {
			foreach (var kvp in bars) {
				kvp.Value.IsVisible = false;
				kvp.Value.Dispose();
			}
			bars.Clear();
		};
	}

	public override void OnGameTick(float dt) {
		if (!config.Enabled) {
			foreach (var bar in bars.Values)
				bar.Dispose();
			bars.Clear();
			return;
		}

		if (config.TargetOnlyHealthBar) {
			var selEntity = Capi.World.Player.CurrentEntitySelection?.Entity;
			if (selEntity is EntityPlayer && !config.ShowOnPlayer)
				return;

			var selectedEntity = selEntity?.EntityId ?? 0;

			if (selEntity != null && !bars.ContainsKey(selectedEntity)) {
				bars.Add(selectedEntity, new HealthBarRenderer() { TargetEntity = selEntity, IsVisible = true });
			}

			if (bars.Count == 0 && selectedEntity == 0)
				return;

			_toDrop.Clear();
			foreach (var (key, bar) in bars) {
				bar.IsVisible = key == selectedEntity;

				var mobGone = bar.TargetEntity is not { Alive: true };
				var fadedOut = bar.IsFullyInvisible && !bar.IsVisible;

				if (mobGone || fadedOut)
					_toDrop.Add(key);
			}

			foreach (var id in _toDrop) {
				bars[id].Dispose();
				bars.Remove(id);
			}
		} else {
			var player = entity;
			var playerPos = player.Pos.XYZ;

			var range = config.DisplayRange;
			var entitiesAroundPlayer = Capi.World.GetEntitiesAround(playerPos, range, range);

			_toDrop.Clear();
			var nearbyEntityIds = new HashSet<long>();
			foreach (var e in entitiesAroundPlayer) {
				if (!e.Alive || e.EntityId == player.EntityId) {
					continue;
				}

				if (e is EntityPlayer && !config.ShowOnPlayer) {
					continue;
				}

				// First check if entity is in camera frustum (cheap check)
				if (!IsInCameraFrustum(playerPos, e.Pos.XYZ)) {
					var bar = bars.Get(e.EntityId);
					if (bar != null) {
						bar.IsVisible = false;
					}
					continue;
				}

				// Todo make a closer-first-displayed approach? But then we need to do more math
				if (nearbyEntityIds.Count >= ModConfig.Instance.MaxBarsDisplayed) {
					return;
				}

				// Only do expensive raycast if entity passed frustum check
				var barRef = bars.Get(e.EntityId);
				if (HasLineOfSight(playerPos, e.Pos.XYZ)) {
					nearbyEntityIds.Add(e.EntityId);
					if (barRef == null) {
						bars.Add(e.EntityId, new HealthBarRenderer() { TargetEntity = e, IsVisible = true });
					} else {
						barRef.IsVisible = true;
					}
				} else {
					if (barRef != null) {
						barRef.IsVisible = false;
					}
				}
			}

			// Clean up bars for entities that are out of range, dead, or faded out

			foreach (var (key, bar) in bars) {
				var mobGone = bar.TargetEntity is not { Alive: true };
				var fadedOut = bar.IsFullyInvisible && !bar.IsVisible;

				var outOfRange = !nearbyEntityIds.Contains(key);
				if (outOfRange) {
					bar.IsVisible = false;
				}

				if (mobGone || fadedOut) {
					bar.IsVisible = false;
					_toDrop.Add(key);
				}
			}

			foreach (var id in _toDrop) {
				bars.Get(id)?.Dispose();
				bars.Remove(id);
			}
		}
	}

	private static bool IsInCameraFrustum(Vec3d playerPos, Vec3d entityPos) {
		var player = Capi.World.Player;
		if (player == null) return false;

		var pitch = player.CameraPitch;
		var yaw = player.CameraYaw;

		// Calculate direction from player to entity
		var toEntity = entityPos.Clone().Sub(playerPos);
		toEntity.Y = 0; // Project to horizontal plane for horizontal FOV check
		var horizontalDist = toEntity.Length();
		if (horizontalDist < 0.01) return true; // Entity is basically at player position

		toEntity.Normalize();

		// Calculate camera forward direction on horizontal plane
		var cameraYawVec = new Vec3d(
			GameMath.Sin(yaw),
			0,
			GameMath.Cos(yaw)
		).Normalize();

		// Check horizontal FOV
		var dotProduct = toEntity.X * cameraYawVec.X + toEntity.Z * cameraYawVec.Z;
		var horizontalAngle = GameMath.Acos(GameMath.Clamp(dotProduct, -1.0, 1.0));
		var halfHorizontalFOV = config.HorizontalFOV * GameMath.DEG2RAD / 2.0;

		return horizontalAngle <= halfHorizontalFOV;
	}

	private static bool HasLineOfSight(Vec3d fromPos, Vec3d toPos) {
		BlockSelection blockSel = null;
		EntitySelection entitySel = null;
		Vec3d from = new Vec3d(fromPos.X, fromPos.Y + 1.6, fromPos.Z);
		Vec3d to = new Vec3d(toPos.X, toPos.Y + 1.6, toPos.Z);
		Capi.World.RayTraceForSelection(from, to, ref blockSel, ref entitySel);
		return blockSel == null;
	}

	public override void OnEntityDespawn(EntityDespawnData d) {
		foreach (var bar in bars.Values) {
			bar.Dispose();

		}
		bars.Clear();
		base.OnEntityDespawn(d);
	}

	public override string PropertyName() => "mobhealthdisplay";
}
