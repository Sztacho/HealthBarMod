#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering
{
    public sealed class HealthBarRenderer : IRenderer, IDisposable
    {
        private const float BaseScaleDivider = 4f;
        private const float ScaleBoost = 2f;
        private const float MinScale = 0.7f;
        private const float Z = 20f;

        private const float FillLerpSpeed = 6f;
        private const float BorderPx = 1f;

        private readonly ICoreClientAPI _capi;
        private readonly HealthBarSettings _set;
        private readonly Matrixf _model = new();

        private readonly MeshRef _borderM, _backM, _fillM;
        private readonly Vec4f _frameCol = new(), _backCol = new(0, 0, 0, 0.6f);
        private Vec4f _hpCol = new();
        
        private static readonly Stack<LoadedTexture> LoadedTexturePool = new();
        private LoadedTexture _txtTex;

        private readonly CairoFont _font = CairoFont.WhiteSmallText();
        private int _cachedHash = 0;
        private string _cachedText = "";
        private float _opacity;
        private float _shownPct = 1f;
        private bool _firstShow = true;

        public Entity? TargetEntity { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFullyInvisible => _opacity <= 0.001f;

        public double RenderOrder => 0.41;
        public int RenderRange => 10;

        public HealthBarRenderer(ICoreClientAPI api, HealthBarSettings settings)
        {
            _capi = api;
            _set = settings;
            
            _borderM = api.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
            _backM = api.Render.UploadMesh(QuadMeshUtil.GetQuad());
            _fillM = api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(_set.FrameColor ?? "#CCCCCC"), ref _frameCol);

            _txtTex = LoadedTexturePool.TryPop(out var pooled) ? pooled : new LoadedTexture(api);
            api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        public void Dispose()
        {
            _capi.Render.DeleteMesh(_borderM);
            _capi.Render.DeleteMesh(_backM);
            _capi.Render.DeleteMesh(_fillM);
            _capi.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);

            _txtTex?.Dispose();
            GC.SuppressFinalize(this);
        }
        
        public void OnRenderFrame(float dt, EnumRenderStage stage)
        {
            if (!CanRender()) return;

            var hpNode = TargetEntity!.WatchedAttributes.GetTreeAttribute("health");
            var cur = hpNode?.GetFloat("currenthealth") ?? 0f;
            var max = MathF.Max(1f, hpNode?.GetFloat("maxhealth") ?? 1f);
            var pct = Clamp01(cur / max);

            if (_firstShow)
            {
                _shownPct = pct;
                _firstShow = false;
            }
            else if (pct < _shownPct)
            {
                _shownPct = Lerp(_shownPct, pct, Clamp01(dt * FillLerpSpeed));
            }
            else _shownPct = pct;

            _opacity = Clamp01(_opacity + dt / (IsVisible ? _set.FadeInSpeed : -_set.FadeOutSpeed));
            if (_opacity <= 0) return; // całkiem niewidoczny

            UpdateHealthColor(_shownPct);
            _frameCol.A = _backCol.A = _opacity;

            var scr = ProjectOnScreen();
            if (scr.Z < 0) return;

            var distScale = BaseScaleDivider / MathF.Max(1f, (float)scr.Z);
            var scale = MathF.Max(MinScale, distScale * ScaleBoost);

            var w = scale * _set.BarWidth;
            var h = scale * _set.BarHeight;
            var x = (float)scr.X - w / 2f;
            var y = _capi.Render.FrameHeight - (float)scr.Y - h - _set.VerticalOffset;

            var sh = _capi.Render.CurrentActiveShader;
            sh.Uniform("noTexture", 1f);
            sh.UniformMatrix("projectionMatrix", _capi.Render.CurrentProjectionMatrix);

            var bp = BorderPx * RuntimeEnv.GUIScale;
            sh.Uniform("rgbaIn", _frameCol);
            DrawQuad(sh, _borderM, x - bp, y - bp, w + bp * 2, h + bp * 2);
            sh.Uniform("rgbaIn", _backCol);
            DrawQuad(sh, _backM, x, y, w, h);
            sh.Uniform("rgbaIn", _hpCol);
            DrawQuad(sh, _fillM, x + (w - _shownPct * w) / 2f, y, _shownPct * w, h);
            if(_set.ShowHpText)
                DrawText(cur, max, x, y, w, h, scale);
        }
        
        private bool CanRender()
        {
            var node = TargetEntity?.WatchedAttributes.GetTreeAttribute("health");
            return node != null && node.GetFloat("currenthealth") > 0 && (_opacity > 0 || IsVisible);
        }

        private void UpdateHealthColor(float pct)
        {
            var hex = pct <= _set.LowHealthThreshold ? _set.LowHealthColor
                : pct <= _set.MidHealthThreshold ? _set.MidHealthColor
                : _set.FullHealthColor;

            ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(hex), ref _hpCol);
            _hpCol.A = _opacity;
        }

        private void DrawQuad(IShaderProgram sh, MeshRef mesh,
            float x, float y, float w, float h)
        {
            _model.Set(_capi.Render.CurrentModelviewMatrix)
                .Translate(x, y, Z)
                .Scale(w, h, 0f)
                .Translate(.5f, .5f, 0f)
                .Scale(.5f, .5f, 0f);

            sh.UniformMatrix("modelViewMatrix", _model.Values);
            _capi.Render.RenderMesh(mesh);
        }


        private void DrawText(float cur, float max, float x, float y, float w, float h, float scale)
        {
            var sizeBucket = (int)(scale * 32);
            var txt = $"{MathF.Ceiling(cur)}/{MathF.Ceiling(max)}";
            var hash = HashCode.Combine(txt, sizeBucket);

            if (hash != _cachedHash)
            {
                _cachedHash = hash;
                _cachedText = txt;

                _font.Color[3] = _opacity;
                _font.StrokeColor = new double[] { 0, 0, 0, _opacity };
                _font.UnscaledFontsize = 10 * scale;
                _font.StrokeWidth = 2.0 * RuntimeEnv.GUIScale;

                for (var i = 0; i < 2; i++)
                {
                    _capi.Gui.TextTexture.GenOrUpdateTextTexture(txt, _font, ref _txtTex);
                    var rw = (w * 0.9f) / _txtTex.Width;
                    var rh = (h * 0.9f) / _txtTex.Height;
                    var s = MathF.Min(rw, rh);
                    if (s >= 1f) break;
                    _font.UnscaledFontsize *= MathF.Sqrt(s);
                }

                _capi.Gui.TextTexture.GenOrUpdateTextTexture(txt, _font, ref _txtTex);
            }

            if (_opacity < 0.01f) return;
            
            var tx = x + (w - _txtTex.Width) / 2f;
            var ty = y + (h - _txtTex.Height) / 2f;

            _capi.Render.Render2DTexturePremultipliedAlpha(
                _txtTex.TextureId, tx + 1, ty + 1, _txtTex.Width, _txtTex.Height,
                Z + 0.9f, new Vec4f(0, 0, 0, _opacity * 0.5f));

            _capi.Render.Render2DTexturePremultipliedAlpha(
                _txtTex.TextureId, tx, ty, _txtTex.Width, _txtTex.Height,
                Z + 1f, new Vec4f(1, 1, 1, _opacity));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp01(float v) => v <= 0f ? 0f : (v >= 1f ? 1f : v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private Vec3d ProjectOnScreen()
        {
            var e = TargetEntity!;
            var p = new Vec3d(e.Pos.X,
                e.Pos.Y + e.CollisionBox.Y2,
                e.Pos.Z);
            p.Add(e.CollisionBox.X2 - e.OriginCollisionBox.X2, 0, 0);

            return MatrixToolsd.Project(
                p,
                _capi.Render.PerspectiveProjectionMat,
                _capi.Render.PerspectiveViewMat,
                _capi.Render.FrameWidth,
                _capi.Render.FrameHeight);
        }
    }
}