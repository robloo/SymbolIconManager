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
            string websiteUrl = string.Empty;

            if (Enum.TryParse(value?.ToString(), ignoreCase: true, out IconSet iconSet))
            {
                switch (iconSet)
                {
                    case IconSet.FluentUISystemFilled:
                    case IconSet.FluentUISystemRegular:
                        websiteUrl = FluentUISystem.WebsiteUrl;
                        break;
                    case IconSet.LineAwesomeBrand:
                    case IconSet.LineAwesomeRegular:
                    case IconSet.LineAwesomeSolid:
                        websiteUrl = LineAwesome.WebsiteUrl;
                        break;
                    case IconSet.SegoeFluent:
                        websiteUrl = SegoeFluent.WebsiteUrl;
                        break;
                    case IconSet.SegoeMDL2Assets:
                        websiteUrl = SegoeMDL2Assets.WebsiteUrl;
                        break;
                    case IconSet.SegoeUISymbol:
                        websiteUrl = SegoeUISymbol.WebsiteUrl;
                        break;
                    case IconSet.WinJSSymbols:
                        websiteUrl = WinJSSymbols.WebsiteUrl;
                        break;
                }
            }

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
