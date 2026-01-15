using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering.Themes;

public sealed class BasicTheme : IHealthBarTheme
{
    private const float BorderPx = 1f;

    public string Id => "basic";

#nullable enable
    private ICoreClientAPI? _capi;
    private MeshRef? _border;
    private MeshRef? _quad;
#nullable disable

    private Vec4f _frame = new();
    private readonly Vec4f _background = new(0f, 0f, 0f, 0.6f);

    public void EnsureResources(ICoreClientAPI capi)
    {
        _capi = capi;
        _border ??= capi.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
        _quad ??= capi.Render.UploadMesh(QuadMeshUtil.GetQuad());
    }

    public void Render(in BarRenderData data, in ThemeRenderContext ctx)
    {
        var sh = ctx.Shader;
        var drawer = ctx.Drawer;

        var bp = BorderPx * RuntimeEnv.GUIScale;

        sh.Uniform("noTexture", 1f);

        HealthBarRenderUtil.ToRgbVec4(ctx.Config.FrameColorArgb, data.Opacity, ref _frame);
        sh.Uniform("rgbaIn", _frame);
        drawer.Draw(sh, _border!, data.X - bp, data.Y - bp, data.Width + bp * 2f, data.Height + bp * 2f);

        _background.A = 0.6f * data.Opacity;
        sh.Uniform("rgbaIn", _background);
        drawer.Draw(sh, _quad!, data.X, data.Y, data.Width, data.Height);

        sh.Uniform("rgbaIn", HealthBarRenderUtil.HpColorToVec4(data, in ctx.Config));
        var fw = data.ShownPercent * data.Width;
        drawer.Draw(sh, _quad!, data.X + (data.Width - fw) * 0.5f, data.Y, fw, data.Height);

        if (ctx.Config.ShowHpText)
            HealthBarRenderUtil.DrawDigitsText(data.CurrentHp, data.MaxHp, data.X, data.Y, data.Width, data.Height,
                data.Opacity, in ctx);
    }

    public void Dispose()
    {
        if (_capi == null) return;
        if (_border != null) _capi.Render.DeleteMesh(_border);
        if (_quad != null) _capi.Render.DeleteMesh(_quad);
        _border = null;
        _quad = null;
        _capi = null;
    }
}