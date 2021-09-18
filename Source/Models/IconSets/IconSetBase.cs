using System.Collections.Generic;

namespace IconManager
{
    /// <summary>
    /// Contains a common base class shared by all icon set implementations.
    /// Common methods for working across multiple icon sets are also included.
    /// </summary>
    public class IconSetBase
    {
        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Finds the name of the icon with the given Unicode point in the defined <see cref="IconSet"/>.
        /// </summary>
        /// <param name="iconSet">The icon set to search within.</param>
        /// <param name="unicodePoint">The Unicode point of the icon.</param>
        /// <returns>The name of the icon; otherwise, false.</returns>
        public static string FindName(IconSet iconSet, uint unicodePoint)
        {
            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                    return FluentUISystem.FindName(unicodePoint, FluentUISystem.IconTheme.Filled);
                case IconSet.FluentUISystemRegular:
                    return FluentUISystem.FindName(unicodePoint, FluentUISystem.IconTheme.Regular);
                case IconSet.LineAwesomeBrand:
                    return LineAwesome.FindName(unicodePoint, LineAwesome.IconStyle.Brand);
                case IconSet.LineAwesomeRegular:
                    return LineAwesome.FindName(unicodePoint, LineAwesome.IconStyle.Regular);
                case IconSet.LineAwesomeSolid:
                    return LineAwesome.FindName(unicodePoint, LineAwesome.IconStyle.Solid);
                case IconSet.SegoeFluent:
                    return SegoeFluent.FindName(unicodePoint);
                case IconSet.SegoeMDL2Assets:
                    return SegoeMDL2Assets.FindName(unicodePoint);
                case IconSet.SegoeUISymbol:
                    // Not currently supported
                    break;
                case IconSet.WinJSSymbols:
                    return WinJSSymbols.FindName(unicodePoint);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets all icons in the defined <see cref="IconSet"/>.
        /// </summary>
        /// <param name="iconSet">The icon set to list icons for.</param>
        /// <returns>All icons in the defined icon set.</returns>
        public static IReadOnlyList<IReadOnlyIcon> GetIcons(IconSet iconSet)
        {
            IReadOnlyList<IReadOnlyIcon>? icons = null;

            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                    icons = FluentUISystem.GetIcons(FluentUISystem.IconTheme.Filled);
                    break;
                case IconSet.FluentUISystemRegular:
                    icons = FluentUISystem.GetIcons(FluentUISystem.IconTheme.Regular);
                    break;
                case IconSet.LineAwesomeBrand:
                    icons = LineAwesome.GetIcons(LineAwesome.IconStyle.Brand);
                    break;
                case IconSet.LineAwesomeRegular:
                    icons = LineAwesome.GetIcons(LineAwesome.IconStyle.Regular);
                    break;
                case IconSet.LineAwesomeSolid:
                    icons = LineAwesome.GetIcons(LineAwesome.IconStyle.Solid);
                    break;
                case IconSet.SegoeFluent:
                    icons = SegoeFluent.Icons;
                    break;
                case IconSet.SegoeMDL2Assets:
                    icons = SegoeMDL2Assets.Icons;
                    break;
                case IconSet.SegoeUISymbol:
                    // Not currently supported
                    break;
                case IconSet.WinJSSymbols:
                    icons = WinJSSymbols.Icons;
                    break;
            }

            if (icons != null)
            {
                return icons;
            }
            else
            {
                return new List<IReadOnlyIcon>().AsReadOnly();
            }
        }
    }
}
