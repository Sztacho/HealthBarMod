using System;
using Vintagestory.API.Client;

namespace HealthBar.Rendering.Themes;

public interface IHealthBarTheme : IDisposable
{
    string Id { get; }

    void EnsureResources(ICoreClientAPI capi);

    void Render(in BarRenderData data, in ThemeRenderContext ctx);
}
