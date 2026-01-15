using System;
using System.Runtime.CompilerServices;
using HealthBar.Rendering.Core;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering.Themes;

public sealed class TiledTheme(TiledThemeDefinition def, string id) : IHealthBarTheme
{
    public string Id { get; } = id;

#nullable enable
    private ICoreClientAPI? _capi;
#nullable disable
    private bool _ensured;

    private LoadedTexture _texture = null!;
    private string SamplerName { get; set; } = "tex2d";

    private bool _frame5;
    private bool _bg5;

    private MeshRef _frameL = null!;
    private MeshRef _frameM = null!;
    private MeshRef _frameR = null!;

    private MeshRef _bgL = null!;
    private MeshRef _bgM = null!;
    private MeshRef _bgR = null!;

    private MeshRef _frameLo = null!;
    private MeshRef _frameLi = null!;
    private MeshRef _frameRi = null!;
    private MeshRef _frameRo = null!;

    private MeshRef _bgLo = null!;
    private MeshRef _bgLi = null!;
    private MeshRef _bgRi = null!;
    private MeshRef _bgRo = null!;

    private MeshRef _fillFull = null!;

#nullable enable
    private MeshRef?[] _fillPartials = [];
    private MeshRef? _heart;
#nullable disable

    private readonly Vec4f _white = new(1f, 1f, 1f, 1f);

    private const bool PixelSnapToScreenPixels = true;

    public void EnsureResources(ICoreClientAPI capi)
    {
        if (_ensured) return;
        _ensured = true;
        _capi = capi;

        _texture = new LoadedTexture(capi);

        var assetLoc = new AssetLocation("healthbar", def.Texture);
        var bmp = capi.Assets.Get(assetLoc).ToBitmap(capi);
        capi.Render.LoadTexture(bmp, ref _texture, linearMag: false, clampMode: 0, generateMipmaps: false);
        bmp.Dispose();

        var guiSh = capi.Render.GetEngineShader(EnumShaderProgram.Gui);
        if (guiSh != null)
        {
            if (guiSh.HasUniform("tex2d")) SamplerName = "tex2d";
            else if (guiSh.HasUniform("tex")) SamplerName = "tex";
            else if (guiSh.HasUniform("tex0")) SamplerName = "tex0";
        }

        _frame5 = HasAny5SliceFrame(def.Tiles);
        _bg5 = HasAny5SliceBg(def.Tiles);

        _frameL = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.FrameLeft)));
        _frameM = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.FrameMid)));
        _frameR = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.FrameRight)));

        _bgL = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.BgLeft)));
        _bgM = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.BgMid)));
        _bgR = capi.Render.UploadMesh(CreateTileQuad(ClampTileIndex(def.Tiles.BgRight)));

        if (_frame5)
        {
            var flo = ClampTileIndex(Pick(def.Tiles.FrameLeftOuter, def.Tiles.FrameLeft));
            var fli = ClampTileIndex(Pick(def.Tiles.FrameLeftInner, def.Tiles.FrameLeft));
            var fri = ClampTileIndex(Pick(def.Tiles.FrameRightInner, def.Tiles.FrameRight));
            var fro = ClampTileIndex(Pick(def.Tiles.FrameRightOuter, def.Tiles.FrameRight));

            _frameLo = capi.Render.UploadMesh(CreateTileQuad(flo));
            _frameLi = capi.Render.UploadMesh(CreateTileQuad(fli));
            _frameRi = capi.Render.UploadMesh(CreateTileQuad(fri));
            _frameRo = capi.Render.UploadMesh(CreateTileQuad(fro));
        }

        if (_bg5)
        {
            var blo = ClampTileIndex(Pick(def.Tiles.BgLeftOuter, def.Tiles.BgLeft));
            var bli = ClampTileIndex(Pick(def.Tiles.BgLeftInner, def.Tiles.BgLeft));
            var bri = ClampTileIndex(Pick(def.Tiles.BgRightInner, def.Tiles.BgRight));
            var bro = ClampTileIndex(Pick(def.Tiles.BgRightOuter, def.Tiles.BgRight));

            _bgLo = capi.Render.UploadMesh(CreateTileQuad(blo));
            _bgLi = capi.Render.UploadMesh(CreateTileQuad(bli));
            _bgRi = capi.Render.UploadMesh(CreateTileQuad(bri));
            _bgRo = capi.Render.UploadMesh(CreateTileQuad(bro));
        }

        var fillIdx = ClampTileIndex(def.Tiles.Fill);
        _fillFull = capi.Render.UploadMesh(CreateTileQuad(fillIdx));
#nullable enable
        _fillPartials = new MeshRef?[def.Tile];
#nullable disable
        for (var cut = 1; cut < def.Tile; cut++)
            _fillPartials[cut] = capi.Render.UploadMesh(CreateTileQuad(fillIdx, cutWidth: cut));

        if (!def.Tiles.HasHeart) return;
        var heartIdx = ClampTileIndex(def.Tiles.Heart);
        _heart = capi.Render.UploadMesh(CreateTileQuad(heartIdx));
    }

    public void Render(in BarRenderData data, in ThemeRenderContext ctx)
    {
        if (!_ensured) EnsureResources(ctx.Capi);

        var sh = ctx.Shader;
        var drawer = ctx.Drawer;

        var tilePx = def.Tile * data.Scale;

        var fixedTiles = _frame5 ? 4 : 2;

        float baseUnitsNoMid = def.BarStartOffset + fixedTiles * def.Tile;
        var desiredUnits = MathF.Max(ctx.Config.BarWidth, baseUnitsNoMid + def.MinMidTiles * def.Tile);

        var midTiles = (int)MathF.Ceiling((desiredUnits - baseUnitsNoMid) / def.Tile);
        if (midTiles < def.MinMidTiles) midTiles = def.MinMidTiles;
        if (midTiles > def.MaxMidTiles) midTiles = def.MaxMidTiles;

        float midUnits = midTiles * def.Tile;

        // Anchor: keep same center + top.
        var centerX = data.X + data.Width * 0.5f;
        var yTop = data.Y + data.Height;

        var totalW = (def.BarStartOffset + (fixedTiles * def.Tile) + midUnits) * data.Scale;
        var totalH = tilePx;

        var x = centerX - totalW * 0.5f;
        var y = yTop - totalH;

        var heartX = x;
        var barX = x + def.BarStartOffset * data.Scale;
        var barY = y;

        var barW = (fixedTiles * def.Tile + midUnits) * data.Scale;
        var barH = tilePx;

        var padPx = def.InnerPad * data.Scale;
        var innerX = barX + padPx;
        var innerY = barY + padPx;
        var innerW = MathF.Max(0f, barW - 2f * padPx);
        var innerH = MathF.Max(0f, barH - 2f * padPx);

        var fillW = innerW * data.ShownPercent;

        if (PixelSnapToScreenPixels)
        {
            heartX = MathF.Round(heartX);
            barX = MathF.Round(barX);
            barY = MathF.Round(barY);
            barW = MathF.Round(barW);
            barH = MathF.Round(barH);
            innerX = MathF.Round(innerX);
            innerY = MathF.Round(innerY);
            innerW = MathF.Round(innerW);
            innerH = MathF.Round(innerH);
            fillW = MathF.Round(fillW);
            tilePx = MathF.Round(tilePx);
        }

        sh.Uniform("noTexture", 0f);
        sh.BindTexture2D(SamplerName, _texture.TextureId, 0);

        _white.A = data.Opacity;
        sh.Uniform("rgbaIn", _white);
        if (_bg5)
            DrawRowTiled5(sh, drawer, _bgLo, _bgLi, _bgM, _bgRi, _bgRo, barX, barY, tilePx, midTiles);
        else
            DrawRowTiled3(sh, drawer, _bgL, _bgM, _bgR, barX, barY, tilePx, midTiles);

        if (fillW > 0.5f && innerH > 0.5f)
        {
            sh.Uniform("rgbaIn", HealthBarRenderUtil.HpColorToVec4(data, in ctx.Config));

            var tileWpx = def.Tile * data.Scale;
            if (PixelSnapToScreenPixels) tileWpx = MathF.Round(tileWpx);

            var full = (int)(fillW / tileWpx);
            var rem = fillW - full * tileWpx;

            var fx = innerX;
            for (var i = 0; i < full; i++)
            {
                drawer.Draw(sh, _fillFull, fx, innerY, tileWpx, innerH);
                fx += tileWpx;
            }

            var remPx = (int)MathF.Round((rem / tileWpx) * def.Tile);
            if (remPx > 0)
            {
                if (remPx >= def.Tile) remPx = def.Tile - 1;
                var partialMesh = _fillPartials[remPx];
                if (partialMesh != null)
                    drawer.Draw(sh, partialMesh, fx, innerY, rem, innerH);
            }
        }

        _white.A = data.Opacity;
        sh.Uniform("rgbaIn", _white);
        if (_frame5)
            DrawRowTiled5(sh, drawer, _frameLo, _frameLi, _frameM, _frameRi, _frameRo, barX, barY, tilePx, midTiles);
        else
            DrawRowTiled3(sh, drawer, _frameL, _frameM, _frameR, barX, barY, tilePx, midTiles);

        if (_heart != null)
            drawer.Draw(sh, _heart, heartX, barY, tilePx, tilePx);

        if (ctx.Config.ShowHpText)
            HealthBarRenderUtil.DrawDigitsText(data.CurrentHp, data.MaxHp, innerX, innerY, innerW, innerH, data.Opacity,
                in ctx);
    }

    private static void DrawRowTiled3(IShaderProgram sh, GuiQuadDrawer drawer,
        MeshRef left, MeshRef mid, MeshRef right,
        float x, float y, float tilePx, int midTiles)
    {
        drawer.Draw(sh, left, x, y, tilePx, tilePx);
        var cx = x + tilePx;
        for (var i = 0; i < midTiles; i++)
        {
            drawer.Draw(sh, mid, cx, y, tilePx, tilePx);
            cx += tilePx;
        }

        drawer.Draw(sh, right, cx, y, tilePx, tilePx);
    }

    private static void DrawRowTiled5(IShaderProgram sh, GuiQuadDrawer drawer,
        MeshRef leftOuter, MeshRef leftInner, MeshRef mid, MeshRef rightInner, MeshRef rightOuter,
        float x, float y, float tilePx, int midTiles)
    {
        drawer.Draw(sh, leftOuter, x, y, tilePx, tilePx);
        var cx = x + tilePx;

        drawer.Draw(sh, leftInner, cx, y, tilePx, tilePx);
        cx += tilePx;

        for (var i = 0; i < midTiles; i++)
        {
            drawer.Draw(sh, mid, cx, y, tilePx, tilePx);
            cx += tilePx;
        }

        drawer.Draw(sh, rightInner, cx, y, tilePx, tilePx);
        cx += tilePx;

        drawer.Draw(sh, rightOuter, cx, y, tilePx, tilePx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Pick(int preferred, int fallback) => preferred >= 0 ? preferred : fallback;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Pick(int? preferred, int fallback) =>
        preferred is >= 0 ? preferred.Value : fallback;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasAny5SliceFrame(TiledThemeDefinition.Tileset t)
        => Get(t.FrameLeftOuter) >= 0 || Get(t.FrameLeftInner) >= 0 || Get(t.FrameRightInner) >= 0 ||
           Get(t.FrameRightOuter) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasAny5SliceBg(TiledThemeDefinition.Tileset t)
        => Get(t.BgLeftOuter) >= 0 || Get(t.BgLeftInner) >= 0 || Get(t.BgRightInner) >= 0 || Get(t.BgRightOuter) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Get(int v) => v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Get(int? v) => v ?? -1;

    private int ClampTileIndex(int tileIndex)
    {
        var tilesAcross = def.TexW / def.Tile;
        var max = tilesAcross - 1;

        if ((uint)tileIndex <= (uint)max) return tileIndex;

        ModSystem.Logger?.Warning(
            $"[HealthBar] Theme '{Id}' tile index {tileIndex} is out of range. texW={def.TexW}, tile={def.Tile} -> maxIndex={max}. Clamping.");
        if (tileIndex < 0) return 0;
        return max < 0 ? 0 : max;
    }

    private MeshData CreateTileQuad(int tileIndex, int cutWidth = -1)
    {
        tileIndex = ClampTileIndex(tileIndex);

        var tile = def.Tile;
        if (cutWidth < 0) cutWidth = tile;
        if (cutWidth < 1) cutWidth = 1;
        if (cutWidth > tile) cutWidth = tile;

        var x = tileIndex * tile;

        var inset = def.UvInsetPx;
        if (cutWidth <= 2) inset = 0f;

        var u0 = (x + inset) / def.TexW;
        var u1 = (x + cutWidth - inset) / def.TexW;

        var vTop = (0 + inset) / def.TexH;
        var vBot = (tile - inset) / def.TexH;

        var v1 = 1f - vTop;
        var v0 = 1f - vBot;

        if (!(u1 <= u0))
            return QuadMeshUtil.GetCustomQuadModelData(
                u0, v0, u1, v1,
                dx: -1f, dy: -1f, dw: 2f, dh: 2f,
                r: 255, g: 255, b: 255, a: 255
            );
        u0 = x / (float)def.TexW;
        u1 = (x + cutWidth) / (float)def.TexW;
        v1 = 1f;
        v0 = 0f;

        return QuadMeshUtil.GetCustomQuadModelData(
            u0, v0, u1, v1,
            dx: -1f, dy: -1f, dw: 2f, dh: 2f,
            r: 255, g: 255, b: 255, a: 255
        );
    }

    public void Dispose()
    {
        if (!_ensured || _capi == null) return;

        _capi.Render.DeleteMesh(_frameL);
        _capi.Render.DeleteMesh(_frameM);
        _capi.Render.DeleteMesh(_frameR);

        _capi.Render.DeleteMesh(_bgL);
        _capi.Render.DeleteMesh(_bgM);
        _capi.Render.DeleteMesh(_bgR);

        if (_frame5)
        {
            _capi.Render.DeleteMesh(_frameLo);
            _capi.Render.DeleteMesh(_frameLi);
            _capi.Render.DeleteMesh(_frameRi);
            _capi.Render.DeleteMesh(_frameRo);
        }

        if (_bg5)
        {
            _capi.Render.DeleteMesh(_bgLo);
            _capi.Render.DeleteMesh(_bgLi);
            _capi.Render.DeleteMesh(_bgRi);
            _capi.Render.DeleteMesh(_bgRo);
        }

        _capi.Render.DeleteMesh(_fillFull);
        for (var i = 1; i < _fillPartials.Length; i++)
        {
            if (_fillPartials[i] != null) _capi.Render.DeleteMesh(_fillPartials[i]!);
        }

        if (_heart != null) _capi.Render.DeleteMesh(_heart);

        _texture.Dispose();
        _ensured = false;
        _capi = null;
    }
}