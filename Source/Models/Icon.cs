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
        public string UnicodeHexString
        {
            get => ToUnicodeHexString(this.UnicodePoint);
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="Icon"/> instance from this instance's values.
        /// </summary>
        /// <returns>The cloned <see cref="Icon"/>.</returns>
        public Icon Clone()
        {
            Icon clone = new Icon()
            {
                IconSet      = this.IconSet,
                Name         = this.Name,
                UnicodePoint = this.UnicodePoint
            };

            return clone;
        }

        /// <summary>
        /// Determines if this icon is considered a Unicode match with the other icon.
        /// Unicode match means both <see cref="UnicodePoint"/> and <see cref="IconSet"/>
        /// are exactly the same and neither are undefined.
        /// </summary>
        /// <remarks>
        /// This method is useful for matching icons during mapping and font generation.
        /// </remarks>
        /// <param name="other">The other icon to compare with.</param>
        /// <returns>True if this icon is considered an exact Unicode match with the other;
        /// otherwise, false.</returns>
        public bool IsUnicodeMatch(Icon other)
        {
            if (this.UnicodePoint != 0 &&
                other.UnicodePoint != 0)
            {
                // IconSet.Undefined is allowed but must be the same for both
                if (this.IconSet == other.IconSet &&
                    this.UnicodePoint == other.UnicodePoint)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the given numerical Unicode point to a hexadecimal formatted string (with no prefix).
        /// </summary>
        /// <param name="unicodePoint">The Unicode point to convert to a string.</param>
        public static string ToUnicodeHexString(uint unicodePoint)
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
