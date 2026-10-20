using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IconManager.Core.Icons;
using System;
using System.Globalization;

namespace IconManager.Converters
{
    /// <summary>
    /// Converts a <see cref="MatchQuality"/> value into a corresponding perceived <see cref="SolidColorBrush"/>.
    /// </summary>
    public class MatchQualityToBrushConverter : IValueConverter
    {
        // Original colors are from the Flat UI theme
        private static readonly Color OriginalDarkRedColor     = Color.FromUInt32(0xFFC0392B);
        private static readonly Color OriginalLightRedColor    = Color.FromUInt32(0xFFD98880);
        private static readonly Color OriginalSlightGreenColor = Color.FromUInt32(0xFFD4EFDF); // Very slightly green
        private static readonly Color OriginalLightGreenColor  = Color.FromUInt32(0xFF7DCEA0);
        private static readonly Color OriginalDarkGreenColor   = Color.FromUInt32(0xFF27AE60);

        // Final colors have the uniform 'white' background replaced with alpha
        // This allows them to work in both light/dark theme variants
        private static readonly Color DarkRedColor     = Color.FromUInt32(0xD4B31100);
        private static readonly Color LightRedColor    = Color.FromUInt32(0x7FB31000);
        private static readonly Color SlightGreenColor = Color.FromUInt32(0x2B00A041); // Very slightly green
        private static readonly Color LightGreenColor  = Color.FromUInt32(0x82009F45);
        private static readonly Color DarkGreenColor   = Color.FromUInt32(0xD8009F43);

        /// <inheritdoc/>
        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is MatchQuality matchQuality)
            {
                switch (matchQuality)
                {
                    case MatchQuality.NoMatch:
                        return new SolidColorBrush(DarkRedColor);
                    case MatchQuality.Low:
                        return new SolidColorBrush(LightRedColor);
                    case MatchQuality.Medium:
                        return new SolidColorBrush(SlightGreenColor);
                    case MatchQuality.High:
                        return new SolidColorBrush(LightGreenColor);
                    case MatchQuality.Exact:
                        return new SolidColorBrush(DarkGreenColor);
                }
            }

            return BindingOperations.DoNothing;
        }

        /// <inheritdoc/>
        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
