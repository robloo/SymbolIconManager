using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IconManager
{
    public partial class IconSetsView : UserControl
    {
        private Dictionary<IconSet, List<IconViewModel>> cachedIconSets = new Dictionary<IconSet, List<IconViewModel>>();

        private string? _FilterText = string.Empty;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconSetsView()
        {
            InitializeComponent();

            // Set the list of icon sets
            var iconSetEnumValues = Enum.GetValues(typeof(IconSet));
            List<ComboBoxItem> iconSets = new List<ComboBoxItem>();

            foreach (var value in iconSetEnumValues)
            {
                iconSets.Add(new ComboBoxItem()
                {
                    Content = value.ToString(),
                    Tag     = value
                });
            }
            this.SelectedIconSetComboBox.Items = iconSets;
            this.SelectedIconSetComboBox.SelectedIndex = 0; // Should be Undefined

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the collection of icons loaded in the view.
        /// </summary>
        public ObservableCollection<IconViewModel> Icons { get; } = new ObservableCollection<IconViewModel>();

        /// <summary>
        /// Gets the collection of filtered icons loaded in the view.
        /// This is what should be displayed to the end-user.
        /// </summary>
        public ObservableCollection<IconViewModel> FilteredIcons { get; } = new ObservableCollection<IconViewModel>();

        /// <summary>
        /// Gets or sets the text used to filter the displayed icons.
        /// </summary>
        public string? FilterText
        {
            get => this._FilterText;
            set
            {
                this._FilterText = value;
                this.UpdateFilteredIcons();
            }
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private void UpdateIcons()
        {
            ComboBoxItem? selectedItem = this.SelectedIconSetComboBox.SelectedItem as ComboBoxItem;
            List<IconViewModel> iconViewModels = new List<IconViewModel>();

            var selectedIconSet = (IconSet?)Enum.Parse(typeof(IconSet), selectedItem?.Tag?.ToString() ?? string.Empty);

            // Get the full list of icons in the set
            if (selectedIconSet != null)
            {
                if (this.cachedIconSets.ContainsKey(selectedIconSet.Value))
                {
                    iconViewModels = this.cachedIconSets[selectedIconSet.Value];
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

                    this.cachedIconSets.Add(selectedIconSet.Value, iconViewModels);
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
                        Icon.ToUnicodeString(this.Icons[i].UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.FilteredIcons.Add(this.Icons[i]);
                    }
                }
            }

            return;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        /// <summary>
        /// Event handler for when the primary selected IconSet changes.
        /// </summary>
        private void SelectedIconSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateIcons();
            return;
        }
    }
}
