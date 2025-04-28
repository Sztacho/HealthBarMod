#nullable enable
using System;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering
{
    public class HealthBarRenderer : IRenderer, IDisposable
    {
        #region Stałe

        private const float BaseScaleDivider = 4f;
        private const float ScaleBoost       = 2.0f;  // Powiększenie pasków
        private const float MinScale         = 0.7f;
        private const float ZIndex           = 20f;

        #endregion

        #region Pola prywatne

        private readonly ICoreClientAPI _api;
        private readonly HealthBarSettings _settings;
        private readonly Matrixf _modelMatrix = new();
        private readonly MeshRef _backgroundMesh;
        private readonly MeshRef _healthMesh;
        private LoadedTexture _healthTextTexture;
        private Vec4f _healthColor = new();
        private float _opacity;

        #endregion

        #region Właściwości

        public Entity? TargetEntity { get; set; }
        public bool IsVisible       { get; set; }
        public double RenderOrder   => 0.41;
        public int    RenderRange   => 10;

        #endregion

        #region Konstruktor

        public HealthBarRenderer(ICoreClientAPI api, HealthBarSettings settings)
        {
            _api      = api      ?? throw new ArgumentNullException(nameof(api));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _backgroundMesh    = _api.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
            _healthMesh        = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());
            _healthTextTexture = new LoadedTexture(_api);

            _api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        #endregion

        #region Renderowanie

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (!CanRender()) return;

            var node  = TargetEntity!.WatchedAttributes.GetTreeAttribute("health");
            float cur = node?.GetFloat("currenthealth") ?? 0f;
            float max = node?.GetFloat("maxhealth")     ?? 1f;
            float pct = Math.Clamp(cur / max, 0f, 1f);

            UpdateOpacity(deltaTime);
            UpdateHealthColor(pct);

            var screen = GetEntityScreenPosition();
            if (screen.Z < 0) return;

            // identyczne skalowanie jak dla paska
            float distance   = Math.Max(1f, (float)screen.Z);
            float rawScale   = BaseScaleDivider / distance;
            float finalScale = Math.Max(MinScale, rawScale * ScaleBoost);

            float w = finalScale * _settings.BarWidth;
            float h = finalScale * _settings.BarHeight;
            float x = (float)screen.X - w / 2f;
            float y = _api.Render.FrameHeight - (float)screen.Y - h - _settings.VerticalOffset;

            var shader = _api.Render.CurrentActiveShader;
            PrepareShader(shader);

            DrawBar(shader, _backgroundMesh, x, y, w, h);      // ramka
            DrawBar(shader, _healthMesh,     x, y, w * pct, h); // wypełnienie
            DrawHealthText(cur, max, x, y, w, h, finalScale);   // tekst ⬅⬅⬅ skalowany
        }

        private bool CanRender()
        {
            if (TargetEntity == null) return false;
            var node = TargetEntity.WatchedAttributes.GetTreeAttribute("health");
            if (node == null) return false;

            float curHealth = node.GetFloat("currenthealth");
            return curHealth > 0f && (_opacity > 0f || IsVisible);
        }

        #endregion

        #region Przygotowanie shader’a

        private void PrepareShader(IShaderProgram shader)
        {
            shader.Uniform("rgbaIn", _healthColor);
            shader.Uniform("noTexture", 1f);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);
        }

        #endregion

        #region Rysowanie paska

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

        #region Aktualizacja stanu

        private void UpdateOpacity(float dt)
        {
            float delta = dt / (IsVisible ? _settings.FadeInSpeed : -_settings.FadeOutSpeed);
            _opacity = Math.Clamp(_opacity + delta, 0f, 1f);
        }

        private void UpdateHealthColor(float percent)
        {
            string hex = percent <= _settings.LowHealthThreshold
                ? _settings.LowHealthColor
                : percent <= _settings.MidHealthThreshold
                    ? _settings.MidHealthColor
                    : _settings.FullHealthColor;

            ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(hex), ref _healthColor);
            _healthColor.A = _opacity;
        }

        #endregion

        #region Tekst zdrowia  (skalowany!)

        private void DrawHealthText(float current, float max,
            float x, float y, float w, float h,
            float scale)
        {
            string txt  = $"{MathF.Ceiling(current)} / {MathF.Ceiling(max)}";

            // --- 1. Ustaw bazowy font ---
            var font          = CairoFont.WhiteSmallText();
            double baseSize   = font.UnscaledFontsize;

            // Skalowanie z odległości
            font.UnscaledFontsize = baseSize * scale;
            font.StrokeWidth      = 2.0 * RuntimeEnv.GUIScale * scale;

            font.Color[3]       = _opacity;
            font.StrokeColor    = new double[] { 0, 0, 0, _opacity };

            // --- 2. Pierwsze generowanie tekstury ---
            _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);

            float tw = _healthTextTexture.Width;
            float th = _healthTextTexture.Height;

            // --- 3. Dopasowanie do paska w pionie ---
            float maxTextHeight = h * 0.9f;          // 90 % wysokości paska
            if (th > maxTextHeight && th > 0)
            {
                float ratio = maxTextHeight / th;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;

                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            // --- 4. Dopasowanie do paska w poziomie ---
            float maxTextWidth = w * 0.9f;
            if (tw > maxTextWidth && tw > 0)
            {
                float ratio = maxTextWidth / tw;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;

                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            // --- 5. Wyśrodkowanie i render ---
            float tx = x + (w - tw) / 2f;
            float ty = y + (h - th) / 2f;

            // cień
            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx + 1f, ty + 1f, tw, th,
                ZIndex + 0.9f,
                new Vec4f(0, 0, 0, _opacity * 0.5f)
            );

            // tekst
            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx, ty, tw, th,
                ZIndex + 1f,
                new Vec4f(1, 1, 1, _opacity)
            );
        }


        #endregion

        #region Pozycjonowanie ekranu

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
            _api.Render.DeleteMesh(_backgroundMesh);
            _api.Render.DeleteMesh(_healthMesh);
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}