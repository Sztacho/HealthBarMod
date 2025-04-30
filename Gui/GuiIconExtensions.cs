using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace HealthBar.Gui;

public static class GuiIconExtensions
{
    public static TextureAtlasPosition GenColorTexture(this ICoreClientAPI api, int r, int g, int b)
    {
        int width = 16, height = 16;
        int[] rgba = new int[width * height];

        int color = ColorUtil.ToRgba(255, r, g, b);
        for (int i = 0; i < rgba.Length; i++)
        {
            rgba[i] = color;
        }

        var texture = new LoadedTexture(api);
        api.Render.LoadOrUpdateTextureFromRgba(rgba, true, 0, ref texture);

        return new TextureAtlasPosition()
        {
            atlasTextureId = texture.TextureId,
            x1 = 0, y1 = 0,
            x2 = 1, y2 = 1
        };
    }
}