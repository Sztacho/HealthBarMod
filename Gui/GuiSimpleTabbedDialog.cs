using System.Collections.Generic;
using HealthBar.Config;
using HealthBar.Gui.Tabs;
using HealthBar.Gui.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace HealthBar.Gui
{
    public class GuiSimpleTabbedDialog : GuiDialog
    {
        private readonly string[] _tabKeys = new[] { "general", "sizepos", "animations", "thresholds", "colors" };
        private int _currentTabIndex = 0;
        private readonly HealthBarSettings _settings;

        private readonly Dictionary<string, GuiElementTextButton> _tabButtons = new();
        private readonly Dictionary<string, ITabPage> _tabPages;

        private const int Width = 800;
        private const int Height = 500;
        private const int TabWidth = 150;
        private const int TabHeight = 35;

        private ElementBounds _insetBounds;

        public GuiSimpleTabbedDialog(ICoreClientAPI capi, HealthBarSettings settings, string toggleKeyCombinationCode) :
            base(capi)
        {
            ToggleKeyCombinationCode = toggleKeyCombinationCode;
            _settings = settings;

            _tabPages = new Dictionary<string, ITabPage>
            {
                { "general", new GeneralTabPage(settings) },
                { "sizepos", new SizePosTabPage(settings) },
                { "animations", new AnimationsTabPage(settings) },
                { "thresholds", new ThresholdsTabPage(settings) },
                { "colors", new ColorsTabPage(settings) }
            };

            BuildDialog();
        }

        private void BuildDialog()
        {
            var contentStartX = TabWidth + 10;
            var contentWidth = Width - contentStartX - 20;
            var contentHeight = Height - (float)GuiStyle.TitleBarHeight - 40;

            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            _insetBounds =
                ElementBounds.Fixed(contentStartX, GuiStyle.TitleBarHeight + 10, contentWidth, contentHeight);

            var bgBounds = ElementBounds.Fill
                .WithFixedPadding(GuiStyle.ElementToDialogPadding)
                .WithFixedWidth(Width)
                .WithFixedHeight(Height);

            var tabStartY = GuiStyle.TitleBarHeight + 10;
            var tabFont = CairoFont.WhiteMediumText().WithFontSize(16f).WithOrientation(EnumTextOrientation.Center);

            var composer = capi.Gui.CreateCompo("healthbar_tabs_gui", dialogBounds)
                .AddDialogTitleBar(GuiComposerUtils.Translate("settings.title"), OnCloseClicked)
                .AddDialogBG(bgBounds)
                .AddInset(_insetBounds, 3);

            for (var i = 0; i < _tabKeys.Length; i++)
            {
                var tabIndex = i;
                var key = _tabKeys[i];

                const int margin = 10;
                const int buttonWidth = TabWidth - margin * 2;
                var buttonY = (int)tabStartY + i * (TabHeight + 5);

                var bounds = ElementBounds.Fixed(margin, buttonY, buttonWidth, TabHeight);

                var button = new GuiElementTextButton(
                    capi,
                    Lang.Get("healthbar:tab." + key),
                    tabFont,
                    tabFont,
                    () =>
                    {
                        OnTabClicked(tabIndex);
                        return true;
                    },
                    bounds
                );

                button.SetActive(tabIndex == _currentTabIndex);
                _tabButtons[key] = button;
                composer.AddInteractiveElement(button, $"tab-{key}");
            }

            var btnStartY = (int)(tabStartY + _tabKeys.Length * (TabHeight + 5) + 20);
            const int btnHeight = 30;
            const int btnSpacing = 5;
            const int btnWidth = TabWidth - 20;
            const int btnX = 10;

            composer
                .AddSmallButton(GuiComposerUtils.Translate("button.save"), OnSaveClicked,
                    ElementBounds.Fixed(btnX, btnStartY, btnWidth, btnHeight))
                .AddSmallButton(GuiComposerUtils.Translate("button.reset"), OnResetClicked,
                    ElementBounds.Fixed(btnX, btnStartY + btnHeight + btnSpacing, btnWidth, btnHeight))
                .AddSmallButton(GuiComposerUtils.Translate("button.close"), TryClose,
                    ElementBounds.Fixed(btnX, btnStartY + 2 * (btnHeight + btnSpacing), btnWidth, btnHeight));

            float y = 0;
            _tabPages[_tabKeys[_currentTabIndex]].Compose(capi, composer, _insetBounds, ref y);

            const int discordBtnHeight = 30;
            const int dialogBottom = Height - 20;
            const int discordBtnY = dialogBottom - discordBtnHeight - 20;

            composer.AddSmallButton(
                GuiComposerUtils.Translate("button.discord"),
                () =>
                {
                    capi.Gui.OpenLink("https://discord.gg/vpdsneENyE");
                    return true;
                },
                ElementBounds.Fixed(10, discordBtnY, TabWidth - 20, discordBtnHeight)
            );

            SingleComposer = composer.Compose();
        }


        private void OnTabClicked(int tabIndex)
        {
            if (tabIndex == _currentTabIndex) return;

            _tabButtons[_tabKeys[_currentTabIndex]].SetActive(false);
            _currentTabIndex = tabIndex;
            _tabButtons[_tabKeys[_currentTabIndex]].SetActive(true);

            SingleComposer?.Dispose();
            BuildDialog();
        }

        private void OnCloseClicked()
        {
            TryClose();
        }

        private bool OnSaveClicked()
        {
            _settings.Save(capi);
            capi.ShowChatMessage(GuiComposerUtils.Translate("message.saved"));
            TryClose();
            return true;
        }

        private bool OnResetClicked()
        {
            _settings.ResetToDefaults();
            capi.ShowChatMessage(GuiComposerUtils.Translate("message.reset"));
            _settings.Save(capi);
            TryClose();
            return true;
        }


        public override string ToggleKeyCombinationCode { get; }
    }
}