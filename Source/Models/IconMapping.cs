using IconManager.Models.Serialization;
using Newtonsoft.Json;

namespace IconManager
{
    /// <summary>
    /// Defines a mapping from the source icon to the destination icon.
    /// Mappings are commonly between different symbol icon sets or different sized glyphs
    /// within the same icon set.
    /// </summary>
    public class IconMapping
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconMapping()
        {
            this.Source               = new Icon();
            this.Destination          = new Icon();
            this.GlyphMatchQuality    = MatchQuality.NoMatch;
            this.MetaphorMatchQuality = MatchQuality.NoMatch;
            this.IsPlaceholder        = false;
            this.Comments             = string.Empty;
        }

        public IconMapping(Icon source, Icon destination)
        {
            this.Source               = source;
            this.Destination          = destination;
            this.GlyphMatchQuality    = MatchQuality.NoMatch;
            this.MetaphorMatchQuality = MatchQuality.NoMatch;
            this.IsPlaceholder        = false;
            this.Comments             = string.Empty;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the source icon to map from.
        /// </summary>
        /// <remarks>
        /// Originally this supported any <see cref="IIcon"/> types.
        /// However, serialization would then serialize all properties in the derived type as well.
        /// It is best to forcefully require <see cref="IIcon"/> as it is the only type supported
        /// in mapping serialization.
        /// </remarks>
        [JsonProperty(Order = 2)]
        public Icon Source { get; set; }

        /// <summary>
        /// Gets or sets the destination icon to map to.
        /// </summary>
        /// <remarks>
        /// Originally this supported any <see cref="IIcon"/> types.
        /// However, serialization would then serialize all properties in the derived type as well.
        /// It is best to forcefully require <see cref="IIcon"/> as it is the only type supported
        /// in mapping serialization.
        /// </remarks>
        [JsonProperty(Order = 1)]
        public Icon Destination { get; set; }

        /// <summary>
        /// Gets or sets the quality of match for the glyphs of the two icons.
        /// </summary>
        [JsonProperty(Order = 3)]
        [JsonConverter(typeof(MatchQualityConverter))]
        public MatchQuality GlyphMatchQuality { get; set; }

        /// <summary>
        /// Gets or sets the quality of match for the metaphor (purpose) of the two icons.
        /// </summary>
        [JsonProperty(Order = 4)]
        [JsonConverter(typeof(MatchQualityConverter))]
        public MatchQuality MetaphorMatchQuality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mapping had no direct equivalent
        /// so a generic substitution was made.
        /// </summary>
        [JsonProperty(Order = 5)]
        public bool IsPlaceholder { get; set; }

        /// <summary>
        /// Gets or sets comments describing the mapping and any decisions or special considerations.
        /// </summary>
        [JsonProperty(Order = 6)]
        public string Comments { get; set; }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating weather the mapping is considered valid
        /// (both a source and destination icon are defined).
        /// </summary>
        [JsonIgnore]
        public bool IsValid
        {
            get => this.Source != null &&
                   this.Source.IconSet != IconSet.Undefined &&
                   this.Source.UnicodePoint != 0 &&
                   this.Destination != null &&
                   this.Destination.IconSet != IconSet.Undefined &&
                   this.Destination.UnicodePoint != 0;
        }
    }
}
