namespace IconManager
{
    /// <summary>
    /// Represents basic, read-only information for an icon.
    /// </summary>
    /// <remarks>
    /// Make sure to keep members and comments in sync with <see cref="IIcon"/>.
    /// </remarks>
    public interface IReadOnlyIcon
    {
        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the <see cref="IconManager.IconSet"/> that contains the icon.
        /// </summary>
        IconSet IconSet { get; }

        /// <summary>
        /// Gets the full name or description of the icon.
        /// The format is specific to each font or icon package.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the 32-bit Unicode point of the icon.
        /// </summary>
        uint UnicodePoint { get; }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the value of the <see cref="UnicodePoint"/> formatted as a hexadecimal string.
        /// </summary>
        string UnicodeString { get; }
    }
}
