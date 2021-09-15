using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace IconManager
{
    /// <summary>
    /// Converts the state of quality of an icon mapping into a corresponding perceived <see cref="SolidColorBrush"/>.
    /// </summary>
    public class IconMappingStateToBrushConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var poorMappingBrush = new SolidColorBrush(new Color(0xFF, 0xF2, 0xD7, 0xD5));
            IconMapping? mapping = null;

            if (value is IconMapping iconMapping)
            {
                mapping = iconMapping;
            }
            else if (value is IconMappingViewModel iconMappingViewModel)
            {
                mapping = iconMappingViewModel.AsIconMapping();
            }

            if (mapping != null)
            {
                if (mapping.IsValid == false ||
                    mapping.IsPlaceholder)
                {
                    return poorMappingBrush;
                }
            }

            return AvaloniaProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }
}
