using System;
using HealthBar.Behaviors;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar;
public class HealthBarMod : ModSystem {
	public static ILogger Logger { get; private set; }
	public static ICoreClientAPI Api { get; private set; }
	public override void StartClientSide(ICoreClientAPI api) {
		Api = api;
		Logger = Mod.Logger;

		try {
			ModConfig.Instance = api.LoadModConfig<ModConfig>(ModConfig.ConfigName);
			if (ModConfig.Instance == null) {
				ModConfig.Instance = new ModConfig();
				Logger.VerboseDebug("[HealthBarMod] Config file not found, creating a new one...");
			}
			api.StoreModConfig(ModConfig.Instance, ModConfig.ConfigName);
		} catch (Exception e) {
			Logger.Error("[HealthBarMod] Failed to load config, you probably made a typo: {0}", e);
			ModConfig.Instance = new ModConfig();
		}


		api.RegisterEntityBehaviorClass("mobhealthdisplay", typeof(HealthBarBehavior));
		api.Event.PlayerEntitySpawn += player => player.Entity.AddBehavior(new HealthBarBehavior(player.Entity));
	}
}
