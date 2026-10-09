using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using IconManager.Core.Icons;
using IconManager.Utilities;
using IconManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace IconManager.ViewModels
{
    /// <summary>
    /// The primary view model for the <see cref="MappingView"/>
    /// </summary>
    public partial class MappingViewModel : ViewModelBase
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingViewModel"/> class.
        /// </summary>
        public MappingViewModel()
        {
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the collection of mappings loaded in the view.
        /// </summary>
        public ObservableCollection<IconMappingViewModel> Mappings { get; } = [];

        /// <summary>
        /// Gets the collection of filtered mappings loaded in the view.
        /// This is what should be displayed to the end-user.
        /// </summary>
        public ObservableCollection<IconMappingViewModel> FilteredMappings { get; } = [];

        /// <summary>
        /// Gets or sets the text used to filter the displayed mappings.
        /// </summary>
        public string? FilterText
        {
            get => field;
            set => this.SetField(ref field, value);
        }

        /***************************************************************************************
         *
         * Commands
         *
         ***************************************************************************************/

        /// <summary>
        /// Select and load an existing mappings file into the editor.
        /// </summary>
        [RelayCommand]
        public async Task OpenMappings()
        {
            this.UpdateMappings(await OpenMappingsFile());

            return;
        }

        /// <summary>
        /// Loads a known mappings file to the editor.
        /// </summary>
        [RelayCommand]
        public void LoadMappings(string? parameter)
        {
            IconMappingList? loadedMappings = null;

            switch (parameter?.ToString()?.ToUpperInvariant())
            {
                case "SEGOEFLUENT.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.SegoeFluent);
                    break;
                case "FLUENTAVALONIA.JSON":
                    loadedMappings = IconMappingList.Load(IconSets.Paths.FluentAvaloniaMappings);
                    break;
                case "FLUENTUISYSTEMTOSEGOEMDL2ASSETS.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.FluentUISystemRegular, IconSet.SegoeMDL2Assets);
                    break;
                case "SEGOEUISYMBOLTOSEGOEMDL2ASSETS.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.SegoeUISymbol, IconSet.SegoeMDL2Assets);
                    break;
            }

            if (loadedMappings is not null)
            {
                this.UpdateMappings(loadedMappings);
            }

            return;
        }

        /// <summary>
        /// Saves all current mappings in the editor to a JSON file.
        /// </summary>
        [RelayCommand]
        public async Task SaveMappings()
        {
            if (this.Mappings.Count == 0)
            {
                return;
            }

            var options = new FilePickerSaveOptions()
            {
                SuggestedFileName = "Mappings.json",
                ShowOverwritePrompt = true,
            };
            var file = await App.Current!.TopLevel!.StorageProvider.SaveFilePickerAsync(options);

            if (file is not null)
            {
                string path = file.Path.AbsolutePath;

                if (File.Exists(path))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(path);
                }

                string? directoryName = Path.GetDirectoryName(path);
                if (directoryName is not null &&
                    Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                using (var fileStream = File.OpenWrite(path))
                {
                    var mappings = this.ViewToMappings();
                    IconMappingUtilities.Reprocess(mappings);
                    IconMappingList.Save(mappings, fileStream);
                }
            }

            return;
        }

        /// <summary>
        /// Clears all mappings in the editor.
        /// </summary>
        [RelayCommand]
        public void ClearMappings()
        {
            this.FilterText = null;
            this.Mappings.Clear();
            this.FilteredMappings.Clear();

            return;
        }

        /// <summary>
        /// Select an additional mappings file to load into the current editor mappings.
        /// This will be a union of both mappings (those in the editor and the selected file).
        /// </summary>
        [RelayCommand]
        public async Task MergeInMappings()
        {
            IconMappingList mappings = this.ViewToMappings();
            IconMappingList newMappings = await OpenMappingsFile();

            newMappings.MergeInto(mappings);
            this.UpdateMappings(mappings);

            return;
        }

        /// <summary>
        /// Scans all icon mappings in the editor and updates any deprecated icons.
        /// </summary>
        [RelayCommand]
        public void UpdateDeprecatedIcons()
        {
            IconMappingList mappings = this.ViewToMappings();

            IconMappingUtilities.UpdateDeprecatedIcons(mappings);

            this.UpdateMappings(mappings);

            return;
        }

        /// <summary>
        /// Assembles all source files and scripts necessary to build a new font from the current mappings.
        /// The output folder will automatically be opened.
        /// </summary>
        [RelayCommand]
        public void BuildFont()
        {
            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(this.ViewToMappings());

            return;
        }

        /***************************************************************************************
         *
         * Private/Protected Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(this.FilterText):
                    this.UpdateFilteredMappings();
                    break;
            }

            return;
        }

        /// <summary>
        /// Updates all mappings displayed in the editor.
        /// </summary>
        /// <param name="mappings">The list of mappings to display.</param>
        private void UpdateMappings(IconMappingList mappings)
        {
            // Update the UI collection
            this.Mappings.Clear();
            foreach (IconMapping mapping in mappings)
            {
                var viewModel = new IconMappingViewModel(mapping);

                viewModel.SourceViewModel.UpdateGlyphAsync();
                viewModel.DestinationViewModel.UpdateGlyphAsync();

                // Glyph and name need to be recalculated when selections change
                viewModel.SourceViewModel.AutoUpdate      = true;
                viewModel.DestinationViewModel.AutoUpdate = true;

                this.Mappings.Add(viewModel);
            }

            this.UpdateFilteredMappings();

            return;
        }

        /// <summary>
        /// Updates the filtered mappings by applying the current <see cref="FilterText"/>.
        /// </summary>
        private void UpdateFilteredMappings()
        {
            this.FilteredMappings.Clear();

            // Apply filter and update the UI collection simultaneously
            if (string.IsNullOrWhiteSpace(this.FilterText))
            {
                for (int i = 0; i < this.Mappings.Count; i++)
                {
                    this.FilteredMappings.Add(this.Mappings[i]);
                }
            }
            else
            {
                for (int i = 0; i < this.Mappings.Count; i++)
                {
                    if (this.Mappings[i].SourceViewModel.Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        this.Mappings[i].DestinationViewModel.Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        Icon.ToUnicodeHexString(this.Mappings[i].SourceViewModel.UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        Icon.ToUnicodeHexString(this.Mappings[i].DestinationViewModel.UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        this.Mappings[i].Comments.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.FilteredMappings.Add(this.Mappings[i]);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Opens a file picker to select a mappings file and then loads it.
        /// </summary>
        /// <returns>The selected mappings file loaded as an <see cref="IconMappingList"/>.</returns>
        private async Task<IconMappingList> OpenMappingsFile()
        {
            var options = new FilePickerOpenOptions()
            {
                AllowMultiple  = false,
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = ["*.json"],
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*"],
                    },
                },
            };
            var files = await App.Current!.TopLevel!.StorageProvider.OpenFilePickerAsync(options);
            IconMappingList mappings = new IconMappingList();

            // Load the mappings file
            if (files is not null &&
                files.Count > 0)
            {
                string path = files[0].Path.AbsolutePath;

                if (File.Exists(path))
                {
                    using (var fileStream = File.OpenRead(path))
                    {
                        try
                        {
                            mappings = IconMappingList.Load(fileStream);
                        }
                        catch
                        {
                            // Unable to open file
                        }
                    }
                }
            }

            return mappings;
        }

        /// <summary>
        /// Saves all current mappings displayed in the editor to <see cref="IconMapping"/>s.
        /// These can then be used in lower-level processing.
        /// </summary>
        /// <returns>A copy of all current <see cref="IconMapping"/>s in the editor.</returns>
        private IconMappingList ViewToMappings()
        {
            var mappings = new IconMappingList();

            foreach (IconMappingViewModel viewModel in this.Mappings)
            {
                mappings.Add(viewModel.AsIconMapping());
            }

            return mappings;
        }
    }
}
