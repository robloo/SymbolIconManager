using IconManager.Core.Icons;
using static IconManager.LineAwesome;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in the Line Awesome icon set.
    /// </summary>
    public class LineAwesomeIcon : IIcon
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="LineAwesomeIcon"/> class.
        /// </summary>
        public LineAwesomeIcon()
        {
        }

        /***************************************************************************************
        *
        * Property Accessors
        *
        ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public IconSet IconSet
        {
            get
            {
                if (this.Style == IconStyle.Brand)
                {
                    return IconSet.LineAwesomeBrand;
                }
                else if (this.Style == IconStyle.Solid)
                {
                    return IconSet.LineAwesomeSolid;
                }
                else
                {
                    return IconSet.LineAwesomeRegular;
                }
            }
            set { /* Do nothing */ }
        }

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Line Awesome style of the icon.
        /// </summary>
        public IconStyle Style { get; set; } = IconStyle.Regular;

        /// <inheritdoc/>
        public uint UnicodePoint { get; set; } = 0;

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public string UnicodeHexString
        {
            get => Icon.ToUnicodeHexString(this.UnicodePoint);
        }

        /***************************************************************************************
        *
        * Methods
        *
        ***************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="LineAwesomeIcon"/> instance from this instance's values.
        /// </summary>
        /// <returns>The cloned <see cref="LineAwesomeIcon"/>.</returns>
        public LineAwesomeIcon Clone()
        {
            var clone = new LineAwesomeIcon()
            {
                Name         = this.Name,
                Style        = this.Style,
                UnicodePoint = this.UnicodePoint
            };

            return clone;
        }

        /// <summary>
        /// Converts this <see cref="LineAwesomeIcon"/> into a standard <see cref="Icon"/>.
        /// </summary>
        /// <remarks>
        /// This is sometimes needed because <see cref="LineAwesomeIcon"/> does not derive from
        /// <see cref="Icon"/> like most other icons do. It only implements the interface.
        /// </remarks>
        /// <returns>A new <see cref="Icon"/>.</returns>
        public Icon AsIcon()
        {
            return new Icon()
            {
                IconSet      = this.IconSet,
                Name         = this.Name,
                UnicodePoint = this.UnicodePoint
            };
        }
    }
}
