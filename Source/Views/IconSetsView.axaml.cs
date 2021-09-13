using Avalonia;
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

        /***************************************************************************************
         *
         * Constructor
         *
         ***************************************************************************************/

        public IconSetsView()
        {
            InitializeComponent();

            // Init the list of icon sets
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

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the list of icons to display in the user-interface.
        /// </summary>
        public ObservableCollection<IconViewModel> IconSetIcons { get; } = new ObservableCollection<IconViewModel>();

        /// <summary>
        /// Gets or sets the text used to filter the display icons.
        /// </summary>
        public string? FilterText { get; set; } = string.Empty;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private void UpdateIcons()
        {
            ComboBoxItem? selectedItem = this.SelectedIconSetComboBox.SelectedItem as ComboBoxItem;
            IReadOnlyList<IIcon>? icons = null;
            List<IconViewModel> iconViewModels = new List<IconViewModel>();
            List<IconViewModel> filteredIconViewModels = new List<IconViewModel>();

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
                    switch (selectedIconSet)
                    {
                        case IconSet.FluentUISystem:
                            icons = FluentUISystem.Icons;
                            break;
                        case IconSet.SegoeFluent:
                            icons = SegoeFluent.Icons;
                            break;
                        case IconSet.SegoeMDL2Assets:
                            icons = SegoeMDL2Assets.Icons;
                            break;
                        case IconSet.SegoeUISymbol:
                            // Not currently supported
                            break;
                        case IconSet.WinJSSymbols:
                            icons = WinJSSymbols.Icons;
                            break;
                    }

                    if (icons != null)
                    {
                        for (int i = 0; i < icons.Count; i++)
                        {
                            var viewModel = new IconViewModel(icons[i], selectedIconSet.Value);
                            viewModel.DownloadGlyphImage();

                            iconViewModels.Add(viewModel);
                        }
                    }

                    this.cachedIconSets.Add(selectedIconSet.Value, iconViewModels);
                }
            }

            // Apply filter
            if (string.IsNullOrWhiteSpace(this.FilterText))
            {
                filteredIconViewModels = iconViewModels;
            }
            else
            {
                for (int i = 0; i < iconViewModels.Count; i++)
                {
                    if (iconViewModels[i].Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        iconViewModels[i].UnicodePoint.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        filteredIconViewModels.Add(iconViewModels[i]);
                    }
                }
            }

            // Update the UI
            this.IconSetIcons.Clear();
            for (int i = 0; i < filteredIconViewModels.Count; i++)
            {
                this.IconSetIcons.Add(filteredIconViewModels[i]);
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

        /// <summary>
        /// Event handler for when the filter text button is clicked.
        /// </summary>
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateIcons();
            return;
        }
    }
}
