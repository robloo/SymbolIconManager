namespace IconManager
{
    public class IconMappingViewModel : ViewModelBase
    {
        private IconViewModel _SourceViewModel;
        private IconViewModel _DestinationViewModel;
        private MatchQuality  _GlyphMatchQuality;
        private MatchQuality  _MetaphorMatchQuality;
        private bool          _IsPlaceholder;
        private string        _Comments;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconMappingViewModel()
        {
            this._SourceViewModel      = new IconViewModel();
            this._DestinationViewModel = new IconViewModel();
            this._GlyphMatchQuality    = MatchQuality.NoMatch;
            this._MetaphorMatchQuality = MatchQuality.NoMatch;
            this._IsPlaceholder        = false;
            this._Comments             = string.Empty;
        }

        public IconMappingViewModel(IconMapping mapping)
        {
            this._SourceViewModel      = new IconViewModel(mapping.Source);
            this._DestinationViewModel = new IconViewModel(mapping.Destination);
            this._GlyphMatchQuality    = mapping.GlyphMatchQuality;
            this._MetaphorMatchQuality = mapping.MetaphorMatchQuality;
            this._IsPlaceholder        = mapping.IsPlaceholder;
            this._Comments             = mapping.Comments;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <inheritdoc cref="IconMapping.Source"/>
        public IconViewModel SourceViewModel
        {
            get => this._SourceViewModel;
            set => this.SetField(ref this._SourceViewModel, value);
        }

        /// <inheritdoc cref="IconMapping.Destination"/>
        public IconViewModel DestinationViewModel
        {
            get => this._DestinationViewModel;
            set => this.SetField(ref this._DestinationViewModel, value);
        }

        /// <inheritdoc cref="IconMapping.GlyphMatchQuality"/>
        public MatchQuality GlyphMatchQuality
        {
            get => this._GlyphMatchQuality;
            set => this.SetField(ref this._GlyphMatchQuality, value);
        }

        /// <inheritdoc cref="IconMapping.MetaphorMatchQuality"/>
        public MatchQuality MetaphorMatchQuality
        {
            get => this._MetaphorMatchQuality;
            set => this.SetField(ref this._MetaphorMatchQuality, value);
        }

        /// <inheritdoc cref="IconMapping.IsPlaceholder"/>
        public bool IsPlaceholder
        {
            get => this._IsPlaceholder;
            set => this.SetField(ref this._IsPlaceholder, value);
        }

        /// <inheritdoc cref="IconMapping.Comments"/>
        public string Comments
        {
            get => this._Comments;
            set => this.SetField(ref this._Comments, value);
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Converts this <see cref="IconMappingViewModel"/> into a standard <see cref="IconMapping"/>.
        /// </summary>
        /// <returns>A new <see cref="IconMapping"/> from the view models properties.</returns>
        public IconMapping AsIconMapping()
        {
            var mapping = new IconMapping()
            {
                Source               = this.SourceViewModel.AsIcon(),
                Destination          = this.DestinationViewModel.AsIcon(),
                GlyphMatchQuality    = this.GlyphMatchQuality,
                MetaphorMatchQuality = this.MetaphorMatchQuality,
                IsPlaceholder        = this.IsPlaceholder,
                Comments             = this.Comments
            };

            return mapping;
        }
    }
}
