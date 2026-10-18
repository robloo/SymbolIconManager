using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using IconManager.IconSets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IconManager
{
    /// <summary>
    /// Represents an editable <see cref="Icon"/>.
    /// </summary>
    public class IconViewModel : ViewModelBase, IIcon
    {
        // Services
        private readonly IGlyphRenderer<GlyphBitmap> _glyphRendererService;
        private readonly IGlyphSourceProvider        _glyphSourceProviderService;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="IconViewModel"/> class.
        /// </summary>
        public IconViewModel()
        {
            this._glyphRendererService       = App.Current.Services.GetRequiredService<IGlyphRenderer<GlyphBitmap>>();
            this._glyphSourceProviderService = App.Current.Services.GetRequiredService<IGlyphSourceProvider>();

            this.FillIconSets();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconViewModel"/> class.
        /// </summary>
        /// <param name="icon">An existing icon to initialize with.</param>
        public IconViewModel(IReadOnlyIcon icon)
            : this()
        {
            this.IsPropertyChangedIgnoredInternally = true;

            this.IconSet      = icon.IconSet;
            this.Name         = icon.Name;
            this.UnicodePoint = icon.UnicodePoint;

            this.IsPropertyChangedIgnoredInternally = false;
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
            get => field;
            set => this.SetProperty(ref field, value);
        } = false;

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the displayed glyph (graphic/symbol) of the icon.
        /// </summary>
        public GlyphBitmap? Glyph
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = null;

        /// <inheritdoc/>
        public IconSet IconSet
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = IconSet.Undefined;

        /// <summary>
        /// Gets the list of options available to select from for <see cref="IconSet"/>.
        /// </summary>
        public ObservableCollection<IconSet> IconSets { get; } = [];

        /// <inheritdoc/>
        public string Name
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = string.Empty;

        /// <inheritdoc/>
        public uint UnicodePoint
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = 0;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.AsIcon().ToString();
        }

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
            this.Glyph = await this._glyphRendererService.RenderGlyphAsync(
                this.IconSet,
                this.UnicodePoint,
                this._glyphSourceProviderService);

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
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (this.IsPropertyChangedIgnoredInternally)
            {
                return;
            }

            if (e.PropertyName == nameof(this.IconSet) ||
                e.PropertyName == nameof(this.UnicodePoint))
            {
                if (this.AutoUpdate)
                {
                    this.UpdateName();
                    this.UpdateGlyphAsync();
                }
            }

            return;
        }

        /// <summary>
        /// Initializes the collection of all available icon sets.
        /// </summary>
        private void FillIconSets()
        {
            this.IconSets.Clear();

            var enumValues = Enum.GetValues<IconSet>();
            foreach (IconSet value in enumValues)
            {
                this.IconSets.Add(value);
            }

            return;
        }
    }
}
