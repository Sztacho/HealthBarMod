using System;
using Vintagestory.API.Client;

namespace HealthBar.Gui;

public static class GuiComposerColorPickerExtensions
{
    /// <summary>
    /// Dodaje suwak kolorów (RGB) z etykietą i obsługą zmian.
    /// </summary>
    /// <param name="composer">GuiComposer</param>
    /// <param name="key">Unikalny klucz</param>
    /// <param name="label">Etykieta: "R", "G" lub "B"</param>
    /// <param name="value">Wartość początkowa (0-255)</param>
    /// <param name="x">Pozycja X</param>
    /// <param name="y">Ref do pozycji Y (przesuwany)</param>
    /// <param name="onChanged">Callback po zmianie wartości</param>
    public static GuiComposer AddColorPickSlider(this GuiComposer composer, string key, string label, int value, int x, ref int y, Action<float> onChanged)
    {
        var labelFont = CairoFont.WhiteSmallText();

        // Etykieta (R / G / B)
        composer.AddStaticText(label, labelFont, ElementBounds.Fixed(x, y, 20, 25));

        // Suwak
        composer.AddSlider(
            (val) =>
            {
                onChanged(val);
                return true;
            },
            ElementBounds.Fixed(x + 30, y, 260, 25),
            key
        );

        // Ustaw początkową wartość suwaka
        var slider = composer[key] as GuiElementSlider;
        slider?.SetValue(value);
        slider?.SetValues(value, 0, 255, 1);

        y += 30;
        return composer;
    }
}