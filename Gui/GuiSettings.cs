using System;
using HealthBar.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace HealthBar.Gui;

public class GuiSettings : GuiDialog
{
    private readonly HealthBarSettings _settings;
    private GuiComposer composer;
    private int y = 40;

    public GuiSettings(ICoreClientAPI capi, HealthBarSettings settings, string toggleKeyCombinationCode) : base(capi)
    {
        _settings = settings;
        ToggleKeyCombinationCode = toggleKeyCombinationCode;
        SetupDialog();
    }

    public override string ToggleKeyCombinationCode { get; }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle).WithFixedWidth(580);
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding).FixedGrow(20, 20);

        composer = capi.Gui.CreateCompo("healthbar_config", dialogBounds)
            .AddDialogBG(bgBounds)
            .AddDialogTitleBar(Translate("settings.title"), OnTitleBarCloseClicked);

        var labelFont = CairoFont.WhiteSmallText().WithFontSize(15f);
        var sectionFont = CairoFont.WhiteMediumText().WithFontSize(17f).WithColor(GuiStyle.ActiveButtonTextColor);

        AddSection("section.sizepos", sectionFont);
        AddNumber("label.barwidth", _settings.BarWidth, val => _settings.BarWidth = TryParse(val, _settings.BarWidth), labelFont);
        AddNumber("label.barheight", _settings.BarHeight, val => _settings.BarHeight = TryParse(val, _settings.BarHeight), labelFont);
        AddNumber("label.verticaloffset", _settings.VerticalOffset, val => _settings.VerticalOffset = TryParse(val, _settings.VerticalOffset), labelFont);

        AddSection("section.animations", sectionFont);
        AddNumber("label.fadein", _settings.FadeInSpeed, val => _settings.FadeInSpeed = TryParse(val, _settings.FadeInSpeed), labelFont);
        AddNumber("label.fadeout", _settings.FadeOutSpeed, val => _settings.FadeOutSpeed = TryParse(val, _settings.FadeOutSpeed), labelFont);

        AddSection("section.thresholds", sectionFont);
        AddNumber("label.lowhp", _settings.LowHealthThreshold, val => _settings.LowHealthThreshold = TryParse(val, _settings.LowHealthThreshold), labelFont);
        AddNumber("label.midhp", _settings.MidHealthThreshold, val => _settings.MidHealthThreshold = TryParse(val, _settings.MidHealthThreshold), labelFont);

        AddSection("section.colors", sectionFont);
        AddColorGroup("label.colorlow", _settings.LowHealthColor, val => _settings.LowHealthColor = val);
        AddColorGroup("label.colormid", _settings.MidHealthColor, val => _settings.MidHealthColor = val);
        AddColorGroup("label.colorfull", _settings.FullHealthColor, val => _settings.FullHealthColor = val);
        AddColorGroup("label.colorframe", _settings.FrameColor, val => _settings.FrameColor = val);

        y += 10;

        composer
            .AddSmallButton(Translate("button.save"), OnSaveClicked, ElementBounds.Fixed(30, y, 140, 30))
            .AddSmallButton(Translate("button.reset"), OnResetClicked, ElementBounds.Fixed(190, y, 140, 30))
            .AddSmallButton(Translate("button.close"), OnCloseClicked, ElementBounds.Fixed(350, y, 140, 30));

        SingleComposer = composer.Compose(focusFirstElement: false);
    }

    private void AddSection(string langKey, CairoFont font)
    {
        composer.AddStaticText(Translate(langKey), font, ElementBounds.Fixed(20, y, 500, 30));
        y += 35;
    }

    private void AddNumber(string langKey, float value, Action<string> onChange, CairoFont font, int stepY = 35)
    {
        string key = $"input-number-{langKey}";
        composer.AddStaticText(Translate(langKey), font, ElementBounds.Fixed(40, y, 240, 25));
        composer.AddNumberInput(ElementBounds.Fixed(310, y, 180, 25), onChange, CairoFont.TextInput(), key);
        (composer[key] as GuiElementNumberInput)?.SetValue(value.ToString("0.##"));
        y += stepY;
    }

    private void AddColorGroup(string langKey, string initialHex, Action<string> onChanged)
    {
        string baseKey = $"colorpicker-{langKey}";
        var (r, g, b) = ParseHex(initialHex);

        composer.AddStaticText(Translate(langKey), CairoFont.WhiteSmallText(), ElementBounds.Fixed(30, y, 300, 25));
        y += 30;
        composer.AddColorPickSlider(baseKey + "-r", "R", r, 30, ref y, _ => UpdateColor(baseKey, onChanged));
        composer.AddColorPickSlider(baseKey + "-g", "G", g, 30, ref y, _ => UpdateColor(baseKey, onChanged));
        composer.AddColorPickSlider(baseKey + "-b", "B", b, 30, ref y, _ => UpdateColor(baseKey, onChanged));
        y += 10;
    }

    private void UpdateColor(string baseKey, Action<string> onChanged)
    {
        var (r, g, b) = GetColorFromSliders(baseKey);
        string hex = $"#{r:X2}{g:X2}{b:X2}";
        onChanged(hex);
    }
    
    private void UpdateAllFieldsFromSettings()
    {
        UpdateNumber("label.barwidth", _settings.BarWidth);
        UpdateNumber("label.barheight", _settings.BarHeight);
        UpdateNumber("label.verticaloffset", _settings.VerticalOffset);
        UpdateNumber("label.fadein", _settings.FadeInSpeed);
        UpdateNumber("label.fadeout", _settings.FadeOutSpeed);
        UpdateNumber("label.lowhp", _settings.LowHealthThreshold);
        UpdateNumber("label.midhp", _settings.MidHealthThreshold);

        UpdateColorSliders("label.colorlow", _settings.LowHealthColor);
        UpdateColorSliders("label.colormid", _settings.MidHealthColor);
        UpdateColorSliders("label.colorfull", _settings.FullHealthColor);
        UpdateColorSliders("label.colorframe", _settings.FrameColor);
    }

    private void UpdateNumber(string langKey, float value)
    {
        string key = $"input-number-{langKey}";
        if (composer[key] is GuiElementNumberInput input)
        {
            input.SetValue(value.ToString("0.##"));
        }
    }

    private void UpdateColorSliders(string langKey, string hex)
    {
        var (r, g, b) = ParseHex(hex);
        string baseKey = $"colorpicker-{langKey}";
        (composer[$"{baseKey}-r"] as GuiElementSlider)?.SetValue(r);
        (composer[$"{baseKey}-g"] as GuiElementSlider)?.SetValue(g);
        (composer[$"{baseKey}-b"] as GuiElementSlider)?.SetValue(b);
    }


    private (int r, int g, int b) GetColorFromSliders(string baseKey)
    {
        var r = (composer[$"{baseKey}-r"] as GuiElementSlider)?.GetValue() ?? 0;
        var g = (composer[$"{baseKey}-g"] as GuiElementSlider)?.GetValue() ?? 0;
        var b = (composer[$"{baseKey}-b"] as GuiElementSlider)?.GetValue() ?? 0;
        return ((int)r, (int)g, (int)b);
    }

    private (int r, int g, int b) ParseHex(string hex)
    {
        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length == 6 &&
            int.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out var r) &&
            int.TryParse(hex[2..4], System.Globalization.NumberStyles.HexNumber, null, out var g) &&
            int.TryParse(hex[4..6], System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            return (r, g, b);
        }
        return (255, 255, 255);
    }

    private string Translate(string key)
    {
        return Lang.Get("healthbar:" + key);
    }

    private float TryParse(string val, float fallback)
    {
        return float.TryParse(val, out var result) ? result : fallback;
    }

    private bool OnSaveClicked()
    {
        _settings.Save(capi);
        capi.ShowChatMessage(Translate("message.saved"));
        TryClose();
        return true;
    }

    private bool OnResetClicked()
    {
        _settings.ResetToDefaults();
        UpdateAllFieldsFromSettings();
        _settings.Save(capi);
        capi.ShowChatMessage(Translate("message.reset"));
        return true;
    }

    private bool OnCloseClicked()
    {
        TryClose();
        return true;
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }
}
