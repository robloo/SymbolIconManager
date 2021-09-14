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

        public static void InitNewMapping(
            IconSet destinationIconSet,
            IconSet sourceIconSet = IconSet.Undefined,
            IconMappingList? baseMappings = null)
        {
            return;
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


            return;
        }
    }
}
