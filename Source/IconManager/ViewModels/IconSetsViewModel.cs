using IconManager.Core.Icons;
using IconManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IconManager.ViewModels
{
    /// <summary>
    /// The primary view model for the <see cref="IconSetsView"/>
    /// </summary>
    public partial class IconSetsViewModel : ViewModelBase
    {
        private Dictionary<IconSet, List<IconViewModel>> _cachedIconSets = [];

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="IconSetsViewModel"/> class.
        /// </summary>
        public IconSetsViewModel()
        {
            this.FillIconSets();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the collection of filtered <see cref="Icons"/>.
        /// This is what should be displayed to the end-user.
        /// </summary>
        public ObservableCollection<IconViewModel> FilteredIcons { get; } = [];

        /// <summary>
        /// Gets or sets the text used to filter the displayed icons.
        /// </summary>
        public string? FilterText
        {
            get => field;
            set
            {
                this.SetField(ref field, value);
                this.UpdateFilteredIcons();
            }
        }

        /// <summary>
        /// Gets the collection of all loaded icons in the selected icon set.
        /// </summary>
        public ObservableCollection<IconViewModel> Icons { get; } = [];

        /// <summary>
        /// Gets the collection of all icon sets that can be selected from.
        /// </summary>
        public ObservableCollection<IconSetViewModel> IconSets { get; } = [];

        /// <summary>
        /// Gets or sets the selected icon set.
        /// </summary>
        public IconSetViewModel? SelectedIconSet
        {
            get => field;
            set => this.SetField(ref field, value);
        } = null;

        /***************************************************************************************
         *
         * Private/Protected Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(this.SelectedIconSet))
            {
                this.UpdateIcons();
            }
        }

        /// <summary>
        /// Updates all icons and filtered icons for the currently selected icon set.
        /// </summary>
        private void UpdateIcons()
        {
            var selectedIconSet = this.SelectedIconSet?.IconSet;
            var iconViewModels = new List<IconViewModel>();

            // Get the full list of icons in the set
            if (selectedIconSet is not null)
            {
                if (this._cachedIconSets.ContainsKey(selectedIconSet.Value))
                {
                    iconViewModels = this._cachedIconSets[selectedIconSet.Value];
                }
                else
                {
                    var icons = IconSetBase.GetIcons(selectedIconSet.Value);
                    for (int i = 0; i < icons.Count; i++)
                    {
                        var viewModel = new IconViewModel(icons[i]);
                        viewModel.UpdateGlyphAsync();

                        iconViewModels.Add(viewModel);
                    }

                    this._cachedIconSets.Add(selectedIconSet.Value, iconViewModels);
                }
            }

            // Update the UI collection
            this.Icons.Clear();
            for (int i = 0; i < iconViewModels.Count; i++)
            {
                this.Icons.Add(iconViewModels[i]);
            }

            this.UpdateFilteredIcons();

            return;
        }

        /// <summary>
        /// Updates the filtered icons applying the current <see cref="FilterText"/>.
        /// </summary>
        private void UpdateFilteredIcons()
        {
            this.FilteredIcons.Clear();

            // Apply filter and update the UI collection simultaneously
            if (string.IsNullOrWhiteSpace(this.FilterText))
            {
                for (int i = 0; i < this.Icons.Count; i++)
                {
                    this.FilteredIcons.Add(this.Icons[i]);
                }
            }
            else
            {
                for (int i = 0; i < this.Icons.Count; i++)
                {
                    if (this.Icons[i].Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        Icon.ToUnicodeHexString(this.Icons[i].UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.FilteredIcons.Add(this.Icons[i]);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Fills all available icon sets.
        /// </summary>
        private void FillIconSets()
        {
            this.IconSets.Clear();

            var iconSetEnumValues = Enum.GetValues<IconSet>();
            foreach (IconSet value in iconSetEnumValues)
            {
                if (value == IconSet.Undefined)
                {
                    continue;
                }

                this.IconSets.Add(new IconSetViewModel(value));
            }

            return;
        }
    }
}
