using IconManager.Core.Icons;
using static IconManager.FluentUISystem;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in the Fluent UI System.
    /// </summary>
    public class FluentUISystemIcon : FluentUISystemIconName, IIcon
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
        public IconSet IconSet
        {
            get
            {
                if (this.Theme == IconTheme.Filled)
                {
                    return IconSet.FluentUISystemFilled;
                }
                else
                {
                    return IconSet.FluentUISystemRegular;
                }
            }
            set { /* Do nothing */ }
        }

        /// <summary>
        /// Gets or sets the raw, unparsed name or description of the icon.
        /// </summary>
        public string RawName { get; set; } = string.Empty;

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
        * Public Methods
        *
        ***************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="FluentUISystemIcon"/> instance from this instance's values.
        /// </summary>
        /// <returns>The cloned <see cref="FluentUISystemIcon"/>.</returns>
        public FluentUISystemIcon Clone()
        {
            var clone = new FluentUISystemIcon()
            {
                RawName      = this.RawName,
                Name         = this.Name, // Automatically parses into components
                UnicodePoint = this.UnicodePoint
            };

            return clone;
        }

        /// <summary>
        /// Converts this <see cref="FluentUISystemIcon"/> into a standard <see cref="Icon"/>.
        /// </summary>
        /// <remarks>
        /// This is sometimes needed because <see cref="FluentUISystemIcon"/> does not derive from
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
