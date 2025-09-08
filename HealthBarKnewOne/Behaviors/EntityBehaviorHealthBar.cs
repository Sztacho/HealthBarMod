using System.Collections.Generic;
using HealthBarKnewOne.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace HealthBarKnewOne.Behaviors;
public sealed class HealthBarBehavior : EntityBehavior {
	private readonly Dictionary<long, HealthBarRenderer> bars = new();
	private readonly List<long> _toDrop = new();
	private static ICoreClientAPI Api => Core.Api as ICoreClientAPI;
	private ModConfig config => ModConfig.Instance;

	public HealthBarBehavior(Entity entity) : base(entity) { }

	public override void OnGameTick(float dt) {
		if (!config.Enabled) {
			foreach (var bar in bars.Values)
				bar.Dispose();
			bars.Clear();
			return;
		}

		var selEntity = Api.World.Player.CurrentEntitySelection?.Entity;
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
	}

	public override void OnEntityDespawn(EntityDespawnData d) {
		foreach (var bar in bars.Values)
			bar.Dispose();
		bars.Clear();
		base.OnEntityDespawn(d);
	}

	public override string PropertyName() => "mobhealthdisplay";
}