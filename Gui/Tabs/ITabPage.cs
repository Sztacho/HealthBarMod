using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public interface ITabPage
    {
        string Key { get; }

        void Compose(
            ICoreClientAPI capi,
            GuiComposer composer,
            ElementBounds areaBounds,
            ref float y
        );
    }
}