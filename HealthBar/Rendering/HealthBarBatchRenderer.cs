using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HealthBar.Client;
using HealthBar.Config;
using HealthBar.Rendering.Core;
using HealthBar.Rendering.Text;
using HealthBar.Rendering.Themes;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering;

public sealed class HealthBarBatchRenderer : IRenderer, IDisposable
{
    private const float BaseScaleDivider = 4f;
    private const float FillLerpSpeed = 6f;

    private readonly ICoreClientAPI _capi;
    private readonly IConfigProvider _config;
    private readonly Dictionary<long, BarState> _states;

    private readonly GuiQuadDrawer _drawer;
    private readonly DigitFont _digits;
    private readonly IThemeRegistry _themes;

    private string _currentTheme = "";
    private IHealthBarTheme _current;

    public double RenderOrder => 0.41;
    public int RenderRange => 10;

    public HealthBarBatchRenderer(ICoreClientAPI capi, IConfigProvider config, Dictionary<long, BarState> states)
    {
        _capi = capi;
        _config = config;
        _states = states;

        _drawer = new GuiQuadDrawer(capi);
        _digits = new DigitFont(capi);
        _themes = new ThemeRegistry(capi);
        _current = _themes.GetOrCreate("basic");
        _current.EnsureResources(capi);
    }

    public void InvalidateTheme() => _currentTheme = "";

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        var cfg = _config.Snapshot;
        if (!cfg.Enabled) return;

        EnsureTheme(cfg.ThemeId);

        var sh = _capi.Render.CurrentActiveShader;
        if (sh == null) return;

        sh.UniformMatrix("projectionMatrix", _capi.Render.CurrentProjectionMatrix);

        var ctx = new ThemeRenderContext(_capi, sh, _drawer, _digits, cfg);

        foreach (var kv in _states)
        {
            var st = kv.Value;
            var ent = st.Entity;
            if (ent is not { Alive: true }) continue;

            st.Opacity = Clamp01(st.Opacity + dt / (st.VisibleThisFrame ? cfg.FadeInSpeed : -cfg.FadeOutSpeed));
            if (st.Opacity <= 0.001f && !st.VisibleThisFrame) continue;

            var hpNode = ent.WatchedAttributes.GetTreeAttribute("health");
            var currentHp = hpNode?.GetFloat("currenthealth") ?? 0f;
            var maxHp = MathF.Max(1f, hpNode?.GetFloat("maxhealth") ?? 1f);
            var percentHp = Clamp01(currentHp / maxHp);

            if (st.FirstShow)
            {
                st.ShownPercent = percentHp;
                st.FirstShow = false;
            }
            else if (percentHp < st.ShownPercent)
            {
                st.ShownPercent = Lerp(st.ShownPercent, percentHp, Clamp01(dt * FillLerpSpeed));
            }
            else
            {
                st.ShownPercent = percentHp;
            }

            var scr = ProjectOnScreen(ent);
            if (scr.Z < 0) continue;

            var distance = MathF.Max(1f, (float)scr.Z);
            var distScale = BaseScaleDivider / distance;

            var minScale = cfg.MinScale <= cfg.MaxScale ? cfg.MinScale : cfg.MaxScale;
            var scaleBoost = cfg.MaxScale / BaseScaleDivider;
            var scale = Math.Clamp(distScale * scaleBoost, minScale, cfg.MaxScale);

            var minOffsetScale = cfg.MinOffsetScale <= cfg.MaxOffsetScale ? cfg.MinOffsetScale : cfg.MaxOffsetScale;
            var offsetBoost = cfg.MaxOffsetScale / BaseScaleDivider;
            var offsetScale = Math.Clamp(distScale * offsetBoost, minOffsetScale, cfg.MaxOffsetScale);
            var scaledOffset = cfg.VerticalOffset * offsetScale;

            var w = scale * cfg.BarWidth;
            var h = scale * cfg.BarHeight;
            var x = (float)scr.X - w * 0.5f;
            var y = _capi.Render.FrameHeight - (float)scr.Y - h - scaledOffset;

            var data = new BarRenderData(ent, currentHp, maxHp, percentHp, st.ShownPercent, x, y, w, h, scale, st.Opacity, st.IsTarget);
            _current.Render(in data, in ctx);
        }
    }

    private void EnsureTheme(string id)
    {
        var themeId = string.IsNullOrWhiteSpace(id) ? "basic" : id;
        if (themeId.Equals(_currentTheme, StringComparison.OrdinalIgnoreCase)) return;

        _currentTheme = themeId;
        _current = _themes.GetOrCreate(themeId);
        _current.EnsureResources(_capi);
    }

    private Vec3d ProjectOnScreen(Entity e)
    {
        double heightRef = e.SelectionBox?.Y2 ?? e.CollisionBox.Y2;
        var p = new Vec3d(e.Pos.X, e.Pos.Y + heightRef, e.Pos.Z);
        p.Add(e.CollisionBox.X2 - e.OriginCollisionBox.X2, 0, 0);

        return MatrixToolsd.Project(
            p,
            _capi.Render.PerspectiveProjectionMat,
            _capi.Render.PerspectiveViewMat,
            _capi.Render.FrameWidth,
            _capi.Render.FrameHeight);
    }

    public void Dispose()
    {
        _themes.Dispose();
        _digits.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Clamp01(float v) => v <= 0f ? 0f : (v >= 1f ? 1f : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
