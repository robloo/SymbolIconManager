using Avalonia.Data;
using Avalonia.Data.Converters;
using IconManager.Core.Icons;
using IconManager.IconSets;
using System;
using System.Globalization;
using Strings = IconManager.Resources.Strings;

namespace IconManager
{
    /// <summary>
    /// Gets the online website URL for the given <see cref="IconSet"/>.
    /// </summary>
    public class IconSetInfoConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            string? valueText = value?.ToString();
            string? parameterText = parameter?.ToString();

            IconSetFamily family =
                Enum.TryParse(valueText, ignoreCase: true, out IconSet iconSet)
                    ? iconSet.GetFamily()
                    : IconSetFamily.Undefined;

            if (family == IconSetFamily.Undefined)
            {
                Enum.TryParse(valueText, ignoreCase: true, out family);
            }

            // Exit if no icon set or icon set family is known
            if (family == IconSetFamily.Undefined)
            {
                return null;
            }

            if (Enum.TryParse(parameterText, ignoreCase: true, out IconSetInfoType infoType))
            {
                switch (infoType)
                {
                    case IconSetInfoType.Description:
                        return GetDescription(family);
                    case IconSetInfoType.Name:
                        return GetName(family);
                    case IconSetInfoType.WebsiteUrl:
                        return GetWebsiteUrl(family);
                }
            }

            // Even if no parameter is provided the default icon set family name will be returned
            return GetName(family);
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

        private static object? GetDescription(IconSetFamily family)
        {
            return family switch
            {
                IconSetFamily.FluentUISystem  => Strings.Resources.IconSetFamily_FluentUISystem_DescriptionText,
                IconSetFamily.LineAwesome     => Strings.Resources.IconSetFamily_LineAwesome_DescriptionText,
                IconSetFamily.SegoeFluent     => Strings.Resources.IconSetFamily_SegoeFluent_DescriptionText,
                IconSetFamily.SegoeMDL2Assets => Strings.Resources.IconSetFamily_SegoeMDL2Assets_DescriptionText,
                IconSetFamily.SegoeUISymbol   => Strings.Resources.IconSetFamily_SegoeUISymbol_DescriptionText,
                IconSetFamily.WinJSSymbols    => Strings.Resources.IconSetFamily_WinJSSymbols_DescriptionText,
                _ => null
            };
        }

        private static object? GetName(IconSetFamily family)
        {
            return family switch
            {
                IconSetFamily.FluentUISystem  => Strings.Resources.IconSetFamily_FluentUISystem_NameText,
                IconSetFamily.LineAwesome     => Strings.Resources.IconSetFamily_LineAwesome_NameText,
                IconSetFamily.SegoeFluent     => Strings.Resources.IconSetFamily_SegoeFluent_NameText,
                IconSetFamily.SegoeMDL2Assets => Strings.Resources.IconSetFamily_SegoeMDL2Assets_NameText,
                IconSetFamily.SegoeUISymbol   => Strings.Resources.IconSetFamily_SegoeUISymbol_NameText,
                IconSetFamily.WinJSSymbols    => Strings.Resources.IconSetFamily_WinJSSymbols_NameText,
                _ => null
            };
        }

        private static object? GetWebsiteUrl(IconSetFamily family)
        {
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
    }
}
