using System;
using Newtonsoft.Json;

namespace IconManager.Models.Serialization
{
    /// <summary>
    /// Converts a nullable <see cref="MatchQuality"/> to/from a JSON string.
    /// </summary>
    public class MatchQualityConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer)
        {
            string text = string.Empty;

            if (value is MatchQuality matchQuality)
            {
                text = matchQuality.ToString();
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
                return Enum.Parse(typeof(MatchQuality), reader.Value?.ToString() ?? string.Empty);
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
