using Avalonia.Media;
using IconManager.Core.Icons;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IconManager
{
    /// <summary>
    /// Represents an editable <see cref="IconMapping"/>.
    /// </summary>
    public class IconMappingViewModel : ViewModelBase
    {
        private readonly Brush PoorMappingBrush = new SolidColorBrush(new Color(0x1F, 0xEB, 0x10, 0x00));

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="IconMappingViewModel"/> class.
        /// </summary>
        public IconMappingViewModel()
        {
            this.SourceViewModel.PropertyChanged      += IconViewModel_PropertyChanged;
            this.DestinationViewModel.PropertyChanged += IconViewModel_PropertyChanged;

            this.FillMatchQuality();
            this.UpdateBackground();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconMappingViewModel"/> class.
        /// </summary>
        /// <param name="mapping">An existing icon mapping to initialize with.</param>
        public IconMappingViewModel(IconMapping mapping)
            : this()
        {
            this.IsPropertyChangedIgnoredInternally = true;

            this.SourceViewModel      = new IconViewModel(mapping.Source);
            this.DestinationViewModel = new IconViewModel(mapping.Destination);
            this.GlyphMatchQuality    = mapping.GlyphMatchQuality;
            this.MetaphorMatchQuality = mapping.MetaphorMatchQuality;
            this.IsPlaceholder        = mapping.IsPlaceholder;
            this.Comments             = mapping.Comments;

            this.IsPropertyChangedIgnoredInternally = false;

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
            get => field;
            set
            {
                field.PropertyChanged -= IconViewModel_PropertyChanged;
                this.SetField(ref field, value);
                field.PropertyChanged += IconViewModel_PropertyChanged;

                this.UpdateDependentProperties();
            }
        } = new IconViewModel();

        /// <inheritdoc cref="IconMapping.Destination"/>
        public IconViewModel DestinationViewModel
        {
            get => field;
            set
            {
                field.PropertyChanged -= IconViewModel_PropertyChanged;
                this.SetField(ref field, value);
                field.PropertyChanged += IconViewModel_PropertyChanged;

                this.UpdateDependentProperties();
            }
        } = new IconViewModel();

        /// <inheritdoc cref="IconMapping.GlyphMatchQuality"/>
        public MatchQuality GlyphMatchQuality
        {
            get => field;
            set => this.SetField(ref field, value);
        } = MatchQuality.NoMatch;

        /// <summary>
        /// Gets the list of options available to select from for <see cref="GlyphMatchQuality"/>.
        /// </summary>
        public ObservableCollection<MatchQuality> GlyphMatchQualityOptions { get; } = [];

        /// <inheritdoc cref="IconMapping.MetaphorMatchQuality"/>
        public MatchQuality MetaphorMatchQuality
        {
            get => field;
            set => this.SetField(ref field, value);
        } = MatchQuality.NoMatch;

        /// <summary>
        /// Gets the list of options available to select from for <see cref="MetaphorMatchQuality"/>.
        /// </summary>
        public ObservableCollection<MatchQuality> MetaphorMatchQualityOptions { get; } = [];

        /// <inheritdoc cref="IconMapping.IsPlaceholder"/>
        public bool IsPlaceholder
        {
            get => field;
            set => this.SetField(ref field, value);
        } = false;

        /// <inheritdoc cref="IconMapping.Comments"/>
        public string Comments
        {
            get => field;
            set => this.SetField(ref field, value);
        } = string.Empty;

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Get the background brush to display in the user-interface for the icon mapping.
        /// This is set automatically based on the validity of the mapping itself.
        /// </summary>
        public Brush Background
        {
            get => field;
            private set => this.SetField(ref field, value);
        } = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// Gets a value indicating whether the source icon name can be edited by the user.
        /// This is only allowed for undefined icon sets.
        /// </summary>
        public bool IsSourceNameEditable
        {
            get => field;
            private set => this.SetField(ref field, value);
        }

        /// <summary>
        /// Gets a value indicating whether the destination icon name can be edited by the user.
        /// This is only allowed for undefined icon sets.
        /// </summary>
        public bool IsDestinationNameEditable
        {
            get => field;
            private set => this.SetField(ref field, value);
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
            base.OnPropertyChanged(propertyName);

            if (this.IsPropertyChangedIgnoredInternally)
            {
                return;
            }

            if (propertyName != nameof(this.Background))
            {
                this.UpdateBackground();
            }

            return;
        }

        /// <summary>
        /// Initializes the collection of all available match quality options.
        /// </summary>
        private void FillMatchQuality()
        {
            this.GlyphMatchQualityOptions.Clear();
            this.MetaphorMatchQualityOptions.Clear();

            var enumValues = Enum.GetValues<MatchQuality>();
            foreach (MatchQuality value in enumValues)
            {
                this.GlyphMatchQualityOptions.Add(value);
                this.MetaphorMatchQualityOptions.Add(value);
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
            this.IsSourceNameEditable      = this.SourceViewModel.IconSet == IconSet.Undefined;
            this.IsDestinationNameEditable = this.DestinationViewModel.IconSet == IconSet.Undefined;
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
