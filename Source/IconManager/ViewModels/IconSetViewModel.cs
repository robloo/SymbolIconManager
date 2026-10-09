using IconManager.Core.Icons;

namespace IconManager.ViewModels
{
    /// <summary>
    /// Represents an <see cref="IconSet"/>.
    /// </summary>
    public class IconSetViewModel : ViewModelBase
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="IconSetViewModel"/> class.
        /// </summary>
        public IconSetViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconSetViewModel"/> class.
        /// </summary>
        /// <param name="iconSet">An <see cref="IconSet"/> to initialize with.</param>
        public IconSetViewModel(IconSet iconSet)
        {
            this.IconSet = iconSet;
            this.Name    = iconSet.ToString();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets the specific icon set.
        /// </summary>
        public IconSet IconSet
        {
            get => field;
            set => this.SetField(ref field, value);
        } = IconSet.Undefined;

        /// <summary>
        /// Gets or sets the displayed name or description of the icon set.
        /// </summary>
        public string Name
        {
            get => field;
            set => this.SetField(ref field, value);
        } = string.Empty;

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(this.Name)
                ? IconSet.ToString()
                : this.Name;
        }
    }
}
