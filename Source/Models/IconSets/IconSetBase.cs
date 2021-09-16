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
