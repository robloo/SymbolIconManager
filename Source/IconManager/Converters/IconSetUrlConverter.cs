using Avalonia.Data;
using Avalonia.Data.Converters;
using IconManager.Core.Icons;
using IconManager.IconSets;
using System;
using System.Globalization;

namespace IconManager
{
    /// <summary>
    /// Gets the online website URL for the given <see cref="IconSet"/>.
    /// </summary>
    public class IconSetUrlConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            string? valueText = value?.ToString();

            IconSetFamily family =
                Enum.TryParse(valueText, ignoreCase: true, out IconSet iconSet)
                    ? iconSet.GetFamily()
                    : IconSetFamily.Undefined;

            if (family == IconSetFamily.Undefined)
            {
                Enum.TryParse(valueText, ignoreCase: true, out family);
            }

            string? websiteUrl = family switch
            {
                IconSetFamily.FluentUISystem  => FluentUISystem.WebsiteUrl,
                IconSetFamily.LineAwesome     => LineAwesome.WebsiteUrl,
                IconSetFamily.SegoeFluent     => SegoeFluent.WebsiteUrl,
                IconSetFamily.SegoeMDL2Assets => SegoeMDL2Assets.WebsiteUrl,
                IconSetFamily.SegoeUISymbol   => SegoeUISymbol.WebsiteUrl,
                IconSetFamily.WinJSSymbols    => WinJSSymbols.WebsiteUrl,
                _ => null
            };

            if (!string.IsNullOrEmpty(websiteUrl))
            {
                if (Uri.TryCreate(websiteUrl, UriKind.RelativeOrAbsolute, out Uri? uri))
                {
                    return uri;
                }
            }

            return null;
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
