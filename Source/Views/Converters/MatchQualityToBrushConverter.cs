using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace IconManager
{
    /// <summary>
    /// Converts a <see cref="MatchQuality"/> value into a corresponding perceived <see cref="SolidColorBrush"/>.
    /// </summary>
    public class MatchQualityToBrushConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is MatchQuality matchQuality)
            {
                switch (matchQuality)
                {
                    // Colors are from Flat UI theme
                    case MatchQuality.NoMatch:
                        return new SolidColorBrush(new Color(0xFF, 0xC0, 0x39, 0x2B)); // Dark red
                    case MatchQuality.Low:
                        return new SolidColorBrush(new Color(0xFF, 0xD9, 0x88, 0x80)); // Lighter red
                    case MatchQuality.Medium:
                        return new SolidColorBrush(new Color(0xFF, 0xD4, 0xEF, 0xDF)); // Very slightly green
                    case MatchQuality.High:
                        return new SolidColorBrush(new Color(0xFF, 0x7D, 0xCE, 0xA0)); // Lighter green
                    case MatchQuality.Exact:
                        return new SolidColorBrush(new Color(0xFF, 0x27, 0xAE, 0x60)); // Dark green
                }
            }

            return AvaloniaProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }
}
