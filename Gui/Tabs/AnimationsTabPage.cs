using Cairo;
using HealthBar.Config;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;

namespace HealthBar.Gui.Tabs
{
    public class AnimationsTabPage : ITabPage
    {
        private readonly HealthBarSettings _settings;
        public string Key => "animations";

        public AnimationsTabPage(HealthBarSettings settings)
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

            y = GuiComposerUtils.AddSectionTo(container, capi, "section.animations", fontSection, y);
            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.fadein", _settings.FadeInSpeed, val =>
            {
                if (float.TryParse(val, out var result)) _settings.FadeInSpeed = result;
            }, fontLabel, y);

            y = GuiComposerUtils.AddNumberInputTo(container, capi, "label.fadeout", _settings.FadeOutSpeed, val =>
            {
                if (float.TryParse(val, out var result)) _settings.FadeOutSpeed = result;
            }, fontLabel, y);

            scrollContainerBounds.fixedHeight = y;
            scrollContainerBounds.CalcWorldBounds();

            composer
                .BeginClip(clipBounds)
                .AddInteractiveElement(container, "scroll-container-animations")
                .EndClip();

            if (y > clipBounds.fixedHeight)
            {
                var scrollbarBounds = areaBounds.RightCopy().WithFixedWidth(20);

                composer.AddVerticalScrollbar(val =>
                {
                    scrollContainerBounds.fixedY = -val;
                    scrollContainerBounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar-animations");

                var scrollbar = composer.GetScrollbar("scrollbar-animations");
                scrollbar?.SetHeights((float)clipBounds.fixedHeight, (float)y);
                scrollbar?.SetScrollbarPosition(0);
            }
        }
    }
}
