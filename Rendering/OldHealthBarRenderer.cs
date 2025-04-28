using HealthBar.Config;
using System;
using System.Diagnostics;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering
{
    public class OldHealthBarRenderer : IRenderer, IDisposable
    {
        private readonly ICoreClientAPI _api;
        private readonly HealthBarSettings _settings;
        private readonly Matrixf _matrix = new();
        private readonly MeshRef? _backgroundMesh;
        private readonly MeshRef? _healthMesh;
        private Vec4f _healthColor = new();
        private float _opacity;
        private LoadedTexture _healthTextTexture; // 🔥 Usunięto readonly!

        public Entity? TargetEntity { get; set; }
        public bool IsVisible { get; set; }

        public double RenderOrder => 0.41;
        public int RenderRange => 10;

        public OldHealthBarRenderer(ICoreClientAPI api, HealthBarSettings settings)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _backgroundMesh = _api.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
            _healthMesh = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            _api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
            _healthTextTexture = new LoadedTexture(api);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (!ShouldRender()) return;

            var healthData = TargetEntity?.WatchedAttributes.GetTreeAttribute("health");
            Debug.Assert(healthData != null, nameof(healthData) + " != null");
            
            float currentHealth = healthData.GetFloat("currenthealth");
            float maxHealth = healthData.GetFloat("maxhealth");
            float healthPercent = currentHealth / maxHealth;

            UpdateOpacity(deltaTime);
            UpdateHealthBarColor(healthPercent);

            var entityPosition = CalculateEntityScreenPosition();
            if (entityPosition.Z < 0) return;

            float scale = 4f / Math.Max(1f, (float)entityPosition.Z);
            float adjustedScale = AdjustScale(scale);

            float barWidth = adjustedScale * _settings.BarWidth;
            float barHeight = adjustedScale * _settings.BarHeight;
            float posX = (float)entityPosition.X - barWidth / 2;
            float posY = _api.Render.FrameHeight - (float)entityPosition.Y - barHeight - _settings.VerticalOffset;

            var shader = _api.Render.CurrentActiveShader;
            shader.Uniform("rgbaIn", _healthColor);
            shader.Uniform("noTexture", 1f);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);

            RenderMesh(_backgroundMesh, shader, posX, posY, barWidth, barHeight);
            RenderMesh(_healthMesh, shader, posX, posY, barWidth * healthPercent, barHeight);

            RenderHealthText(currentHealth, maxHealth, posX, posY, barWidth, barHeight);
        }

        private bool ShouldRender()
        {
            return TargetEntity != null
                && TargetEntity.WatchedAttributes.HasAttribute("health")
                && (_opacity > 0 || IsVisible);
        }

        private void UpdateOpacity(float deltaTime)
        {
            float change = deltaTime / (IsVisible ? _settings.FadeInSpeed : -_settings.FadeOutSpeed);
            _opacity = Math.Clamp(_opacity + change, 0f, 1f);
        }

        private void UpdateHealthBarColor(float healthPercent)
        {
            string colorHex = healthPercent <= _settings.LowHealthThreshold
                ? _settings.LowHealthColor
                : (healthPercent <= _settings.MidHealthThreshold
                    ? _settings.MidHealthColor
                    : _settings.FullHealthColor);

            ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(colorHex), ref _healthColor);
            _healthColor.A = _opacity;
        }

        private Vec3d CalculateEntityScreenPosition()
        {
            var entityPos = new Vec3d(
                TargetEntity.Pos.X,
                TargetEntity.Pos.Y + TargetEntity.CollisionBox.Y2,
                TargetEntity.Pos.Z);

            entityPos.Add(
                TargetEntity.CollisionBox.X2 - TargetEntity.OriginCollisionBox.X2,
                0,
                TargetEntity.CollisionBox.Z2 - TargetEntity.OriginCollisionBox.Z2);

            return MatrixToolsd.Project(
                entityPos,
                _api.Render.PerspectiveProjectionMat,
                _api.Render.PerspectiveViewMat,
                _api.Render.FrameWidth,
                _api.Render.FrameHeight);
        }

        private static float AdjustScale(float scale)
        {
            float adjustedScale = Math.Min(1f, scale);
            return adjustedScale > 0.75f ? 0.75f + (adjustedScale - 0.75f) / 2 : adjustedScale;
        }

        private void RenderMesh(MeshRef? meshRef, IShaderProgram shader, float posX, float posY, float width, float height)
        {
            _matrix.Set(_api.Render.CurrentModelviewMatrix)
                   .Translate(posX, posY, 20)
                   .Scale(width, height, 0)
                   .Translate(0.5f, 0.5f, 0)
                   .Scale(0.5f, 0.5f, 0);

            shader.UniformMatrix("modelViewMatrix", _matrix.Values);
            _api.Render.RenderMesh(meshRef);
        }

        private void RenderHealthText(float currentHealth, float maxHealth, float posX, float posY, float barWidth, float barHeight)
        {
            string healthText = $"{MathF.Ceiling(currentHealth)} / {MathF.Ceiling(maxHealth)}";

            var font = CairoFont.WhiteSmallText();
            font.Color[3] = _opacity;

            // 🔥 Poprawka: Usunięcie readonly pozwala na ref:
            _api.Gui.TextTexture.GenOrUpdateTextTexture(healthText, font, ref _healthTextTexture);

            float textWidth = _healthTextTexture.Width;
            float textHeight = _healthTextTexture.Height;

            float textX = posX + barWidth / 2 - textWidth / 2;
            float textY = posY + barHeight / 2 - textHeight / 2;

            // ✅ Upewnij się, że font.Color to Vec4f
            Vec4f color = new Vec4f(
                (float)font.Color[0],
                (float)font.Color[1],
                (float)font.Color[2],
                (float)font.Color[3]
            );

            _api.Render.Render2DTexturePremultipliedAlpha(
                _healthTextTexture.TextureId,
                textX,
                textY,
                textWidth,
                textHeight,
                21f, // Z-index nad paskiem
                color
            );
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _api.Render.DeleteMesh(_backgroundMesh);
            _api.Render.DeleteMesh(_healthMesh);
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
        }
    }
}
