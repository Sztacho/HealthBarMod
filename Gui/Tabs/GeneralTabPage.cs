using Cairo;
using HealthBar.Config;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public class GeneralTabPage : ITabPage
    {
        private readonly HealthBarSettings _settings;
        public string Key => "general";

        public GeneralTabPage(HealthBarSettings settings)
        {
            _settings = settings;
        }

        public void Compose(ICoreClientAPI capi, GuiComposer composer, ElementBounds areaBounds, ref float y)
        {
            var clipBounds = areaBounds.ForkContainingChild(GuiStyle.HalfPadding);
            var scrollContainerBounds = ElementBounds.Fixed(0, 0, areaBounds.fixedWidth - 20, 0);
            scrollContainerBounds.ParentBounds = areaBounds;

            var container = new GuiElementContainer(capi, scrollContainerBounds);

            var fontSection = CairoFont.WhiteMediumText()
                .WithFontSize(17f)
                .WithColor(GuiStyle.ActiveButtonTextColor);

            var fontLabel = CairoFont.WhiteSmallText().WithFontSize(15f);

            y = 10;
            y = GuiComposerUtils.AddSectionTo(container, capi, "section.general", fontSection, y);
            y = GuiComposerUtils.AddSwitchTo(container, capi, "label.enabled", _settings.Enabled, val => _settings.Enabled = val, fontLabel, y);
            y = GuiComposerUtils.AddSwitchTo(container, capi, "label.showonplayer", _settings.ShowOnPlayer, val => _settings.ShowOnPlayer = val, fontLabel, y);
            y = GuiComposerUtils.AddSwitchTo(container, capi, "label.showhptext", _settings.ShowHpText, val => _settings.ShowHpText = val, fontLabel, y);

            scrollContainerBounds.fixedHeight = y;
            scrollContainerBounds.CalcWorldBounds();

            composer
                .BeginClip(clipBounds)
                .AddInteractiveElement(container, "scroll-container")
                .EndClip();

            if (y > clipBounds.fixedHeight)
            {
                var scrollbarBounds = areaBounds.RightCopy().WithFixedWidth(20);

                composer.AddVerticalScrollbar(val =>
                {
                    scrollContainerBounds.fixedY = -val;
                    scrollContainerBounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar-general");

                var scrollbar = composer.GetScrollbar("scrollbar-general");
                scrollbar?.SetHeights((float)clipBounds.fixedHeight, (float)y);
                scrollbar?.SetScrollbarPosition(0);
            }
        }

    }
}
