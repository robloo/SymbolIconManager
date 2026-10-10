using IconManager.Core.Icons;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in WinJS Symbols.
    /// </summary>
    public class WinJSSymbolsIcon : Icon, IIcon
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="WinJSSymbolsIcon"/> class.
        /// </summary>
        public WinJSSymbolsIcon() : base()
        {
            base.IconSet = IconSet.WinJSSymbols;
        }
    }
}
