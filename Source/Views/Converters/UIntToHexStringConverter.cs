using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace IconManager
{
    /// <summary>
    /// Converts an unsigned integer value into a hexadecimal formatted string and vice versa.
    /// </summary>
    public class UIntToHexStringConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is uint unsignedInt)
            {
                return Icon.ToUnicodeHexString(unsignedInt);
            }
            else if (value is int integer)
            {
                if (integer >= 0)
                {
                    return Icon.ToUnicodeHexString(System.Convert.ToUInt32(integer));
                }
                else
                {
                    return AvaloniaProperty.UnsetValue;
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
            if (value is string str)
            {
                try
                {
                    return System.Convert.ToUInt32(str, 16);
                }
                catch
                {
                    return AvaloniaProperty.UnsetValue;
                }
            }
            
            return AvaloniaProperty.UnsetValue;
        }
    }
}
