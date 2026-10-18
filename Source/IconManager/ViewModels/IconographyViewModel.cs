using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using IconManager.Core.Icons;
using IconManager.IconSets;
using IconManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IconManager.ViewModels
{
    /// <summary>
    /// The primary view model for the <see cref="IconographyView"/>
    /// </summary>
    public partial class IconographyViewModel : ViewModelBase
    {
        private Dictionary<IconSet, List<IconViewModel>> _cachedIconSets = [];

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="IconographyViewModel"/> class.
        /// </summary>
        public IconographyViewModel()
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
            set => this.SetProperty(ref field, value);
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
        /// Gets or sets the selected icon.
        /// </summary>
        public IconViewModel? SelectedIcon
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = null;

        /// <summary>
        /// Gets or sets the selected icon set.
        /// </summary>
        public IconSetViewModel? SelectedIconSet
        {
            get => field;
            set => this.SetProperty(ref field, value);
        } = null;

        /***************************************************************************************
         *
         * Commands
         *
         ***************************************************************************************/

        /// <summary>
        /// Clears any active filter text and rebuilds the displayed icons.
        /// </summary>
        [RelayCommand]
        public void ClearFilterText()
        {
            this.FilterText = null;

            return;
        }

        /// <summary>
        /// Clears any selected icon view model which will also close any open details side pane.
        /// </summary>
        [RelayCommand]
        public void ClearSelectedIcon()
        {
            this.SelectedIcon = null;

            return;
        }

        /// <summary>
        /// Copies the given text string to the clipboard.
        /// </summary>
        /// <param name="text">The text string to copy to the clipboard.</param>
        /// <returns>An awaitable <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task CopyTextAsync(string? text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var lifetime = Application.Current?.ApplicationLifetime;
                IClipboard? clipboard = null;

                if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    clipboard = TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
                }

                if (lifetime is ISingleViewApplicationLifetime singleView)
                {
                    clipboard = TopLevel.GetTopLevel(singleView.MainView)?.Clipboard;
                }

                if (clipboard is not null)
                {
                    try
                    {
                        await clipboard.SetTextAsync(text);
                    }
                    catch { }
                }
            }
        }

        /***************************************************************************************
         *
         * Private/Protected Methods
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

            switch (e.PropertyName)
            {
                case nameof(this.FilterText):
                    this.UpdateFilteredIcons();
                    break;
                case nameof(this.SelectedIconSet):
                    this.UpdateIcons();
                    break;
            }

            return;
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
        /// Updates the filtered icons by applying the current <see cref="FilterText"/>.
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
        /// Initializes the collection of all available icon sets.
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
