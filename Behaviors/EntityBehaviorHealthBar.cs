using HealthBar.Rendering;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Behaviors
{
    public class HealthBarBehavior : EntityBehavior
    {
        private readonly ICoreClientAPI _api;
        private readonly HealthBarRenderer _renderer;

        public HealthBarBehavior(Entity entity) : base(entity)
        {
            _api = (ICoreClientAPI)entity.Api;
            _renderer = new HealthBarRenderer(_api, HealthBarMod.Settings);
        }

        public override void OnGameTick(float deltaTime)
        {
            var selectedEntity = _api.World.Player.CurrentEntitySelection?.Entity;
            _renderer.TargetEntity = selectedEntity;
            _renderer.IsVisible = selectedEntity != null;
        }

        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            _renderer.Dispose();
        }

        public override string PropertyName() => "mobhealthdisplay";
    }
}