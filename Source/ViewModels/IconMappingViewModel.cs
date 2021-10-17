using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IconManager
{
    public class IconMappingViewModel : ViewModelBase
    {
        private readonly Brush PoorMappingBrush = new SolidColorBrush(new Color(0xFF, 0xF2, 0xD7, 0xD5));

        private IconViewModel _SourceViewModel;
        private IconViewModel _DestinationViewModel;
        private MatchQuality  _GlyphMatchQuality;
        private MatchQuality  _MetaphorMatchQuality;
        private bool          _IsPlaceholder;
        private string        _Comments;

        private Brush _Background;
        private bool  _IsSourceNameEditable;
        private bool  _IsDestinationNameEditable;

        private ObservableCollection<MatchQuality> _GlyphMatchQualityOptions    = new ObservableCollection<MatchQuality>();
        private ObservableCollection<MatchQuality> _MetaphorMatchQualityOptions = new ObservableCollection<MatchQuality>();

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconMappingViewModel()
        {
            this._Background           = new SolidColorBrush(Colors.Transparent);
            this._SourceViewModel      = new IconViewModel();
            this._DestinationViewModel = new IconViewModel();
            this._GlyphMatchQuality    = MatchQuality.NoMatch;
            this._MetaphorMatchQuality = MatchQuality.NoMatch;
            this._IsPlaceholder        = false;
            this._Comments             = string.Empty;

            this._SourceViewModel.PropertyChanged      += IconViewModel_PropertyChanged;
            this._DestinationViewModel.PropertyChanged += IconViewModel_PropertyChanged;

            this.InitOptions();
            this.UpdateBackground();
        }

        public IconMappingViewModel(IconMapping mapping)
        {
            this._Background           = new SolidColorBrush(Colors.Transparent);
            this._SourceViewModel      = new IconViewModel(mapping.Source);
            this._DestinationViewModel = new IconViewModel(mapping.Destination);
            this._GlyphMatchQuality    = mapping.GlyphMatchQuality;
            this._MetaphorMatchQuality = mapping.MetaphorMatchQuality;
            this._IsPlaceholder        = mapping.IsPlaceholder;
            this._Comments             = mapping.Comments;

            this._SourceViewModel.PropertyChanged      += IconViewModel_PropertyChanged;
            this._DestinationViewModel.PropertyChanged += IconViewModel_PropertyChanged;

            this.InitOptions();
            this.UpdateBackground();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <inheritdoc cref="IconMapping.Source"/>
        public IconViewModel SourceViewModel
        {
            get => this._SourceViewModel;
            set
            {
                this._SourceViewModel.PropertyChanged -= IconViewModel_PropertyChanged;
                this.SetField(ref this._SourceViewModel, value);
                this._SourceViewModel.PropertyChanged += IconViewModel_PropertyChanged;

                this.UpdateDependentProperties();
            }
        }

        /// <inheritdoc cref="IconMapping.Destination"/>
        public IconViewModel DestinationViewModel
        {
            get => this._DestinationViewModel;
            set
            {
                this._DestinationViewModel.PropertyChanged -= IconViewModel_PropertyChanged;
                this.SetField(ref this._DestinationViewModel, value);
                this._DestinationViewModel.PropertyChanged += IconViewModel_PropertyChanged;

                this.UpdateDependentProperties();
            }
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

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Get the background brush to display in the user-interface for the icon mapping.
        /// This is set automatically based on the validity of the mapping itself.
        /// </summary>
        public Brush Background
        {
            get => this._Background;
            private set => this.SetField(ref this._Background, value);
        }

        /// <summary>
        /// Gets the list of options available to select from for <see cref="GlyphMatchQuality"/>.
        /// </summary>
        public ObservableCollection<MatchQuality> GlyphMatchQualityOptions
        {
            get => this._GlyphMatchQualityOptions;
        }

        /// <summary>
        /// Gets the list of options available to select from for <see cref="MetaphorMatchQuality"/>.
        /// </summary>
        public ObservableCollection<MatchQuality> MetaphorMatchQualityOptions
        {
            get => this._MetaphorMatchQualityOptions;
        }

        /// <summary>
        /// Gets a value indicating whether the source icon name can be edited by the user.
        /// This is only allowed for undefined icon sets.
        /// </summary>
        public bool IsSourceNameEditable
        {
            get => this._IsSourceNameEditable;
            private set => this.SetField(ref this._IsSourceNameEditable, value);
        }

        /// <summary>
        /// Gets a value indicating whether the destination icon name can be edited by the user.
        /// This is only allowed for undefined icon sets.
        /// </summary>
        public bool IsDestinationNameEditable
        {
            get => this._IsDestinationNameEditable;
            private set => this.SetField(ref this._IsDestinationNameEditable, value);
        }

        /***************************************************************************************
         *
         * Public Methods
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

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName != nameof(this.Background))
            {
                this.UpdateBackground();
            }

            base.OnPropertyChanged(propertyName);
            return;
        }

        /// <summary>
        /// Initializes the list of options to select from.
        /// </summary>
        private void InitOptions()
        {
            var enumValues = (MatchQuality[])Enum.GetValues(typeof(MatchQuality));

            foreach (var value in enumValues)
            {
                this._GlyphMatchQualityOptions.Add(value);
                this._MetaphorMatchQualityOptions.Add(value);
            }

            return;
        }

        /// <summary>
        /// Recalculates and updates the background brush.
        /// </summary>
        private void UpdateBackground()
        {
            // Converting to an IconMapping and then throwing it away isn't the best performance
            // However, that is better than code duplication
            if (this.AsIconMapping().IsValid == false ||
                this.IsPlaceholder)
            {
                this.Background = this.PoorMappingBrush;
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.Transparent);
            }

            return;
        }

        /// <summary>
        /// Updates the values of properties that are calculated depending on other property values.
        /// </summary>
        private void UpdateDependentProperties()
        {
            this.IsSourceNameEditable      = this._SourceViewModel.IconSet == IconSet.Undefined;
            this.IsDestinationNameEditable = this._DestinationViewModel.IconSet == IconSet.Undefined;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        /// <summary>
        /// Event handler for when a child IconViewModel property changes.
        /// </summary>
        private void IconViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.UpdateBackground();
            this.UpdateDependentProperties();

            return;
        }
    }
}
