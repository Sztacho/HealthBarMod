using HealthBar.Config;
using HealthBar.Rendering.Core;
using HealthBar.Rendering.Text;
using Vintagestory.API.Client;

namespace HealthBar.Rendering.Themes;

public readonly struct ThemeRenderContext(
    ICoreClientAPI capi,
    IShaderProgram shader,
    GuiQuadDrawer drawer,
    DigitFont digits,
    in ModConfigSnapshot cfg)
{
    public readonly ICoreClientAPI Capi = capi;
    public readonly IShaderProgram Shader = shader;
    public readonly GuiQuadDrawer Drawer = drawer;
    public readonly DigitFont Digits = digits;
    public readonly ModConfigSnapshot Config = cfg;
}
