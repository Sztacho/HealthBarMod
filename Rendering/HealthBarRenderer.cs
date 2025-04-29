#nullable enable
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using HealthBar.Config;
using Vintagestory.API.Config;

namespace HealthBar.Rendering
{
    /// <summary>A health bar with a dark background and an independent frame-outline.</summary>
    public class HealthBarRenderer : IRenderer, IDisposable
    {
        #region Constante

        private const float BaseScaleDivider = 4f;
        private const float ScaleBoost       = 2f;
        private const float MinScale         = 0.7f;
        private const float ZIndex           = 20f;

        private const float FillLerpSpeed = 6f;
        private const float BorderPx      = 1f;

        #endregion

        #region Private Fields

        private readonly ICoreClientAPI _api;
        private readonly HealthBarSettings _settings;
        private readonly Matrixf _modelMatrix = new();

        private readonly MeshRef _borderMesh;
        private readonly MeshRef _backgroundMesh;
        private readonly MeshRef _healthMesh;

        private LoadedTexture _healthTextTexture;
        private Vec4f _healthColor = new();
        private readonly Vec4f _frameColor  = new();
        private readonly Vec4f _backColor   = new(0, 0, 0, 0.6f);

        private float _opacity;
        private float _displayPct        = 1f;
        private bool  _justBecameVisible = true;

        #endregion

        #region Parameters

        public Entity? TargetEntity { get; set; }
        public bool IsVisible       { get; set; }
        public double RenderOrder   => 0.41;
        public int    RenderRange   => 10;
        
        public bool IsFullyInvisible => _opacity <= 0.001f;

        #endregion

        #region Contructor

        public HealthBarRenderer(ICoreClientAPI api, HealthBarSettings settings)
        {
            _api      = api      ?? throw new ArgumentNullException(nameof(api));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _borderMesh     = _api.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
            _backgroundMesh = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());
            _healthMesh     = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            ColorUtil.ToRGBAVec4f(
                ColorUtil.Hex2Int(_settings.FrameColor ?? "#999999"),
                ref _frameColor);

            _healthTextTexture = new LoadedTexture(_api);
            _api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        #endregion

        #region Rendering

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (!CanRender()) return;

            var node        = TargetEntity!.WatchedAttributes.GetTreeAttribute("health");
            var curHealth = node?.GetFloat("currenthealth") ?? 0f;
            var maxHealth = node?.GetFloat("maxhealth")     ?? 1f;
            var targetPct = Clamp01(curHealth / maxHealth);

            if (_justBecameVisible)
            {
                _displayPct        = targetPct;
                _justBecameVisible = false;
            }
            else if (targetPct < _displayPct)
            {
                var lerpStep  = Clamp01(deltaTime * FillLerpSpeed);
                _displayPct     = Lerp(_displayPct, targetPct, lerpStep);
            }
            else
            {
                _displayPct = targetPct;
            }

            UpdateOpacity(deltaTime);
            UpdateHealthColor(_displayPct);

            _frameColor.A = _backColor.A = _opacity;

            var screen = GetEntityScreenPosition();
            if (screen.Z < 0) return;

            var distance   = Math.Max(1f, (float)screen.Z);
            var rawScale   = BaseScaleDivider / distance;
            var finalScale = Math.Max(MinScale, rawScale * ScaleBoost);

            var w = finalScale * _settings.BarWidth;
            var h = finalScale * _settings.BarHeight;
            var x = (float)screen.X - w / 2f;
            var y = _api.Render.FrameHeight - (float)screen.Y - h - _settings.VerticalOffset;

            var shader = _api.Render.CurrentActiveShader;
            shader.Uniform("noTexture", 1f);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);

            var bp = BorderPx * RuntimeEnv.GUIScale;
            shader.Uniform("rgbaIn", _frameColor);
            DrawBar(shader, _borderMesh, x - bp, y - bp, w + bp * 2, h + bp * 2);

            shader.Uniform("rgbaIn", _backColor);
            DrawBar(shader, _backgroundMesh, x, y, w, h);

            shader.Uniform("rgbaIn", _healthColor);
            var fillW   = w * _displayPct;
            var centerX = x + w / 2f;
            var newX    = centerX - fillW / 2f;
            DrawBar(shader, _healthMesh, newX, y, fillW, h);

            DrawHealthText(curHealth, maxHealth, x, y, w, h, finalScale);
        }

        private bool CanRender()
        {
            var node = TargetEntity?.WatchedAttributes.GetTreeAttribute("health");
            if (node == null) return false;

            var curHealth = node.GetFloat("currenthealth");
            return curHealth > 0f && (_opacity > 0f || IsVisible);
        }

        #endregion

        #region Update opacity and color

        private void UpdateOpacity(float dt)
        {
            var wasInvisible = _opacity == 0f;
            var delta       = dt / (IsVisible ? _settings.FadeInSpeed : -_settings.FadeOutSpeed);
            _opacity          = Clamp01(_opacity + delta);

            if (wasInvisible && _opacity > 0f)
                _justBecameVisible = true;
        }

        private void UpdateHealthColor(float percent)
        {
            var hex = percent <= _settings.LowHealthThreshold
                ? _settings.LowHealthColor
                : percent <= _settings.MidHealthThreshold
                    ? _settings.MidHealthColor
                    : _settings.FullHealthColor;

            ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(hex), ref _healthColor);
            _healthColor.A = _opacity;
        }

        #endregion

        #region Draw mesh

        private void DrawBar(IShaderProgram shader, MeshRef mesh, float x, float y, float w, float h)
        {
            _modelMatrix
                .Set(_api.Render.CurrentModelviewMatrix)
                .Translate(x, y, ZIndex)
                .Scale(w, h, 0f)
                .Translate(0.5f, 0.5f, 0f)
                .Scale(0.5f, 0.5f, 0f);

            shader.UniformMatrix("modelViewMatrix", _modelMatrix.Values);
            _api.Render.RenderMesh(mesh);
        }

        #endregion

        #region Draw text

        private void DrawHealthText(float current, float max,
                                    float x, float y, float w, float h,
                                    float scale)
        {
            var txt = $"{MathF.Ceiling(current)} / {MathF.Ceiling(max)}";

            var font        = CairoFont.WhiteSmallText();
            var baseSize = font.UnscaledFontsize;

            font.UnscaledFontsize = baseSize * scale;
            font.StrokeWidth      = 2.0 * RuntimeEnv.GUIScale * scale;

            font.Color[3]       = _opacity;
            font.StrokeColor    = new double[] { 0, 0, 0, _opacity };

            _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
            float tw = _healthTextTexture.Width;
            float th = _healthTextTexture.Height;

            var maxTextHeight = h * 0.9f;
            if (th > maxTextHeight && th > 0)
            {
                var ratio            = maxTextHeight / th;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;
                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            var maxTextWidth = w * 0.9f;
            if (tw > maxTextWidth && tw > 0)
            {
                var ratio            = maxTextWidth / tw;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;
                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            var tx = x + (w - tw) / 2f;
            var ty = y + (h - th) / 2f;

            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx + 1f, ty + 1f, tw, th,
                ZIndex + 0.9f,
                new Vec4f(0, 0, 0, _opacity * 0.5f)
            );

            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx, ty, tw, th,
                ZIndex + 1f,
                new Vec4f(1, 1, 1, _opacity)
            );
        }

        #endregion

        #region Screen position

        private Vec3d GetEntityScreenPosition()
        {
            var pos = new Vec3d(
                TargetEntity!.Pos.X,
                TargetEntity.Pos.Y + TargetEntity.CollisionBox.Y2,
                TargetEntity.Pos.Z
            );
            pos.Add(TargetEntity.CollisionBox.X2 - TargetEntity.OriginCollisionBox.X2, 0, 0);

            return MatrixToolsd.Project(
                pos,
                _api.Render.PerspectiveProjectionMat,
                _api.Render.PerspectiveViewMat,
                _api.Render.FrameWidth,
                _api.Render.FrameHeight
            );
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _api.Render.DeleteMesh(_borderMesh);
            _api.Render.DeleteMesh(_backgroundMesh);
            _api.Render.DeleteMesh(_healthMesh);
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
            _healthTextTexture?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Help method Clamp/Lerp

        private static float Clamp01(float v)  => v < 0f ? 0f : v > 1f ? 1f : v;

        private static float Lerp(float a, float b, float t)
        {
            t = t switch
            {
                < 0f => 0f,
                > 1f => 1f,
                _ => t
            };
            return a + (b - a) * t;
        }

        #endregion
    }
}
