using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering.Text;

public sealed class DigitFont : IDisposable
{
    public const float GlyphW = 6f;
    public const float GlyphH = 8f;

    private const string Map = "0123456789/";
    private const int GlyphCount = 11;
    private const int TexW = (int)(GlyphW * GlyphCount);
    private const int TexH = (int)GlyphH;

    private readonly ICoreClientAPI _capi;
    private readonly MeshRef[] _glyphMeshes = new MeshRef[GlyphCount];

    public LoadedTexture Texture => _texture;
    private LoadedTexture _texture;

    public string SamplerName { get; private set; } = "tex2d";

    public DigitFont(ICoreClientAPI capi)
    {
        _capi = capi;
        _texture = new LoadedTexture(capi);
        Load();
    }

    private void Load()
    {
        var assetLoc = new AssetLocation("healthbar", "textures/gui/digits.png");
        var bmp = _capi.Assets.Get(assetLoc).ToBitmap(_capi);

        _capi.Render.LoadTexture(bmp, ref _texture, linearMag: false, clampMode: 0, generateMipmaps: false);
        bmp.Dispose();

        var guiSh = _capi.Render.GetEngineShader(EnumShaderProgram.Gui);
        if (guiSh != null)
        {
            if (guiSh.HasUniform("tex2d")) SamplerName = "tex2d";
            else if (guiSh.HasUniform("tex")) SamplerName = "tex";
            else if (guiSh.HasUniform("tex0")) SamplerName = "tex0";
        }

        for (var i = 0; i < GlyphCount; i++)
        {
            var u0 = (i * GlyphW) / TexW;
            var u1 = ((i + 1) * GlyphW) / TexW;

            var mesh = QuadMeshUtil.GetCustomQuadModelData(
                u0, 0f, u1, 1f,
                dx: -1f, dy: -1f, dw: 2f, dh: 2f,
                r: 255, g: 255, b: 255, a: 255
            );
            _glyphMeshes[i] = _capi.Render.UploadMesh(mesh);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MeshRef GetMesh(char c)
    {
        var idx = c == '/' ? 10 : (c - '0');
        return (uint)idx < (uint)GlyphCount ? _glyphMeshes[idx] : null;
    }

    public void Dispose()
    {
        foreach (var t in _glyphMeshes)
        {
            if (t != null) _capi.Render.DeleteMesh(t);
        }

        _texture.Dispose();
    }
}
