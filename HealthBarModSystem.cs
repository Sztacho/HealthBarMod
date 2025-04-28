using HealthBar.Behaviors;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar
{
    public class HealthBarMod : ModSystem
    {
        public static HealthBarSettings Settings { get; private set; } = new HealthBarSettings();

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            Settings = api.LoadModConfig<HealthBarSettings>("mobhealthdisplay.json") ?? new HealthBarSettings();
            api.StoreModConfig(Settings, "mobhealthdisplay.json");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.RegisterEntityBehaviorClass("mobhealthdisplay", typeof(HealthBarBehavior));
            api.Event.PlayerEntitySpawn += player =>
                player.Entity.AddBehavior(new HealthBarBehavior(player.Entity));
        }
    }
}