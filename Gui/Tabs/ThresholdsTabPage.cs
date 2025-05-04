using Cairo;
using HealthBar.Config;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public class ThresholdsTabPage : ITabPage
    {
        private readonly HealthBarSettings _settings;
        public string Key => "thresholds";

        public ThresholdsTabPage(HealthBarSettings settings)
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

            y = GuiComposerUtils.AddSectionTo(container, capi, "section.thresholds", fontSection, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.lowhp", _settings.LowHealthThreshold, val => _settings.LowHealthThreshold = TryParse(val, _settings.LowHealthThreshold), fontLabel, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.midhp", _settings.MidHealthThreshold, val => _settings.MidHealthThreshold = TryParse(val, _settings.MidHealthThreshold), fontLabel, y);

            scrollContainerBounds.fixedHeight = y;
            scrollContainerBounds.CalcWorldBounds();

            composer
                .BeginClip(clipBounds)
                .AddInteractiveElement(container, "scroll-container-sizepos")
                .EndClip();

            if (y > clipBounds.fixedHeight)
            {
                var scrollbarBounds = areaBounds.RightCopy().WithFixedWidth(20);

                composer.AddVerticalScrollbar(val =>
                {
                    scrollContainerBounds.fixedY = -val;
                    scrollContainerBounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar-sizepos");

                var scrollbar = composer.GetScrollbar("scrollbar-sizepos");
                scrollbar?.SetHeights((float)clipBounds.fixedHeight, (float)y);
                scrollbar?.SetScrollbarPosition(0);
            }
        }

        private float TryParse(string val, float fallback)
        {
            return float.TryParse(val, out var result) ? result : fallback;
        }
    }
}
