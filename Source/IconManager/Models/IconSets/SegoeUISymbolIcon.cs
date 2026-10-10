using IconManager.Core.Icons;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in Segoe UI Symbols.
    /// </summary>
    public class SegoeUISymbolIcon : Icon, IIcon
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="SegoeUISymbolIcon"/> class.
        /// </summary>
        public SegoeUISymbolIcon() : base()
        {
            base.IconSet = IconSet.SegoeUISymbol;
        }
    }
}
