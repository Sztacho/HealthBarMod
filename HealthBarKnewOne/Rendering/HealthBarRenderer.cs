using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace HealthBarKnewOne.Rendering;
public sealed class HealthBarRenderer : IRenderer, IDisposable {
	private const float BaseScaleDivider = 4f;
	private const float ScaleBoost = 2f;
	private const float MinScale = 0.7f;
	private const float Z = 20f;

	private const float FillLerpSpeed = 6f;
	private const float BorderPx = 1f;

	private static ICoreClientAPI Api => ModSystem.Api as ICoreClientAPI;
	public static ModConfig Config => ModConfig.Instance;
	private readonly Matrixf _model = new();

	private readonly MeshRef border, background, fillMesh;
	private readonly Vec4f frameColor = new(), backgroundColor = new(0, 0, 0, 0.6f);
	private Vec4f hpColor = new();

	private static readonly Stack<LoadedTexture> LoadedTexturePool = new();
	private LoadedTexture _txtTex;

	private readonly CairoFont _font = CairoFont.WhiteSmallText();
	private int _cachedHash = 0;
	private string _cachedText = "";
	private float opacity;
	private float _shownPct = 1f;
	private bool _firstShow = true;

	public Entity? TargetEntity { get; set; }
	public bool IsVisible { get; set; }
	public bool IsFullyInvisible => opacity <= 0.001f;

	public double RenderOrder => 0.41;
	public int RenderRange => 10;

	public HealthBarRenderer() {
		border = Api.Render.UploadMesh(LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb));
		background = Api.Render.UploadMesh(QuadMeshUtil.GetQuad());
		fillMesh = Api.Render.UploadMesh(QuadMeshUtil.GetQuad());

		ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(Config.FrameColor ?? "#CCCCCC"), ref frameColor);

		_txtTex = LoadedTexturePool.TryPop(out var pooled) ? pooled : new LoadedTexture(Api);
		Api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
	}

	public void Dispose() {
		Api.Render.DeleteMesh(border);
		Api.Render.DeleteMesh(background);
		Api.Render.DeleteMesh(fillMesh);
		Api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);

		_txtTex?.Dispose();
		GC.SuppressFinalize(this);
	}

	public void OnRenderFrame(float dt, EnumRenderStage stage) {
		if (!CanRender())
			return;

		var hpNode = TargetEntity!.WatchedAttributes.GetTreeAttribute("health");
		var currentHp = hpNode?.GetFloat("currenthealth") ?? 0f;
		var maxHp = MathF.Max(1f, hpNode?.GetFloat("maxhealth") ?? 1f);
		var percentHp = Clamp01(currentHp / maxHp);

		if (_firstShow) {
			_shownPct = percentHp;
			_firstShow = false;
		} else if (percentHp < _shownPct) {
			_shownPct = Lerp(_shownPct, percentHp, Clamp01(dt * FillLerpSpeed));
		} else
			_shownPct = percentHp;

		opacity = Clamp01(opacity + dt / (IsVisible ? Config.FadeInSpeed : -Config.FadeOutSpeed));
		if (opacity <= 0)
			return; // całkiem niewidoczny

		UpdateHealthColor((int)(percentHp * 100));
		frameColor.A = backgroundColor.A = opacity;

		var scr = ProjectOnScreen();
		if (scr.Z < 0)
			return;

		var distScale = BaseScaleDivider / MathF.Max(1f, (float)scr.Z);
		var scale = MathF.Max(MinScale, distScale * ScaleBoost);

		var width = scale * Config.BarWidth;
		var height = scale * Config.BarHeight;
		var x = (float)scr.X - width / 2f;
		var y = Api.Render.FrameHeight - (float)scr.Y - height - Config.VerticalOffset;

		var sh = Api.Render.CurrentActiveShader;
		sh.Uniform("noTexture", 1f);
		sh.UniformMatrix("projectionMatrix", Api.Render.CurrentProjectionMatrix);

		var bp = BorderPx * RuntimeEnv.GUIScale;
		sh.Uniform("rgbaIn", frameColor);
		DrawQuad(sh, border, x - bp, y - bp, width + bp * 2, height + bp * 2);
		sh.Uniform("rgbaIn", backgroundColor);
		DrawQuad(sh, background, x, y, width, height);
		sh.Uniform("rgbaIn", hpColor);
		DrawQuad(sh, fillMesh, x + (width - _shownPct * width) / 2f, y, _shownPct * width, height);
		if (Config.ShowHpText)
			DrawText(currentHp, maxHp, x, y, width, height, scale);
	}

	private bool CanRender() {
		var node = TargetEntity?.WatchedAttributes.GetTreeAttribute("health");
		return node != null && node.GetFloat("currenthealth") > 0 && (opacity > 0 || IsVisible);
	}

	private void UpdateHealthColor(int hp) {
		var hex = hp <= Config.LowHealthThreshold ? Config.LowHealthColor : hp <= Config.MidHealthThreshold ? Config.MidHealthColor : Config.FullHealthColor;

		ColorUtil.ToRGBAVec4f(ColorUtil.Hex2Int(hex), ref hpColor);
		hpColor.A = opacity;
	}

	private void DrawQuad(IShaderProgram sh, MeshRef mesh,
		float x, float y, float w, float h) {
		_model.Set(Api.Render.CurrentModelviewMatrix)
			.Translate(x, y, Z)
			.Scale(w, h, 0f)
			.Translate(.5f, .5f, 0f)
			.Scale(.5f, .5f, 0f);

		sh.UniformMatrix("modelViewMatrix", _model.Values);
		Api.Render.RenderMesh(mesh);
	}


	private void DrawText(float cur, float max, float x, float y, float w, float h, float scale) {
		var sizeBucket = (int)(scale * 32);
		var txt = $"{MathF.Ceiling(cur)}/{MathF.Ceiling(max)}";
		var hash = HashCode.Combine(txt, sizeBucket);

		if (hash != _cachedHash) {
			_cachedHash = hash;
			_cachedText = txt;

			_font.Color[3] = opacity;
			_font.StrokeColor = new double[] { 0, 0, 0, opacity };
			_font.UnscaledFontsize = 10 * scale;
			_font.StrokeWidth = 2.0 * RuntimeEnv.GUIScale;

			for (var i = 0; i < 2; i++) {
				Api.Gui.TextTexture.GenOrUpdateTextTexture(txt, _font, ref _txtTex);
				var rw = (w * 0.9f) / _txtTex.Width;
				var rh = (h * 0.9f) / _txtTex.Height;
				var s = MathF.Min(rw, rh);
				if (s >= 1f)
					break;
				_font.UnscaledFontsize *= MathF.Sqrt(s);
			}

			Api.Gui.TextTexture.GenOrUpdateTextTexture(txt, _font, ref _txtTex);
		}

		if (opacity < 0.01f)
			return;

		var tx = x + (w - _txtTex.Width) / 2f;
		var ty = y + (h - _txtTex.Height) / 2f;

		Api.Render.Render2DTexturePremultipliedAlpha(
			_txtTex.TextureId, tx + 1, ty + 1, _txtTex.Width, _txtTex.Height,
			Z + 0.9f, new Vec4f(0, 0, 0, opacity * 0.5f));

		Api.Render.Render2DTexturePremultipliedAlpha(
			_txtTex.TextureId, tx, ty, _txtTex.Width, _txtTex.Height,
			Z + 1f, new Vec4f(1, 1, 1, opacity));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Clamp01(float v) => v <= 0f ? 0f : (v >= 1f ? 1f : v);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Lerp(float a, float b, float t) => a + (b - a) * t;

	private Vec3d ProjectOnScreen() {
		var e = TargetEntity!;
		var p = new Vec3d(e.Pos.X,
			e.Pos.Y + e.CollisionBox.Y2,
			e.Pos.Z);
		p.Add(e.CollisionBox.X2 - e.OriginCollisionBox.X2, 0, 0);

		return MatrixToolsd.Project(
			p,
			Api.Render.PerspectiveProjectionMat,
			Api.Render.PerspectiveViewMat,
			Api.Render.FrameWidth,
			Api.Render.FrameHeight);
	}
}