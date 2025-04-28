#nullable enable
using System;

namespace HealthBar.Config
{
    /// <summary>
    /// Configure the health bar.
    /// </summary>
    public class HealthBarSettings
    {
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
    }
}