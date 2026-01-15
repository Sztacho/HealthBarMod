using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace HealthBar.Rendering.Core;

public sealed class GuiQuadDrawer(ICoreClientAPI capi)
{
    private const float Z = 20f;

    private readonly Matrixf _model = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(IShaderProgram sh, MeshRef mesh, float x, float y, float w, float h)
    {
        _model.Set(capi.Render.CurrentModelviewMatrix)
            .Translate(x, y, Z)
            .Scale(w, h, 0f)
            .Translate(.5f, .5f, 0f)
            .Scale(.5f, .5f, 0f);

        sh.UniformMatrix("modelViewMatrix", _model.Values);
        capi.Render.RenderMesh(mesh);
    }
}
