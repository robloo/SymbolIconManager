using System;

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
        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public IconSet IconSet { get; set; } = IconSet.Undefined;

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public uint UnicodePoint { get; set; } = 0;

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public string UnicodeString
        {
            get => ToUnicodeString(this.UnicodePoint);
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Converts the given numerical Unicode point to a hexadecimal formatted string (with no prefix).
        /// </summary>
        /// <param name="unicodePoint">The Unicode point to convert to a string.</param>
        public static string ToUnicodeString(uint unicodePoint)
        {
            if (unicodePoint <= 0xFFFF)
            {
                return unicodePoint.ToString("X4");
            }
            else
            {
                return unicodePoint.ToString("X");
            }
        }
    }
}
