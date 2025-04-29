using System.Collections.Generic;
using HealthBar.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Behaviors
{
    public class HealthBarBehavior : EntityBehavior
    {
        private readonly ICoreClientAPI _api;

        private readonly Dictionary<long, HealthBarRenderer> _bars = new();

        public HealthBarBehavior(Entity entity) : base(entity)
        {
            _api = (ICoreClientAPI)entity.Api;
        }

        public override void OnGameTick(float dt)
        {
            var selEntity = _api.World.Player.CurrentEntitySelection?.Entity;
            var selId = selEntity?.EntityId ?? 0;
            
            if (selEntity != null && !_bars.ContainsKey(selId))
            {
                var bar = new HealthBarRenderer(_api, HealthBarMod.Settings)
                {
                    TargetEntity = selEntity,
                    IsVisible = true
                };
                
                _bars.Add(selId, bar);
            }
            foreach (var id in new List<long>(_bars.Keys))
            {
                var bar = _bars[id];

                bar.IsVisible = id == selId;

                var mobGone   = bar.TargetEntity is not { Alive: true };
                var fadedOut  = bar.IsFullyInvisible && !bar.IsVisible;

                if (!mobGone && !fadedOut) continue;
                bar.Dispose();
                _bars.Remove(id);
            }
        }

        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            foreach (var bar in _bars.Values) bar.Dispose();
            _bars.Clear();
            base.OnEntityDespawn(despawn);
        }

        public override string PropertyName() => "mobhealthdisplay";
    }
}