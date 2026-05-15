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
    private static IConfigProvider Config { get; set; } = NullConfigProvider.Instance;

    public static event Action SettingsChanged;

#nullable enable
    private ConfigProvider? _configProvider;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        Api = api;
        Logger = Mod.Logger;

        try
        {
            _configProvider = new ConfigProvider(api);
            Config = _configProvider;
            _configProvider.Changed += OnConfigChanged;
        }
        catch (Exception e)
        {
            Logger?.Warning($"[HealthBar] Failed to initialize config provider, using defaults: {e}");
            _configProvider = null;
            Config = NullConfigProvider.Instance;
        }
    }

    private static void OnConfigChanged() => SettingsChanged?.Invoke();

    private HealthBarClientSystem? _client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        var config = (IConfigProvider?)_configProvider ?? NullConfigProvider.Instance;
        Config = config;
        _client = new HealthBarClientSystem(api, config);
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
