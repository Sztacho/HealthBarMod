using System;
using ConfigLib;
using HealthBarKnewOne.Behaviors;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBarKnewOne;
public class Core : ModSystem {
	public static ILogger Logger { get; private set; }
	public static ICoreAPI Api { get; private set; }
	public static Mod ModI { get; private set; }

	public override void Start(ICoreAPI api) {
		base.Start(api);
		api.RegisterEntityBehaviorClass("mobhealthdisplay", typeof(HealthBarBehavior));

		Api = api;
		Logger = Mod.Logger;
		ModI = Mod;

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

		if (api.ModLoader.IsModEnabled("configlib")) {
			SubscribeToConfigChange(api);
		}
	}

	public override void StartClientSide(ICoreClientAPI api) {
		api.Event.PlayerEntitySpawn += player => player.Entity.AddBehavior(new HealthBarBehavior(player.Entity));
	}

	private void SubscribeToConfigChange(ICoreAPI api) {
		ConfigLibModSystem system = api.ModLoader.GetModSystem<ConfigLibModSystem>();

		system.SettingChanged += (domain, config, setting) => {
			if (domain != "healthbarknewone")
				return;

			var succ = setting.AssignSettingValue(ModConfig.Instance);
			ModConfig.Instance = api.LoadModConfig<ModConfig>(ModConfig.ConfigName); // Refused to cooperate without this
		};

		system.ConfigsLoaded += () => {
			system.GetConfig("healthbarknewone")?.AssignSettingsValues(ModConfig.Instance);
		};
	}
}