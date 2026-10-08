using Avalonia.Media.Imaging;
using System;
using System.Collections.ObjectModel;

namespace IconManager
{
    public class IconViewModel : ViewModelBase, IIcon
    {
        private bool    _AutoUpdate;
        private Bitmap? _Glyph;
        private IconSet _IconSet;
        private string  _Name;
        private uint    _UnicodePoint;

        private ObservableCollection<IconSet> _IconSetOptions = new ObservableCollection<IconSet>();

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconViewModel()
        {
            this._AutoUpdate   = false;
            this._Glyph        = null;
            this._IconSet      = IconSet.Undefined;
            this._Name         = string.Empty;
            this._UnicodePoint = 0;

            this.InitOptions();
        }

        public IconViewModel(IReadOnlyIcon icon)
        {
            this._AutoUpdate   = false;
            this._Glyph        = null;
            this._IconSet      = icon.IconSet;
            this._Name         = icon.Name;
            this._UnicodePoint = icon.UnicodePoint;

            this.InitOptions();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets a value indicating whether the glyph and name properties will be
        /// automatically updated based on changes to other properties.
        /// </summary>
        /// <remarks>
        /// This value is set to false by default for performance reasons.
        /// In addition, it should always be set to false when changing multiple properties at once.
        /// This avoids unnecessary (and expensive) recalculation of properties such as Glyph.
        /// 
        /// Setting this property to true will not immediately update the glyph and name.
        /// No updates occur until other properties change.
        /// </remarks>
        public bool AutoUpdate
        {
            get => this._AutoUpdate;
            set => this.SetField(ref this._AutoUpdate, value);
        }

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

        /// <summary>
        /// Gets the list of options available to select from for <see cref="IconSet"/>.
        /// </summary>
        public ObservableCollection<IconSet> IconSetOptions
        {
            get => this._IconSetOptions;
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
        /// Generates and sets a new preview glyph image based on <see cref="IconSet"/> and
        /// <see cref="UnicodePoint"/>.
        /// </summary>
        public async void UpdateGlyphAsync()
        {
            this.Glyph = await GlyphRenderer.GetPreviewBitmapAsync(this.IconSet, this.UnicodePoint);
            return;
        }

        /// <summary>
        /// Retrieves and sets the icon name based on <see cref="IconSet"/> and
        /// <see cref="UnicodePoint"/>.
        /// </summary>
        public void UpdateName()
        {
            this.Name = IconSetBase.FindName(this.IconSet, this.UnicodePoint);
            return;
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName)
        {
            if ((propertyName == nameof(this.IconSet) ||
                 propertyName == nameof(this.UnicodePoint)) &&
                (propertyName != nameof(this.Glyph) &&
                 propertyName != nameof(this.Name)))
            {
                if (this.AutoUpdate)
                {
                    this.UpdateName();
                    this.UpdateGlyphAsync();
                }
            }

            base.OnPropertyChanged(propertyName);
            return;
        }

        /// <summary>
        /// Initializes the list of options to select from.
        /// </summary>
        private void InitOptions()
        {
            var enumValues = (IconSet[])Enum.GetValues(typeof(IconSet));

            foreach (var value in enumValues)
            {
                this._IconSetOptions.Add(value);
            }

            return;
        }
    }
}
