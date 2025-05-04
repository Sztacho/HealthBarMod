using System;
using System.Globalization;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace HealthBar.Gui.Utils
{
    public static class GuiComposerUtils
    {
        public static float AddNumberInputTo(GuiElementContainer container, ICoreClientAPI capi, string langKey,
            float value, Action<string> onChange, CairoFont font, float y)
        {
            var labelBounds = ElementBounds.Fixed(40, y, 240, 25);
            var label = new GuiElementStaticText(capi, Lang.Get("healthbar:" + langKey), EnumTextOrientation.Left,
                labelBounds, font);

            var inputBounds = ElementBounds.Fixed(310, y, 180, 25);
            var input = new GuiElementNumberInput(capi, inputBounds, onChange, CairoFont.TextInput());

            container.Add(label);
            container.Add(input);
            input.SetValue(value.ToString("0.##"));

            return y + 35f;
        }

        public static float AddSectionTo(GuiElementContainer container, ICoreClientAPI capi, string langKey,
            CairoFont font, float y)
        {
            var bounds = ElementBounds.Fixed(20, y, 500, 30);
            var label = new GuiElementStaticText(capi, Lang.Get("healthbar:" + langKey), EnumTextOrientation.Left,
                bounds, font);
            container.Add(label);
            return y + 35f;
        }

        public static float AddSwitchTo(GuiElementContainer container, ICoreClientAPI capi, string langKey, bool value,
            Action<bool> onToggle, CairoFont font, float y)
        {
            var labelBounds = ElementBounds.Fixed(40, y, 240, 25);
            var label = new GuiElementStaticText(capi, Lang.Get("healthbar:" + langKey), EnumTextOrientation.Left,
                labelBounds, font);

            var switchBounds = ElementBounds.Fixed(310, y, 180, 25);
            var toggle = new GuiElementSwitch(capi, onToggle, switchBounds);
            toggle.SetValue(value);

            container.Add(label);
            container.Add(toggle);

            return y + 35f;
        }

        public static float AddColorGroupTo(
            GuiElementContainer container,
            ICoreClientAPI capi,
            string langKey,
            string initialHex,
            Action<string> onChanged,
            float y)
        {
            var baseKey = $"colorpicker-{langKey}";
            var (r, g, b) = ParseHex(initialHex);

            int rVal = r, gVal = g, bVal = b;

            var font = CairoFont.WhiteSmallText();
            var labelBounds = ElementBounds.Fixed(30, y, 300, 25);
            var label = new GuiElementStaticText(capi, Translate(langKey), EnumTextOrientation.Left, labelBounds, font);
            container.Add(label);
            y += 30;

            y = AddColorPickSliderTo(container, capi, "R", r, y, val =>
            {
                rVal = val;
                onChanged(ToHex(rVal, gVal, bVal));
            });

            y = AddColorPickSliderTo(container, capi, "G", g, y, val =>
            {
                gVal = val;
                onChanged(ToHex(rVal, gVal, bVal));
            });

            y = AddColorPickSliderTo(container, capi, "B", b, y, val =>
            {
                bVal = val;
                onChanged(ToHex(rVal, gVal, bVal));
            });

            return y + 10;
        }

        private static float AddColorPickSliderTo(
            GuiElementContainer container,
            ICoreClientAPI capi,
            string label,
            int value,
            float y,
            Action<int> onChanged)
        {
            var font = CairoFont.WhiteSmallText();

            var labelBounds = ElementBounds.Fixed(50, y, 40, 25);
            var labelEl = new GuiElementStaticText(capi, label, EnumTextOrientation.Left, labelBounds, font);

            var sliderBounds = ElementBounds.Fixed(100, y, 300, 25);
            var slider = new GuiElementSlider(capi, val =>
            {
                onChanged(val);
                return true;
            }, sliderBounds);
            slider?.SetValue(value);
            slider?.SetValues(value, 0, 255, 1);

            container.Add(labelEl);
            container.Add(slider);

            return y + 30;
        }

        private static string ToHex(int r, int g, int b)
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static (int r, int g, int b) ParseHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex[1..];
            if (hex.Length == 6 &&
                int.TryParse(hex[..2], NumberStyles.HexNumber, null, out var r) &&
                int.TryParse(hex[2..4], NumberStyles.HexNumber, null, out var g) &&
                int.TryParse(hex[4..6], NumberStyles.HexNumber, null, out var b))
            {
                return (r, g, b);
            }

            return (255, 255, 255);
        }

        public static string Translate(string key)
        {
            return Lang.Get("healthbar:" + key);
        }
    }
}