using System;
using IconManager.Core.Icons;
using Newtonsoft.Json;

namespace IconManager.Core.Serialization
{
    /// <summary>
    /// Converts a nullable <see cref="IconSet"/> to/from a JSON string.
    /// </summary>
    public class IconSetConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer)
        {
            string text = string.Empty;

            if (value is IconSet iconSet)
            {
                text = iconSet.ToString();
            }

            writer.WriteValue(text);

            return;
        }

        /// <inheritdoc/>
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            try
            {
                return Enum.Parse(typeof(IconSet), reader.Value?.ToString() ?? string.Empty);
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get => true;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }
}
