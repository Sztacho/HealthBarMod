using System;

namespace HealthBar.Rendering.Themes;

public interface IThemeRegistry : IDisposable
{
    IHealthBarTheme GetOrCreate(string themeId);
}
