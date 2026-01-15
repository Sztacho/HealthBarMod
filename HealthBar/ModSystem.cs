using System;
using HealthBar.Client;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar;

public class ModSystem : Vintagestory.API.Common.ModSystem
{
    public static ILogger Logger { get; private set; }
    public static ICoreAPI Api { get; private set; }
    private static IConfigProvider Config { get; set; } = null!;

    public static event Action SettingsChanged;

#nullable enable
    private ConfigProvider? _configProvider;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        Api = api;
        Logger = Mod.Logger;

        _configProvider = new ConfigProvider(api);
        Config = _configProvider;
        _configProvider.Changed += OnConfigChanged;
    }

    private static void OnConfigChanged() => SettingsChanged?.Invoke();

    private HealthBarClientSystem? _client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        _client = new HealthBarClientSystem(api, Config);
    }

    public override void Dispose()
    {
        _client?.Dispose();
        _client = null;
        if (_configProvider != null)
        {
            _configProvider.Changed -= OnConfigChanged;
            _configProvider = null;
        }

        base.Dispose();
    }
}