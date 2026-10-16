using IconManager.Core.Icons;
using Newtonsoft.Json;
using System.IO;

namespace IconManager.Core.Serialization
{
    /// <summary>
    /// Contains methods used to serialize an <see cref="IconMappingList"/> to/from JSON.
    /// </summary>
    public static class IconMappingSerializer
    {
        /// <summary>
        /// Loads a mapping list from the given stream.
        /// </summary>
        /// <param name="jsonStream">The JSON formatted stream to load from.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(Stream jsonStream)
        {
            var mappings = new IconMappingList();

            using (var reader = new StreamReader(jsonStream))
            {
                string jsonString = reader.ReadToEnd();
                var rawMappings = JsonConvert.DeserializeObject<IconMappingList>(jsonString);

                if (rawMappings is not null)
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
