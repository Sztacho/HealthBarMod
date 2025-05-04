using Cairo;
using HealthBar.Config;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public class SizePosTabPage : ITabPage
    {
        private readonly HealthBarSettings _settings;
        public string Key => "sizepos";

        public SizePosTabPage(HealthBarSettings settings)
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

            y = GuiComposerUtils.AddSectionTo(container, capi, "section.sizepos", fontSection, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.barwidth", _settings.BarWidth, val => _settings.BarWidth = TryParse(val, _settings.BarWidth), fontLabel, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.barheight", _settings.BarHeight, val => _settings.BarHeight = TryParse(val, _settings.BarHeight), fontLabel, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.verticaloffset", _settings.VerticalOffset, val => _settings.VerticalOffset = TryParse(val, _settings.VerticalOffset), fontLabel, y);

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
