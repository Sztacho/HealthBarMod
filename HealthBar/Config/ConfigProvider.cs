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
        ModConfig.Instance ??= new ModConfig();
        Snapshot = new ModConfigSnapshot(ModConfig.Instance);
        Changed?.Invoke();
    }

    private void HookConfigLibIfPresent()
    {
        if (!_api.ModLoader.IsModEnabled("configlib")) return;

        try
        {
            var system = _api.ModLoader.GetModSystem<ConfigLibModSystem>();
            if (system == null)
            {
                ModSystem.Logger?.Warning("[HealthBar] configlib is enabled but its mod system is unavailable. Falling back to local JSON config only.");
                return;
            }

            system.SettingChanged += (domain, config, setting) =>
            {
                if (domain != "healthbar") return;
                ModConfig.Instance ??= new ModConfig();
                setting.AssignSettingValue(ModConfig.Instance);
                NotifyChanged();
            };
        }
        catch (Exception e)
        {
            ModSystem.Logger?.Warning($"[HealthBar] Failed to hook configlib, using local JSON config fallback: {e}");
        }
    }
}
