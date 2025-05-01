using System.Collections.Generic;
using Cairo;
using HealthBar.Config;
using Vintagestory.API.Client;

namespace HealthBar.Gui;

public class GuiSimpleTabbedDialog : GuiDialog
{
    private readonly string[] tabKeys = new[] { "general", "sizepos", "animations", "colors" };
    private int currentTabIndex = 0;

    private ElementBounds scrollBounds;
    private ElementBounds clipBounds;
    private ElementBounds scrollbarBounds;

    private readonly Dictionary<string, GuiElementTextButton> tabButtons = new();
    private readonly List<(GuiElementStaticText element, double offsetY)> currentElements = new();

    private const int WIDTH = 800;
    private const int HEIGHT = 500;
    private const int TAB_WIDTH = 150;
    private const int TAB_HEIGHT = 35;

    private float scrollOffsetY = 0;

    public GuiSimpleTabbedDialog(ICoreClientAPI capi, HealthBarSettings settings, string toggleKeyCombinationCode) : base(capi)
    {
        ToggleKeyCombinationCode = toggleKeyCombinationCode;
        SetupDialog();
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        var bgBounds = ElementBounds.Fill
            .WithFixedPadding(GuiStyle.ElementToDialogPadding)
            .WithFixedWidth(WIDTH)
            .WithFixedHeight(HEIGHT);

        var tabStartY = GuiStyle.TitleBarHeight + 10;
        var tabFont = CairoFont.WhiteMediumText().WithFontSize(16f).WithOrientation(EnumTextOrientation.Center);

        var contentStartX = TAB_WIDTH + 10;
        var contentWidth = WIDTH - contentStartX - 20;
        var contentHeight = HEIGHT - GuiStyle.TitleBarHeight - 40;

        var insetBounds = ElementBounds.Fixed(contentStartX, GuiStyle.TitleBarHeight + 10, contentWidth, contentHeight);
        scrollbarBounds = insetBounds.RightCopy().WithFixedWidth(20);

        clipBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding);
        scrollBounds = ElementBounds.Fixed(0, 0, contentWidth - 20, 30);

        var composer = capi.Gui.CreateCompo("healthbar_tabs_gui", dialogBounds)
            .AddDialogTitleBar("Ustawienia", OnCloseClicked)
            .AddDialogBG(bgBounds)
            .AddInset(insetBounds, 3);

        for (int i = 0; i < tabKeys.Length; i++)
        {
            int tabIndex = i;
            string key = tabKeys[i];

            const int margin = 10;
            int buttonWidth = TAB_WIDTH - margin * 2;
            int buttonX = margin;
            int buttonY = (int)tabStartY + i * (TAB_HEIGHT + 5);

            var bounds = ElementBounds.Fixed(buttonX, buttonY, buttonWidth, TAB_HEIGHT);

            var button = new GuiElementTextButton(
                capi,
                key.Capitalize(),
                tabFont,
                tabFont,
                () => { OnTabClicked(tabIndex); return true; },
                bounds
            );

            button.SetActive(tabIndex == currentTabIndex);
            tabButtons[key] = button;
            composer.AddInteractiveElement(button, $"tab-{key}");
        }

        composer
            .BeginClip(clipBounds)
            .AddDynamicCustomDraw(scrollBounds, OnDrawScrollContent, "scroll-draw")
            .EndClip()
            .AddVerticalScrollbar(OnScrollbarChanged, scrollbarBounds, "scrollbar");

        SingleComposer = composer.Compose();

        LoadTabContent(currentTabIndex);
    }

    private void LoadTabContent(int tabIndex)
    {
        currentElements.Clear();

        var font = CairoFont.WhiteSmallText();
        float y = 0;

        for (int i = 0; i < 20; i++)
        {
            var bounds = ElementBounds.Fixed(0, y, 500, 30);
            bounds.ParentBounds = scrollBounds; // <=== KLUCZOWE!
            bounds.CalcWorldBounds();

            var element = new GuiElementStaticText(
                capi,
                $"[{tabKeys[tabIndex].Capitalize()}] Element {i + 1}",
                EnumTextOrientation.Left,
                bounds,
                font
            );

            currentElements.Add((element, y));
            y += 35;
        }

        scrollBounds.fixedHeight = y;
        scrollBounds.CalcWorldBounds();

        var scrollbar = SingleComposer.GetScrollbar("scrollbar");
        scrollbar?.SetHeights((float)clipBounds.fixedHeight, y);

        scrollOffsetY = 0;

        capi.Event.EnqueueMainThreadTask(() =>
        {
            SingleComposer.GetCustomDraw("scroll-draw").Redraw();
        }, "force-redraw");
    }

    private void OnTabClicked(int tabIndex)
    {
        if (tabIndex == currentTabIndex) return;

        tabButtons[tabKeys[currentTabIndex]].SetActive(false);
        currentTabIndex = tabIndex;
        tabButtons[tabKeys[currentTabIndex]].SetActive(true);

        scrollOffsetY = 0;

        var scrollbar = SingleComposer.GetScrollbar("scrollbar");
        scrollbar?.SetScrollbarPosition(0);

        LoadTabContent(currentTabIndex);
    }

    private void OnScrollbarChanged(float value)
    {
        scrollOffsetY = 5 - value;
        scrollBounds.fixedY = scrollOffsetY;
        scrollBounds.CalcWorldBounds();
        SingleComposer.GetCustomDraw("scroll-draw").Redraw();
    }

    private void OnCloseClicked()
    {
        TryClose();
    }

    private void OnDrawScrollContent(Context ctx, ImageSurface surface, ElementBounds currentBounds)
    {
        foreach (var (element, relY) in currentElements)
        {
            if (element?.Bounds == null) continue;

            element.Bounds.absFixedX = currentBounds.absFixedX;
            element.Bounds.absFixedY = currentBounds.absFixedY + scrollOffsetY + relY;
            element.ComposeElements(ctx, surface);
        }
    }

    public override string ToggleKeyCombinationCode { get; }
}

public static class StringExtensions
{
    public static string Capitalize(this string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    }
}
