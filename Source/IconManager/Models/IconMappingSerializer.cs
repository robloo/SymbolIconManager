using Avalonia.Platform;
using IconManager.Core.Icons;
using Newtonsoft.Json;
using System;
using System.IO;

namespace IconManager
{
    /// <summary>
    /// Contains methods used to serialize an <see cref="IconMappingList"/> to/from JSON.
    /// </summary>
    public static class IconMappingSerializer
    {
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
                    resourcePath = IconSets.Paths.SegoeFluentMappings;
                    break;
            }

            return IconMappingSerializer.Load(resourcePath);
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
                resourcePath = IconSets.Paths.FluentUISystemToSegoeMDL2AssetsMappings;
            }
            else if (sourceIconSet == IconSet.SegoeUISymbol &&
                     destIconSet == IconSet.SegoeMDL2Assets)
            {
                resourcePath = IconSets.Paths.SegoeUISymbolToSegoeMDL2AssetsMappings;
            }

            return IconMappingSerializer.Load(resourcePath);
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
                using (var sourceStream = AssetLoader.Open(new Uri(resourcePath)))
                {
                    return IconMappingSerializer.Load(sourceStream);
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
