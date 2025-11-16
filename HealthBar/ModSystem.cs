using System;
using ConfigLib;
using HealthBar.Behaviors;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar;
public class ModSystem : Vintagestory.API.Common.ModSystem {
	public static ILogger Logger { get; private set; }
	public static ICoreAPI Api { get; private set; }
	public static event Action SettingsChanged;

	public override void Start(ICoreAPI api) {
		base.Start(api);
		api.RegisterEntityBehaviorClass("mobhealthdisplay", typeof(HealthBarBehavior));

		Api = api;
		Logger = Mod.Logger;

		try {
			ModConfig.Instance = api.LoadModConfig<ModConfig>(ModConfig.ConfigName) ?? new ModConfig();
			api.StoreModConfig(ModConfig.Instance, ModConfig.ConfigName);
		} catch (Exception _) { ModConfig.Instance = new ModConfig(); }

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
			if (domain != "healthbar")
				return;

			setting.AssignSettingValue(ModConfig.Instance);
			SettingsChanged?.Invoke();
		};
	}
}
