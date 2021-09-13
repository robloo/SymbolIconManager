namespace IconManager
{
    /// <summary>
    /// Represents basic information for an icon.
    /// </summary>
    public interface IIcon
    {
        /// <summary>
        /// Gets or sets the <see cref="IconManager.IconSet"/> that contains the icon.
        /// </summary>
        /// <remarks>
        /// This should be used to interpret and differentiate the <see cref="UnicodePoint"/>.
        /// </remarks>
        IconSet IconSet { get; set; }

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
