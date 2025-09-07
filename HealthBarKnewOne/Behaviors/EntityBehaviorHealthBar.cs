using System.Collections.Generic;
using HealthBar.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Behaviors {
	public sealed class HealthBarBehavior : EntityBehavior {
		private readonly Dictionary<long, HealthBarRenderer> _bars = new();
		private readonly List<long> _toDrop = new();
		private ICoreClientAPI capi;
		private ModConfig config => ModConfig.Instance;

		public HealthBarBehavior(Entity entity) : base(entity) {
			capi = (ICoreClientAPI)entity.Api;
		}

		public override void OnGameTick(float dt) {
			if (!config.Enabled) {
				foreach (var bar in _bars.Values)
					bar.Dispose();
				_bars.Clear();
				return;
			}

			var selEntity = capi.World.Player.CurrentEntitySelection?.Entity;
			if (selEntity is EntityPlayer && !config.ShowOnPlayer)
				return;

			var selId = selEntity?.EntityId ?? 0;

			if (selEntity != null && !_bars.ContainsKey(selId)) {
				_bars.Add(selId, new HealthBarRenderer(capi, config) {
					TargetEntity = selEntity,
					IsVisible = true
				});
			}

			if (_bars.Count == 0 && selId == 0)
				return;

			_toDrop.Clear();
			foreach (var (key, bar) in _bars) {
				bar.IsVisible = key == selId;

				var mobGone = bar.TargetEntity is not { Alive: true };
				var fadedOut = bar.IsFullyInvisible && !bar.IsVisible;

				if (mobGone || fadedOut)
					_toDrop.Add(key);
			}

			foreach (var id in _toDrop) {
				_bars[id].Dispose();
				_bars.Remove(id);
			}
		}

		public override void OnEntityDespawn(EntityDespawnData d) {
			foreach (var bar in _bars.Values)
				bar.Dispose();
			_bars.Clear();
			base.OnEntityDespawn(d);
		}

		public override string PropertyName() => "mobhealthdisplay";
	}
}