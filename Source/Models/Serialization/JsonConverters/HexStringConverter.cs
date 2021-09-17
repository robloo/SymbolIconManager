using System;
using Newtonsoft.Json;

namespace IconManager.Models.Serialization
{
    /// <summary>
    /// Converts a nullable <see cref="uint"/> to/from a hexadecimal formatted JSON string.
    /// </summary>
    public class HexStringConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer)
        {
            string text = string.Empty;

            if (value is uint integer)
            {
                text = Icon.ToUnicodeHexString(integer);
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
                string value = reader.Value?.ToString() ?? string.Empty;

                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToUInt32(value.Substring(2), 16);
                }
                else
                {
                    return Convert.ToUInt32(value, 16);
                }
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
