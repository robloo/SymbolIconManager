using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;

namespace IconManager.Converters
{
    /// <summary>
    /// Gets a contrasting foreground brush (based on the active theme variant) for the given input color/brush.
    /// </summary>
    public class ForegroundContrastBrushConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the alpha channel threshold below which the default foreground color is always used.
        /// </summary>
        public byte AlphaThreshold { get; set; } = 128;

        /// <inheritdoc/>
        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            Color? backgroundColor = ConvertToColor(value);

            if (backgroundColor is null)
            {
                return BindingOperations.DoNothing;
            }

            // Get the current text foreground brushes from the theme
            // Warning: This assumes Fluent v2 theme is being used
            Brush? primaryBrush = this.GetThemeBrush("TextFillColorPrimaryBrush");
            Brush? inverseBrush = this.GetThemeBrush("TextFillColorInverseBrush");

            // As a fallback, just use black/white
            if (primaryBrush is null || inverseBrush is null)
            {
                if (Application.Current?.ActualThemeVariant == ThemeVariant.Dark)
                {
                    primaryBrush = new SolidColorBrush(Colors.White);
                    inverseBrush = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    primaryBrush = new SolidColorBrush(Colors.Black);
                    inverseBrush = new SolidColorBrush(Colors.White);
                }
            }

            Color? primaryBrushColor = ConvertToColor(primaryBrush);

            if (primaryBrushColor is null)
            {
                return BindingOperations.DoNothing;
            }

            // Identify the dark and light foreground brushes
            Brush lightBrush;
            Brush darkBrush;

            if (ColorHelper.GetRelativeLuminance(primaryBrushColor.Value) <= 0.5)
            {
                darkBrush  = primaryBrush;
                lightBrush = inverseBrush;
            }
            else
            {
                darkBrush  = inverseBrush;
                lightBrush = primaryBrush;
            }

            if (backgroundColor.Value.A < AlphaThreshold)
            {
                // If the transparency is less than the threshold, just use the default brush
                return primaryBrush;
            }
            else
            {
                // Chose a light/dark brush based on contrast to the base color
                return (ColorHelper.GetRelativeLuminance(backgroundColor.Value) <= 0.5)
                    ? lightBrush // Dark color, return light for contrast
                    : darkBrush; // Bright color, return dark for contrast
            }
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

        /// <summary>
        /// Finds any brush resource with the given key in the current application resources.
        /// </summary>
        private Brush? GetThemeBrush(string key)
        {
            if (Application.Current is null)
            {
                return null;
            }

            if (Application.Current.TryGetResource(
                key,
                Application.Current.ActualThemeVariant,
                out var resource))
            {
                return resource as Brush;
            }

            return null;
        }

        /// <summary>
        /// Attempts to convert the given value into a <see cref="Color"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to a <see cref="Color"/>; otherwise, null.</returns>
        private static Color? ConvertToColor(object? value)
        {
            if (value is uint uintValue)
            {
                return Color.FromUInt32(uintValue);
            }
            else if (value is int intValue)
            {
                return Color.FromUInt32(unchecked((uint)intValue));
            }
            else if (value is Color colorValue)
            {
                return colorValue;
            }
            else if (value is HsvColor hsvColorValue)
            {
                return hsvColorValue.ToRgb();
            }
            else if (value is SolidColorBrush solidColorBrushValue)
            {
                Color color = solidColorBrushValue.Color;
                byte alpha = color.A;

                if (solidColorBrushValue.Opacity != 1.0)
                {
                    // Blend the two alpha values together
                    alpha = (byte)Math.Round(
                        alpha * solidColorBrushValue.Opacity,
                        MidpointRounding.AwayFromZero);
                }

                return Color.FromArgb(
                    alpha,
                    color.R,
                    color.G,
                    color.B);
            }

            return null;
        }
    }
}
