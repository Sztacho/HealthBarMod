using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar.Rendering.Themes;

public sealed class ThemeRegistry : IThemeRegistry
{
    private readonly ICoreClientAPI _capi;
    private readonly Dictionary<string, IHealthBarTheme> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ThemeRegistry(ICoreClientAPI capi)
    {
        _capi = capi;
        _cache["basic"] = new BasicTheme();
    }

    public IHealthBarTheme GetOrCreate(string themeId)
    {
        var id = (themeId ?? "basic").Trim();
        if (id.Length == 0) id = "basic";

        if (_cache.TryGetValue(id, out var existing)) return existing;

        if (id.Equals("pixel", StringComparison.OrdinalIgnoreCase))
            id = "pixel";

        var theme = TryLoadTiledTheme(id) ?? _cache["basic"];
        _cache[id] = theme;
        return theme;
    }

    #nullable enable
    private IHealthBarTheme? TryLoadTiledTheme(string id)
    {
        try
        {
            var loc = new AssetLocation("healthbar", $"config/themes/{id}");
            if (!_capi.Assets.Exists(loc))
                loc = new AssetLocation("healthbar", $"config/themes/{id}.json");

            ModSystem.Logger.Debug($"[HealthBar] Loading tiled theme '{id}' from {loc}");

            var asset = _capi.Assets.Get(loc);
            if (asset == null) return null;

            var json = asset.ToText();
            if (string.IsNullOrWhiteSpace(json)) return null;

            var def = TiledThemeDefinition.FromJson(json);
            return new TiledTheme(def, id);
        }
        catch (Exception e)
        {
            ModSystem.Logger?.Warning($"[HealthBar] Failed to load theme '{id}': {e}");
            return null;
        }
    }


    public void Dispose()
    {
        foreach (var kv in _cache)
            kv.Value.Dispose();
        _cache.Clear();
    }
}