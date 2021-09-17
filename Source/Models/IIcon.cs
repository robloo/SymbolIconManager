using IconManager.Models.Serialization;
using Newtonsoft.Json;

namespace IconManager
{
    /// <summary>
    /// Represents basic information for an icon.
    /// </summary>
    /// <remarks>
    /// Make sure to keep members and comments in sync with <see cref="IReadOnlyIcon"/>.
    /// </remarks>
    public interface IIcon : IReadOnlyIcon
    {
        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the <see cref="IconManager.IconSet"/> that contains the icon.
        /// </summary>
        /// <remarks>
        /// This should be used to interpret and differentiate the <see cref="UnicodePoint"/>.
        /// </remarks>
        [JsonProperty(Order = 1)]
        [JsonConverter(typeof(IconSetConverter))]
        new IconSet IconSet { get; set; }

        /// <summary>
        /// Gets or sets the full name or description of the icon.
        /// The format is specific to each font or icon package.
        /// </summary>
        [JsonProperty(Order = 3)]
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets the 32-bit Unicode point of the icon.
        /// </summary>
        [JsonProperty(Order = 2)]
        [JsonConverter(typeof(HexStringConverter))]
        new uint UnicodePoint { get; set; }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the value of the <see cref="UnicodePoint"/> formatted as a hexadecimal string.
        /// This value is intended for display to users and export in mappings files.
        /// </summary>
        [JsonIgnore]
        new string UnicodeHexString { get; }
    }
}
