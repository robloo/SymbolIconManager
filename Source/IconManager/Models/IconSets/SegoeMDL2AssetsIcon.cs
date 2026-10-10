using IconManager.Core.Icons;

namespace IconManager
{
    /// <summary>
    /// Represents a single icon in Segoe MDL2 Assets.
    /// </summary>
    public class SegoeMDL2AssetsIcon : Icon, IIcon
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="SegoeMDL2AssetsIcon"/> class.
        /// </summary>
        public SegoeMDL2AssetsIcon() : base()
        {
            base.IconSet = IconSet.SegoeMDL2Assets;
        }
    }
}
