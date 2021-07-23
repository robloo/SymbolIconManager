namespace IconManager
{
    /// <summary>
    /// Defines a mapping from the source icon to the destination icon.
    /// Mappings are commonly between different symbol icon sets or different sized glyphs
    /// within the same icon set.
    /// </summary>
    public class IconMapping
    {
        public IconMapping(IIcon source, IIcon destination)
        {
            this.Source      = source;
            this.Destination = destination;
        }

        /// <summary>
        /// Gets or sets the source icon to map from.
        /// This must contain a Unicode point in hexadecimal format with no prefix.
        /// </summary>
        public IIcon Source { get; set; }

        /// <summary>
        /// Gets or sets the destination icon to map to.
        /// This must contain a Unicode point in hexadecimal format with no prefix.
        /// </summary>
        public IIcon Destination { get; set; }
    }
}
