using IconManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IconManager.Utilities
{
    /// <summary>
    /// Specialized helper methods useful when working with icon mappings.
    /// </summary>
    public class IconMappingUtilities
    {
        /// <summary>
        /// Attempts to repair outdated or invalid source icon UnicodePoints in the
        /// given mappings list.
        /// </summary>
        /// <remarks>
        /// This is a slow process as all source glyphs will attempt to be loaded.
        /// </remarks>
        /// <param name="mappings">The mapping list to repair.</param>
        public static async Task RepairMappingListSourceUnicodePoints(IconMappingList mappings)
        {
            var filled = IconSetBase.GetIcons(IconSet.FluentUISystemFilled);
            var regular = IconSetBase.GetIcons(IconSet.FluentUISystemRegular);

            // Repair source UnicodePoint by matching icon Name
            foreach (var mapping in mappings)
            {
                if (mapping.Source.IsValidForSource)
                {
                    var glyph = await GlyphProvider.GetGlyphSourceStreamAsync(
                        mapping.Source.IconSet,
                        mapping.Source.UnicodePoint);

                    bool isGlyphValid = true;
                    if (glyph == null ||
                        glyph.Length == 0)
                    {
                        isGlyphValid = false;
                    }

                    if (mapping.Source.UnicodePoint > 0 &&
                        isGlyphValid == false)
                    {
                        IReadOnlyList<IReadOnlyIcon>? newestIcons = null;

                        if (mapping.Source.IconSet == IconSet.FluentUISystemFilled)
                        {
                            newestIcons = filled;
                        }
                        else if (mapping.Source.IconSet == IconSet.FluentUISystemRegular)
                        {
                            newestIcons = regular;
                        }

                        if (newestIcons != null)
                        {
                            foreach (var icon in newestIcons)
                            {
                                if (icon.Name == mapping.Source.Name &&
                                    icon.UnicodePoint != mapping.Source.UnicodePoint)
                                {
                                    mapping.Source.UnicodePoint = icon.UnicodePoint;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return;
        }
    }
}
