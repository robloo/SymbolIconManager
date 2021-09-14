using IconManager.Models.Serialization;
using Newtonsoft.Json;

namespace IconManager
{
    /// <summary>
    /// Represents basic information for an icon.
    /// </summary>
    public interface IIcon
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
        IconSet IconSet { get; set; }

        /// <summary>
        /// Gets or sets the full name or description of the icon.
        /// The format is specific to each font or icon package.
        /// </summary>
        [JsonProperty(Order = 3)]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the 32-bit Unicode point of the icon.
        /// </summary>
        [JsonProperty(Order = 2)]
        [JsonConverter(typeof(HexStringConverter))]
        uint UnicodePoint { get; set; }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the value of the <see cref="UnicodePoint"/> formatted as a hexadecimal string.
        /// </summary>
        [JsonIgnore]
        string UnicodeString { get; }
    }
}
