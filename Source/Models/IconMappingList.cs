using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace IconManager
{
    /// <summary>
    /// Contains a list of icon mappings along with methods to process them.
    /// </summary>
    public class IconMappingList : List<IconMapping>
    {
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
        /// Creates a brand-new <see cref="IconMappingList"/> including all icons is the specified
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
        /// Reprocesses an existing mappings list to repair and standardize each mapping.
        /// This will ensure names are updated, entries are sorted, etc.
        /// Warning: Unicode points are assumed valid.
        /// </summary>
        /// <param name="mappings">The mappings to reprocess.</param>
        public static void ReprocessMappings(IconMappingList mappings)
        {
            // Update names, assumes Unicode points are valid
            foreach (var mapping in mappings)
            {
                switch (mapping.Source.IconSet)
                {
                    case IconSet.SegoeFluent:
                        mapping.Source.Name = SegoeFluent.FindName(mapping.Source.UnicodePoint);
                        break;
                    case IconSet.SegoeMDL2Assets:
                        mapping.Source.Name = SegoeMDL2Assets.FindName(mapping.Source.UnicodePoint);
                        break;
                }

                switch (mapping.Destination.IconSet)
                {
                    case IconSet.SegoeFluent:
                        mapping.Destination.Name = SegoeFluent.FindName(mapping.Destination.UnicodePoint);
                        break;
                    case IconSet.SegoeMDL2Assets:
                        mapping.Destination.Name = SegoeMDL2Assets.FindName(mapping.Destination.UnicodePoint);
                        break;
                }
            }

            // Sort each mapping by destination icon set and Unicode point
            mappings.Sort((x, y) =>
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

            // TODO: Remove duplicates?

            return;
        }
    }
}
