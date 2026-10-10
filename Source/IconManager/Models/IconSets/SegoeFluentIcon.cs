using IconManager.Core.Icons;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in Segoe Fluent Icons.
    /// </summary>
    public class SegoeFluentIcon : Icon, IIcon
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="SegoeFluentIcon"/> class.
        /// </summary>
        public SegoeFluentIcon() : base()
        {
            base.IconSet = IconSet.SegoeFluent;
        }
    }
}
