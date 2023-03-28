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
        /// Ensures all icon names are updated in the mapping list.
        /// Any custom names for defined icon sets will be overwritten.
        /// Undefined icon sets are not modified.
        /// </summary>
        /// <param name="mappings">The mapping list to process.</param>
        public static void UpdateNames(IconMappingList mappings)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Source.IconSet != IconSet.Undefined)
                {
                    mapping.Source.Name = IconSetBase.FindName(
                        mapping.Source.IconSet,
                        mapping.Source.UnicodePoint);
                }

                if (mapping.Destination.IconSet != IconSet.Undefined)
                {
                    mapping.Destination.Name = IconSetBase.FindName(
                        mapping.Destination.IconSet,
                        mapping.Destination.UnicodePoint);
                }
            }

            return;
        }

        /// <summary>
        /// Ensures all icons are updated to the latest versions.
        /// Any deprecated icons that have a replacement will be overwritten.
        /// Warning: It is best practice to ensure all names are updated before calling this method.
        /// </summary>
        /// <param name="mappings">The mapping list to process.</param>
        public static void UpdateDeprecatedIcons(IconMappingList mappings)
        {
            foreach (var mapping in mappings)
            {
                // Currently, only deprecated Fluent UI System icons are relevant
                if (mapping.Source.IconSet == IconSet.FluentUISystemFilled ||
                    mapping.Source.IconSet == IconSet.FluentUISystemRegular)
                {
                    var icon = new FluentUISystem.Icon()
                    {
                        Name         = mapping.Source.Name,
                        UnicodePoint = mapping.Source.UnicodePoint
                        // IconSet is set automatically with the name
                    };

                    // Just assume it is deprecated as a search would take just as long
                    var result = FluentUISystem.UpdateDeprecated(icon);

                    if (result.Item1)
                    {
                        mapping.Source = result.Item2.AsIcon();
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Reprocesses the mappings list to repair and standardize each mapping.
        /// This will ensure names are updated, etc.
        /// </summary>
        /// <param name="mappings">The mapping list to reprocess.</param>
        public static void Reprocess(IconMappingList mappings)
        {
            IconMappingUtilities.UpdateNames(mappings);

            // TODO: Remove duplicates?

            // TODO: For known IconSets, ensure Unicode point is within range

            return;
        }

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

        /// <summary>
        /// Creates a new <see cref="IconMappingList"/> including all icons is the specified
        /// <paramref name="destinationIconSet"/>.
        /// </summary>
        /// <param name="destinationIconSet">
        /// Defines the <see cref="IconSet"/> used to generate the full list of mappings,
        /// each mapping representing a single icon in the set.
        /// This is the common <see cref="IconSet"/> used in the destination of all mappings.
        /// </param>
        /// <param name="sourceIconSet">
        /// An optional value used to set the icon set used as the source for all icons.
        /// Since it is common for multiple sources to be used, this should almost never be set.
        /// </param>
        /// <param name="baseMappings">
        /// An optional collection of base mappings used to set initial mapping values.
        /// Order of lists is important, the first mapping match will stop the search.
        /// Place primary, higher quality mapping lists before secondary ones.
        /// </param>
        /// <returns>A new icon mapping list.</returns>
        public static IconMappingList InitNewMappings(
            IconSet destinationIconSet,
            IconSet sourceIconSet = IconSet.Undefined,
            List<IconMappingList>? baseMappings = null)
        {
            var icons = IconSetBase.GetIcons(destinationIconSet);
            IconMappingList mappings = new IconMappingList();

            for (int i = 0; i < icons.Count; i++)
            {
                IconMapping? baseMapping = null;

                var mapping = new IconMapping()
                {
                    Destination = new Icon()
                    {
                        IconSet      = icons[i].IconSet,
                        Name         = icons[i].Name,
                        UnicodePoint = icons[i].UnicodePoint
                    },
                    Source = new Icon()
                    {
                        IconSet = sourceIconSet, // Undefined will equal default
                    }
                    // Leave metadata unset and the default value
                };

                // Attempt to find a matching base mapping
                // Only the first match is used, order of lists is important
                if (baseMappings != null)
                {
                    for (int j = 0; j < baseMappings.Count; j++)
                    {
                        for (int k = 0; k < baseMappings[j].Count; k++)
                        {
                            if (baseMappings[j][k].Destination.IsUnicodeMatch(mapping.Destination))
                            {
                                if (mapping.Source.IconSet == IconSet.Undefined ||
                                    (mapping.Source.IconSet != IconSet.Undefined &&
                                     baseMappings[j][k].Source.IconSet == mapping.Source.IconSet))
                                {
                                    // Match found
                                    baseMapping = baseMappings[j][k];
                                    break;
                                }
                            }
                        }

                        if (baseMapping != null)
                        {
                            break;
                        }
                    }
                }

                // Copy over information from the base mapping
                if (baseMapping != null)
                {
                    // Copy everything but the Destination
                    // Even the destination name isn't copied as the new name
                    // generated above is considered more accurate.
                    mapping.Source               = baseMapping.Source.Clone();
                    mapping.GlyphMatchQuality    = baseMapping.GlyphMatchQuality;
                    mapping.MetaphorMatchQuality = baseMapping.MetaphorMatchQuality;
                    mapping.IsPlaceholder        = baseMapping.IsPlaceholder;
                    mapping.Comments             = baseMapping.Comments;
                }

                mappings.Add(mapping);
            }

            return mappings;
        }

        /// <summary>
        /// Creates a new, specialized <see cref="IconMappingList"/> of all icons in the specified
        /// <paramref name="iconSet"/> mapped to themselves (identity mapping).
        /// </summary>
        /// <remarks>
        /// For supported icon sets, this specialized mapping can be used to completely rebuild fonts from source glyphs.
        /// For example, it can build the FluentUISystem fonts using the source SVG images.
        /// </remarks>
        /// <param name="iconSet">
        /// The <see cref="IconSet"/> to generated the identify mappings for.
        /// </param>
        /// <returns>A new identity icon mapping list.</returns>
        public static IconMappingList InitNewIdentityMappings(
            IconSet iconSet)
        {
            var icons = IconSetBase.GetIcons(iconSet);
            IconMappingList mappings = new IconMappingList();

            for (int i = 0; i < icons.Count; i++)
            {
                var mapping = new IconMapping()
                {
                    Destination = new Icon()
                    {
                        IconSet      = icons[i].IconSet,
                        Name         = icons[i].Name,
                        UnicodePoint = icons[i].UnicodePoint
                    },
                    Source = new Icon()
                    {
                        IconSet      = icons[i].IconSet,
                        Name         = icons[i].Name,
                        UnicodePoint = icons[i].UnicodePoint
                    },
                    GlyphMatchQuality    = MatchQuality.Exact,
                    MetaphorMatchQuality = MatchQuality.Exact,
                    IsPlaceholder        = false,
                    Comments             = string.Empty
                };

                mappings.Add(mapping);
            }

            return mappings;
        }
    }
}
