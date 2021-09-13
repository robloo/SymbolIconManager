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
        /// Reprocesses an existing mapping file to repair each mapping and standardize formatting.
        /// This will ensure names are updated, entries are sorted, etc.
        /// </summary>
        public static void ReprocessMappingFile(string filePath)
        {
            return;
        }
    }
}
