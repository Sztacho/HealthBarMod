using System;
using ConfigLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly ICoreAPI _api;

    public ModConfigSnapshot Snapshot { get; private set; }
    #nullable enable
    public event Action? Changed;
    #nullable disable
    public ConfigProvider(ICoreAPI api)
    {
        this._api = api;
        ReloadFromDisk();
        HookConfigLibIfPresent();
    }

    public void ReloadFromDisk()
    {
        try
        {
            ModConfig.Instance = _api.LoadModConfig<ModConfig>(ModConfig.ConfigName) ?? new ModConfig();
            _api.StoreModConfig(ModConfig.Instance, ModConfig.ConfigName);
        }
        catch
        {
            ModConfig.Instance = new ModConfig();
        }

        Snapshot = new ModConfigSnapshot(ModConfig.Instance);
    }

    public void NotifyChanged()
    {
        Snapshot = new ModConfigSnapshot(ModConfig.Instance);
        Changed?.Invoke();
    }

    private void HookConfigLibIfPresent()
    {
        if (!_api.ModLoader.IsModEnabled("configlib")) return;

        var system = _api.ModLoader.GetModSystem<ConfigLibModSystem>();
        system.SettingChanged += (domain, config, setting) =>
        {
            if (domain != "healthbar") return;
            setting.AssignSettingValue(ModConfig.Instance);
            NotifyChanged();
        };
    }
}
