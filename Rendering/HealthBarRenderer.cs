#nullable enable
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using HealthBar.Config;
using Vintagestory.API.Config;

namespace HealthBar.Rendering
{
    public class HealthBarRenderer : IRenderer, IDisposable
    {
        #region Stałe

        private const float BaseScaleDivider = 4f;
        private const float ScaleBoost       = 2f;
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

            // Skalowanie w zależności od odległości
            float distance   = Math.Max(1f, (float)screen.Z);
            float rawScale   = BaseScaleDivider / distance;
            float finalScale = Math.Max(MinScale, rawScale * ScaleBoost);

            float w = finalScale * _settings.BarWidth;
            float h = finalScale * _settings.BarHeight;
            float x = (float)screen.X - w / 2f;                                               // lewa krawędź ramki
            float y = _api.Render.FrameHeight - (float)screen.Y - h - _settings.VerticalOffset;

            var shader = _api.Render.CurrentActiveShader;
            PrepareShader(shader);

            // ---------- RAMKA ----------
            DrawBar(shader, _backgroundMesh, x, y, w, h);

            // ---------- WYPEŁNIENIE (symetryczne) ----------
            float fillW   = w * pct;                   // nowa szerokość
            float centerX = x + w / 2f;                // środek ramki
            float newX    = centerX - fillW / 2f;      // lewa krawędź wypełnienia

            DrawBar(shader, _healthMesh, newX, y, fillW, h);

            // ---------- TEKST ----------
            DrawHealthText(cur, max, x, y, w, h, finalScale);
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

        #region Tekst zdrowia

        private void DrawHealthText(float current, float max,
                                     float x, float y, float w, float h,
                                     float scale)
        {
            string txt = $"{MathF.Ceiling(current)} / {MathF.Ceiling(max)}";

            var font        = CairoFont.WhiteSmallText();
            double baseSize = font.UnscaledFontsize;

            // startowe powiększenie = skala odległości
            font.UnscaledFontsize = baseSize * scale;
            font.StrokeWidth      = 2.0 * RuntimeEnv.GUIScale * scale;

            font.Color[3]       = _opacity;
            font.StrokeColor    = new double[] { 0, 0, 0, _opacity };

            // pierwsze wygenerowanie
            _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
            float tw = _healthTextTexture.Width;
            float th = _healthTextTexture.Height;

            // pion – maks 90 % wysokości paska
            float maxTextHeight = h * 0.9f;
            if (th > maxTextHeight && th > 0)
            {
                float ratio            = maxTextHeight / th;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;

                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            // poziom – maks 90 % szerokości paska
            float maxTextWidth = w * 0.9f;
            if (tw > maxTextWidth && tw > 0)
            {
                float ratio            = maxTextWidth / tw;
                font.UnscaledFontsize *= ratio;
                font.StrokeWidth      *= ratio;

                _api.Gui.TextTexture.GenOrUpdateTextTexture(txt, font, ref _healthTextTexture);
                tw = _healthTextTexture.Width;
                th = _healthTextTexture.Height;
            }

            float tx = x + (w - tw) / 2f;
            float ty = y + (h - th) / 2f;

            // cień
            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx + 1f, ty + 1f, tw, th,
                ZIndex + 0.9f,
                new Vec4f(0f, 0f, 0f, _opacity * 0.5f)
            );

            // tekst
            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                tx, ty, tw, th,
                ZIndex + 1f,
                new Vec4f(1f, 1f, 1f, _opacity)
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
