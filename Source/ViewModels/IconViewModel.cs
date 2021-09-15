using Avalonia.Media.Imaging;

namespace IconManager
{
    public class IconViewModel : ViewModelBase, IIcon
    {
        private Bitmap? _Glyph;
        private IconSet _IconSet;
        private string  _Name;
        private uint    _UnicodePoint;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconViewModel()
        {
            this._Glyph        = null;
            this._IconSet      = IconSet.Undefined;
            this._Name         = string.Empty;
            this._UnicodePoint = 0;
        }

        public IconViewModel(IIcon icon)
        {
            this._Glyph        = null;
            this._IconSet      = icon.IconSet;
            this._Name         = icon.Name;
            this._UnicodePoint = icon.UnicodePoint;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the displayed glyph (graphic/symbol) of the icon.
        /// </summary>
        public Bitmap? Glyph
        {
            get => this._Glyph;
            set => this.SetField(ref this._Glyph, value);
        }

        /// <inheritdoc/>
        public IconSet IconSet
        {
            get => this._IconSet;
            set => this.SetField(ref this._IconSet, value);
        }

        /// <inheritdoc/>
        public string Name
        {
            get => this._Name;
            set => this.SetField(ref this._Name, value);
        }

        /// <inheritdoc/>
        public uint UnicodePoint
        {
            get => this._UnicodePoint;
            set => this.SetField(ref this._UnicodePoint, value);
        }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public string UnicodeString
        {
            get => Icon.ToUnicodeString(this.UnicodePoint);
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Converts this <see cref="IconViewModel"/> into a standard <see cref="Icon"/>.
        /// </summary>
        /// <returns>A new <see cref="Icon"/> from the view model properties.</returns>
        public Icon AsIcon()
        {
            return new Icon()
            {
                IconSet      = this.IconSet,
                Name         = this.Name,
                UnicodePoint = this.UnicodePoint
            };
        }

        /// <summary>
        /// Generates and adds the preview glyph image.
        /// </summary>
        public async void AddGlyphAsync()
        {
            this.Glyph = await GlyphRenderer.GetBitmapAsync(this.IconSet, this.UnicodePoint);
            return;
        }
    }
}
