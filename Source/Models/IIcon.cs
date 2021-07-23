namespace IconManager
{
    /// <summary>
    /// Represents basic information for an icon.
    /// </summary>
    public interface IIcon
    {
        /// <summary>
        /// Gets or sets the full name or description of the icon.
        /// The format is specific to each font or icon package.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the Unicode point of the icon.
        /// This must be a hexadecimal format string with no prefix.
        /// </summary>
        string UnicodePoint { get; set; }
    }
}
