using Cairo;
using HealthBar.Config;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public class ColorsTabPage : ITabPage
    {
        private readonly HealthBarSettings _settings;
        public string Key => "colors";

        public ColorsTabPage(HealthBarSettings settings)
        {
            _settings = settings;
        }

        public void Compose(
            ICoreClientAPI capi,
            GuiComposer composer,
            ElementBounds areaBounds,
            ref float y)
        {
            var clipBounds = areaBounds.ForkContainingChild(GuiStyle.HalfPadding);
            var scrollContainerBounds = ElementBounds.Fixed(0, 0, areaBounds.fixedWidth - 20, 0);
            scrollContainerBounds.ParentBounds = areaBounds;

            var container = new GuiElementContainer(capi, scrollContainerBounds);
            y = 10;

            y = GuiComposerUtils.AddSectionTo(container, capi, "section.colors", CairoFont.WhiteMediumText().WithFontSize(17f).WithColor(GuiStyle.ActiveButtonTextColor), y);

            y = GuiComposerUtils.AddColorGroupTo(container, capi, "label.colorlow", _settings.LowHealthColor, val => _settings.LowHealthColor = val, y);
            y = GuiComposerUtils.AddColorGroupTo(container, capi, "label.colormid", _settings.MidHealthColor, val => _settings.MidHealthColor = val, y);
            y = GuiComposerUtils.AddColorGroupTo(container, capi, "label.colorfull", _settings.FullHealthColor, val => _settings.FullHealthColor = val, y);
            y = GuiComposerUtils.AddColorGroupTo(container, capi, "label.colorframe", _settings.FrameColor, val => _settings.FrameColor = val, y);

            scrollContainerBounds.fixedHeight = y + 10;
            scrollContainerBounds.CalcWorldBounds();

            composer
                .BeginClip(clipBounds)
                .AddInteractiveElement(container, "scroll-container")
                .EndClip();

            var scrollbarBounds = areaBounds.RightCopy().WithFixedWidth(20);
            composer.AddVerticalScrollbar(val =>
            {
                scrollContainerBounds.fixedY = -val;
                scrollContainerBounds.CalcWorldBounds();
            }, scrollbarBounds, "scrollbar-colors");

            var scrollbar = composer.GetScrollbar("scrollbar-colors");
            scrollbar?.SetHeights((float)clipBounds.fixedHeight, (float)scrollContainerBounds.fixedHeight);
            scrollbar?.SetScrollbarPosition(0);
        }
    }
}
