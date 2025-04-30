using HealthBar.Behaviors;
using HealthBar.Config;
using HealthBar.Gui;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HealthBar
{
    public class HealthBarMod : ModSystem
    {
        public static HealthBarSettings Settings { get; private set; } = new HealthBarSettings();
        private ICoreClientAPI _capi;
        private GuiDialog _guiSettings;
        
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            Settings = api.LoadModConfig<HealthBarSettings>("mobhealthdisplay.json") ?? new HealthBarSettings();
            api.StoreModConfig(Settings, "mobhealthdisplay.json");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            _capi = api;
            _guiSettings = new GuiSettings(_capi, Settings, "health:bar:config");
            RegisterBehavior();
            RegisterHotKeys();
        }

        private void RegisterBehavior()
        {
            _capi.RegisterEntityBehaviorClass("mobhealthdisplay", typeof(HealthBarBehavior));
            _capi.Event.PlayerEntitySpawn += player =>
                player.Entity.AddBehavior(new HealthBarBehavior(player.Entity));
        }

        private void RegisterHotKeys()
        {
            _capi.Input.RegisterHotKey("health:bar:config", "Health bar config", GlKeys.BackSlash);
            _capi.Input.SetHotKeyHandler("health:bar:config", _onConfigChanged);
        }

        private bool _onConfigChanged(KeyCombination keyCombination)
        {
            _capi.Logger.Notification("Health bar config");
            if (_guiSettings.IsOpened()) _guiSettings.TryClose();
            else _guiSettings.TryOpen();
        
            return true;
        }
    }
}