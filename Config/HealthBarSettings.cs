#nullable enable
using Vintagestory.API.Client;

namespace HealthBar.Config
{
    /// <summary>
    /// Configure the health bar.
    /// </summary>
    public class HealthBarSettings
    {
        #region General
        public bool Enabled { get; set; } = true;
        public bool ShowOnPlayer { get; set; } = true;
        
        public bool ShowHpText { get; set; } = true;
        
        #endregion
        #region Size and position

        /// <summary>Width as pixels</summary>
        public float BarWidth { get; set; } = 66f;

        /// <summary>Height as pixels</summary>
        public float BarHeight { get; set; } = 6.6f;

        /// <summary>Offset</summary>
        public float VerticalOffset { get; set; } = 22f;

        #endregion

        #region Animation

        /// <summary>Time (in seconds) for the bar to be fully displayed.</summary>
        public float FadeInSpeed { get; set; } = 0.3f;

        /// <summary>Time (in seconds) for the bar to fade completely.</summary>
        public float FadeOutSpeed { get; set; } = 0.5f;

        #endregion

        #region Thresholds and colors

        /// <summary>The percentage threshold below which a low health color is used.</summary>
        public float LowHealthThreshold { get; set; } = 0.25f;

        /// <summary>The percentage threshold below which the color of average health is used.</summary>
        public float MidHealthThreshold { get; set; } = 0.6f;

        /// <summary>Bar color at low health (hex).</summary>
        public string LowHealthColor { get; set; } = "#FF4444";

        /// <summary>Bar color at average health (hex).</summary>
        public string MidHealthColor { get; set; } = "#FFCC00";

        /// <summary>Bar color at full health (hex).</summary>
        public string FullHealthColor { get; set; } = "#44FF44";

        /// <summary>Frame color (hex).</summary>
        public string FrameColor { get; set; } = "#cccccc";

        #endregion
        
        public void Save(ICoreClientAPI capi)
        {
            capi.StoreModConfig(this, "mobhealthdisplay.json");
        }

        public void Load(ICoreClientAPI capi)
        {
            var loaded = capi.LoadModConfig<HealthBarSettings>("mobhealthdisplay.json");
            if (loaded != null)
            {
                this.Enabled = loaded.Enabled;
                this.ShowOnPlayer = loaded.ShowOnPlayer;
                this.ShowHpText = loaded.ShowHpText;
                this.BarWidth = loaded.BarWidth;
                this.BarHeight = loaded.BarHeight;
                this.VerticalOffset = loaded.VerticalOffset;
                this.FadeInSpeed = loaded.FadeInSpeed;
                this.FadeOutSpeed = loaded.FadeOutSpeed;
                this.LowHealthThreshold = loaded.LowHealthThreshold;
                this.MidHealthThreshold = loaded.MidHealthThreshold;
                this.LowHealthColor = loaded.LowHealthColor;
                this.MidHealthColor = loaded.MidHealthColor;
                this.FullHealthColor = loaded.FullHealthColor;
                this.FrameColor = loaded.FrameColor;
            }
        }

        public void ResetToDefaults()
        {
            var defaults = new HealthBarSettings();
            this.Enabled = defaults.Enabled;
            this.ShowOnPlayer = defaults.ShowOnPlayer;
            this.ShowHpText = defaults.ShowHpText;
            this.BarWidth = defaults.BarWidth;
            this.BarHeight = defaults.BarHeight;
            this.VerticalOffset = defaults.VerticalOffset;
            this.FadeInSpeed = defaults.FadeInSpeed;
            this.FadeOutSpeed = defaults.FadeOutSpeed;
            this.LowHealthThreshold = defaults.LowHealthThreshold;
            this.MidHealthThreshold = defaults.MidHealthThreshold;
            this.LowHealthColor = defaults.LowHealthColor;
            this.MidHealthColor = defaults.MidHealthColor;
            this.FullHealthColor = defaults.FullHealthColor;
            this.FrameColor = defaults.FrameColor;
        }
    }
}