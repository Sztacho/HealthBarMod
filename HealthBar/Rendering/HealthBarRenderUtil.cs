using System;
using System.Runtime.CompilerServices;
using HealthBar.Config;
using HealthBar.Rendering.Themes;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering;

internal static class HealthBarRenderUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToRgbVec4(int argb, float alpha, ref Vec4f dst)
    {
        dst.R = ((argb >> 16) & 255) * (1f / 255f);
        dst.G = ((argb >> 8) & 255) * (1f / 255f);
        dst.B = (argb & 255) * (1f / 255f);
        dst.A = alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4f HpColorToVec4(in BarRenderData data, in ModConfigSnapshot cfg)
    {
        var hp = (int)(data.PercentHp * 100f);
        var argb = hp <= cfg.LowHealthThreshold
            ? cfg.LowHealthColorArgb
            : hp <= cfg.MidHealthThreshold
                ? cfg.MidHealthColorArgb
                : cfg.FullHealthColorArgb;

        return new Vec4f(
            ((argb >> 16) & 255) * (1f / 255f),
            ((argb >> 8) & 255) * (1f / 255f),
            (argb & 255) * (1f / 255f),
            data.Opacity);
    }

    public static void DrawDigitsText(float cur, float max, float areaX, float areaY, float areaW, float areaH, float opacity, in ThemeRenderContext ctx)
    {
        if (opacity <= 0.01f) return;

        var curI = (int)MathF.Ceiling(cur);
        var maxI = (int)MathF.Ceiling(max);
        if (curI < 0) curI = 0;
        if (maxI < 1) maxI = 1;

        Span<char> buf = stackalloc char[16];
        var len = 0;
        len += WriteInt(curI, buf[len..]);
        buf[len++] = '/';
        len += WriteInt(maxI, buf[len..]);

        var minH = 10f * Vintagestory.API.Config.RuntimeEnv.GUIScale;
        var maxH = 18f * Vintagestory.API.Config.RuntimeEnv.GUIScale;

        var margin = MathF.Max(1f, 1f * Vintagestory.API.Config.RuntimeEnv.GUIScale);
        var availW = areaW - 2f * margin;
        var availH = areaH - 2f * margin;
        if (availW <= 0 || availH <= 0) return;

        var desiredH = availH * 0.85f;
        if (desiredH < minH) return;
        var targetH = MathF.Min(desiredH, maxH);

        var glyphScale = targetH / Text.DigitFont.GlyphH;
        glyphScale = Math.Clamp(glyphScale, 0.5f, 4f);

        var gh = Text.DigitFont.GlyphH * glyphScale;
        if (gh > availH) return;

        var gw = Text.DigitFont.GlyphW * glyphScale;
        var totalW = gw * len;
        if (totalW > availW) return;

        var tx = areaX + (areaW - totalW) * 0.5f;
        var ty = areaY + (areaH - gh) * 0.5f;

        tx = MathF.Round(tx);
        ty = MathF.Round(ty);

        var sh = ctx.Shader;
        sh.Uniform("noTexture", 0f);
        sh.BindTexture2D(ctx.Digits.SamplerName, ctx.Digits.Texture.TextureId, 0);

        var shadowOpacity = opacity * 0.55f;
        sh.Uniform("rgbaIn", new Vec4f(shadowOpacity, shadowOpacity, shadowOpacity, shadowOpacity));
        DrawDigitsSpan(buf[..len], tx + 1, ty + 1, gw, gh, in ctx);

        sh.Uniform("rgbaIn", new Vec4f(opacity, opacity, opacity, opacity));
        DrawDigitsSpan(buf[..len], tx, ty, gw, gh, in ctx);
    }

    private static void DrawDigitsSpan(ReadOnlySpan<char> text, float x, float y, float gw, float gh, in ThemeRenderContext ctx)
    {
        var cx = x;
        
        foreach (var t in text)
        {
            var mesh = ctx.Digits.GetMesh(t);
            if (mesh != null)
                ctx.Drawer.Draw(ctx.Shader, mesh, cx, y, gw, gh);
            cx += gw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteInt(int value, Span<char> dst)
    {
        if (value == 0)
        {
            dst[0] = '0';
            return 1;
        }

        var v = value;
        var len = 0;
        while (v > 0 && len < dst.Length)
        {
            var digit = v % 10;
            dst[len++] = (char)('0' + digit);
            v /= 10;
        }

        for (int i = 0, j = len - 1; i < j; i++, j--)
        {
            (dst[i], dst[j]) = (dst[j], dst[i]);
        }

        return len;
    }
}
