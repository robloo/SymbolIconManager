using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using Avalonia;
using Avalonia.Platform;

namespace IconManager
{
    /// <summary>
    /// Contains a list of icon mappings along with methods to process them.
    /// </summary>
    public class IconMappingList : List<IconMapping>
    {
        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Merges any mappings from this list into the destination.
        /// Any matching mappings in the destination will be replaced by information in the source.
        /// Any mappings that exist in the source but not the destination will be added to the destination.
        /// </summary>
        /// <param name="destMappings">The destination mappings list to merge into.</param>
        public void MergeInto(IconMappingList destMappings)
        {
            foreach (IconMapping sourceMapping in this)
            {
                bool matchFound = false;
                foreach (IconMapping destMapping in destMappings)
                {
                    if (sourceMapping.Destination.IsUnicodeMatch(destMapping.Destination))
                    {
                        destMapping.Source               = sourceMapping.Source.Clone();
                        destMapping.GlyphMatchQuality    = sourceMapping.GlyphMatchQuality;
                        destMapping.MetaphorMatchQuality = sourceMapping.MetaphorMatchQuality;
                        destMapping.IsPlaceholder        = sourceMapping.IsPlaceholder;
                        // Exclude comments intentionally

                        matchFound = true;
                        // Do not break, update all duplicates (Yes, this is slow)
                    }
                }

                if (matchFound == false)
                {
                    destMappings.Add(sourceMapping.Clone());
                }
            }

            return;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given destination name.
        /// </summary>
        /// <param name="name">The name of the destination icon to search for.</param>
        /// <param name="ignoreCase">Whether to ignore character case of the name.</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindByDestinationName(string name, bool ignoreCase = true)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (ignoreCase &&
                    string.Equals(mapping.Destination.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(mapping);
                }
                else if (string.Equals(mapping.Destination.Name, name, StringComparison.Ordinal))
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given destination Unicode point.
        /// </summary>
        /// <param name="unicodePoint">The Unicode point of the destination icon to search for..</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindByDestinationUnicode(uint unicodePoint)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (mapping.Destination.UnicodePoint == unicodePoint)
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given source name.
        /// </summary>
        /// <param name="name">The name of the source icon to search for.</param>
        /// <param name="ignoreCase">Whether to ignore character case of the name.</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindBySourceName(string name, bool ignoreCase = true)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (ignoreCase &&
                    string.Equals(mapping.Source.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(mapping);
                }
                else if (string.Equals(mapping.Source.Name, name, StringComparison.Ordinal))
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given source Unicode point.
        /// </summary>
        /// <param name="unicodePoint">The Unicode point of the source icon to search for..</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindBySourceUnicode(uint unicodePoint)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (mapping.Source.UnicodePoint == unicodePoint)
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Sort each mapping alphabetically by source name.
        /// </summary>
        public void SortBySourceName()
        {
            this.Sort((x, y) =>
            {
                return string.Compare(x.Source.Name, y.Source.Name);
            });

            return;
        }

        /// <summary>
        /// Sort each mapping by source icon set and Unicode point.
        /// </summary>
        public void SortBySourceUnicode()
        {
            this.Sort((x, y) =>
            {
                if (x.Source.IconSet == y.Source.IconSet)
                {
                    return x.Source.UnicodePoint.CompareTo(y.Source.UnicodePoint);
                }
                else
                {
                    return x.Source.IconSet.CompareTo(y.Source.IconSet);
                }
            });

            return;
        }

        /// <summary>
        /// Sort each mapping alphabetically by destination name.
        /// </summary>
        public void SortByDestinationName()
        {
            this.Sort((x, y) =>
            {
                return string.Compare(x.Destination.Name, y.Destination.Name);
            });

            return;
        }

        /// <summary>
        /// Sort each mapping by destination icon set and Unicode point.
        /// </summary>
        public void SortByDestinationUnicode()
        {
            this.Sort((x, y) =>
            {
                if (x.Destination.IconSet == y.Destination.IconSet)
                {
                    return x.Destination.UnicodePoint.CompareTo(y.Destination.UnicodePoint);
                }
                else
                {
                    return x.Destination.IconSet.CompareTo(y.Destination.IconSet);
                }
            });

            return;
        }

        /// <summary>
        /// Ensures all icon names are updated in the mapping list.
        /// Any custom names for defined icon sets will be overwritten.
        /// Undefined icon sets are not modified.
        /// </summary>
        public void UpdateNames()
        {
            foreach (var mapping in this)
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
        /// Reprocesses the mappings list to repair and standardize each mapping.
        /// This will ensure names are updated, etc.
        /// </summary>
        public void Reprocess()
        {
            this.UpdateNames();

            // TODO: Remove duplicates?

            // TODO: For known IconSets, ensure Unicode point is within range

            return;
        }

        /***************************************************************************************
         *
         * Static Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Loads a known mapping list for the given icon set.
        /// This will be a 'Font Mapping File'.
        /// </summary>
        /// <param name="iconSet">The icon set to get the mapping list for.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(IconSet iconSet)
        {
            string resourcePath = string.Empty;

            switch (iconSet)
            {
                case IconSet.SegoeFluent:
                    resourcePath = "avares://IconManager/Data/Mappings/SegoeFluent.json";
                    break;
            }

            if (string.IsNullOrWhiteSpace(resourcePath) == false)
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                using (var sourceStream = assets.Open(new Uri(resourcePath)))
                {
                    return IconMappingList.Load(sourceStream);
                }
            }

            return new IconMappingList();
        }

        /// <summary>
        /// Loads a known mapping list for converting from the source icon set to the destination icon set.
        /// This will be an 'Icon Set Mapping File'.
        /// </summary>
        /// <param name="sourceIconSet">The source icon set to convert from.</param>
        /// <param name="destIconSet">The destination icon set to convert to.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(
            IconSet sourceIconSet,
            IconSet destIconSet)
        {
            string resourcePath = string.Empty;

            if ((sourceIconSet == IconSet.FluentUISystemFilled ||
                 sourceIconSet == IconSet.FluentUISystemRegular) &&
                destIconSet == IconSet.SegoeMDL2Assets)
            {
                resourcePath = "avares://IconManager/Data/Mappings/FluentUISystemToSegoeMDL2Assets.json";
            }
            else if (sourceIconSet == IconSet.SegoeUISymbol &&
                     destIconSet == IconSet.SegoeMDL2Assets)
            {
                resourcePath = "avares://IconManager/Data/Mappings/SegoeUISymbolToSegoeMDL2Assets.json";
            }

            if (string.IsNullOrWhiteSpace(resourcePath) == false)
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                using (var sourceStream = assets.Open(new Uri(resourcePath)))
                {
                    return IconMappingList.Load(sourceStream);
                }
            }

            return new IconMappingList();
        }

        /// <summary>
        /// Loads a mapping list from the given stream.
        /// </summary>
        /// <param name="jsonStream">The JSON formatted stream to load from.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(Stream jsonStream)
        {
            IconMappingList mappings = new IconMappingList();

            using (var reader = new StreamReader(jsonStream))
            {
                string jsonString = reader.ReadToEnd();
                var rawMappings = JsonConvert.DeserializeObject<IconMappingList>(jsonString);

                if (rawMappings != null)
                {
                    mappings = rawMappings;
                }
            }

            return mappings;
        }

        /// <summary>
        /// Saves the given mapping list to the given stream.
        /// </summary>
        /// <param name="mappings">The mapping list to save.</param>
        /// <param name="stream">The destination stream to write the JSON data to.</param>
        public static void Save(IconMappingList mappings, Stream stream)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            string jsonString = JsonConvert.SerializeObject(mappings, settings);

            if (string.IsNullOrEmpty(jsonString) == false)
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(jsonString);
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
