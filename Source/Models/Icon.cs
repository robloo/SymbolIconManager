namespace IconManager
{
    /// <summary>
    /// Represents a single, generic icon.
    /// </summary>
    /// <remarks>
    /// This should usually not be used directly.
    /// Instead, use an icon class from an icon set.
    /// </remarks>
    public class Icon : IIcon
    {
        /// <inheritdoc/>
        public IconSet IconSet { get; set; } = IconSet.Undefined;

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string UnicodePoint { get; set; } = string.Empty;
    }
}
