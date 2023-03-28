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
                        // Only copy over the source if it is valid
                        if (sourceMapping.Source.IsValidForSource)
                        {
                            destMapping.Source               = sourceMapping.Source.Clone();
                            destMapping.GlyphMatchQuality    = sourceMapping.GlyphMatchQuality;
                            destMapping.MetaphorMatchQuality = sourceMapping.MetaphorMatchQuality;
                            destMapping.IsPlaceholder        = sourceMapping.IsPlaceholder;
                            // Exclude comments intentionally
                        }

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

            return IconMappingList.Load(resourcePath);
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

            return IconMappingList.Load(resourcePath);
        }

        /// <summary>
        /// Loads a mapping list from the given application resource path.
        /// </summary>
        /// <param name="jsonStream">The application resource path to load from.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath) == false)
            {
                var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();

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
    }
}
