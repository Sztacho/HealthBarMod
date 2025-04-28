#nullable enable
using System;

namespace HealthBar.Config
{
    /// <summary>
    /// Konfigurowalne parametry paska zdrowia.
    /// </summary>
    public class HealthBarSettings
    {
        #region Rozmiar i pozycja

        /// <summary>Szerokość paska w pikselach.</summary>
        public float BarWidth { get; set; } = 66f;

        /// <summary>Wysokość paska w pikselach.</summary>
        public float BarHeight { get; set; } = 6.6f;

        /// <summary>Offset pionowy względem pozycji encji.</summary>
        public float VerticalOffset { get; set; } = 22f;

        #endregion

        #region Animacje fade

        /// <summary>Czas (w sekundach) na pełne wyświetlenie paska.</summary>
        public float FadeInSpeed { get; set; } = 0.3f;

        /// <summary>Czas (w sekundach) na całkowite zanikanie paska.</summary>
        public float FadeOutSpeed { get; set; } = 0.5f;

        #endregion

        #region Progi i kolory

        /// <summary>Próg procentowy, poniżej którego używany jest kolor niskiego zdrowia.</summary>
        public float LowHealthThreshold { get; set; } = 0.25f;

        /// <summary>Próg procentowy, poniżej którego używany jest kolor średniego zdrowia.</summary>
        public float MidHealthThreshold { get; set; } = 0.6f;

        /// <summary>Kolor paska przy niskim zdrowiu (hex).</summary>
        public string LowHealthColor { get; set; } = "#FF4444";

        /// <summary>Kolor paska przy średnim zdrowiu (hex).</summary>
        public string MidHealthColor { get; set; } = "#FFCC00";

        /// <summary>Kolor paska przy pełnym zdrowiu (hex).</summary>
        public string FullHealthColor { get; set; } = "#44FF44";

        #endregion
    }
}