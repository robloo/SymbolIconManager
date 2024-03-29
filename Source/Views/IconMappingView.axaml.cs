using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IconManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IconManager
{
    public partial class IconMappingView : UserControl
    {
        private string? _FilterText = string.Empty;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconMappingView()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the collection of mappings loaded in the view.
        /// </summary>
        public ObservableCollection<IconMappingViewModel> Mappings { get; } = new ObservableCollection<IconMappingViewModel>();

        /// <summary>
        /// Gets the collection of filtered mappings loaded in the view.
        /// This is what should be displayed to the end-user.
        /// </summary>
        public ObservableCollection<IconMappingViewModel> FilteredMappings { get; } = new ObservableCollection<IconMappingViewModel>();

        /// <summary>
        /// Gets or sets the text used to filter the displayed mappings.
        /// </summary>
        public string? FilterText
        {
            get => this._FilterText;
            set
            {
                this._FilterText = value;
                this.UpdateFilteredMappings();
            }
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

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

        private async Task<IconMappingList> OpenMappingsFile()
        {
            var options = new FilePickerOpenOptions()
            {
                AllowMultiple  = false,
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = new string[] { "*.json" },
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = new string[] { "*" },
                    },
                },
            };
            var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(options);
            IconMappingList mappings = new IconMappingList();

            // Load the mappings file
            if (files != null &&
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

        private IconMappingList ViewToMappings()
        {
            var mappings = new IconMappingList();

            foreach (IconMappingViewModel viewModel in this.Mappings)
            {
                mappings.Add(viewModel.AsIconMapping());
            }

            return mappings;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        /// <summary>
        /// Event handler for when the load button is clicked.
        /// </summary>
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateMappings(await OpenMappingsFile());

            return;
        }

        /// <summary>
        /// Event handler for when a known mapping file is clicked in the load button flyout.
        /// </summary>
        private void LoadMappingItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            IconMappingList? loadedMappings = null;

            switch (menuItem?.Header?.ToString()?.ToUpperInvariant())
            {
                case "SEGOEFLUENT.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.SegoeFluent);
                    break;
                case "FLUENTAVALONIA.JSON":
                    loadedMappings = IconMappingList.Load("avares://IconManager/Data/Mappings/FluentAvalonia.json");
                    break;
                case "FLUENTUISYSTEMTOSEGOEMDL2ASSETS.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.FluentUISystemRegular, IconSet.SegoeMDL2Assets);
                    break;
                case "SEGOEUISYMBOLTOSEGOEMDL2ASSETS.JSON":
                    loadedMappings = IconMappingList.Load(IconSet.SegoeUISymbol, IconSet.SegoeMDL2Assets);
                    break;
            }

            if (loadedMappings != null)
            {
                this.UpdateMappings(loadedMappings);
            }

            return;
        }

        /// <summary>
        /// Event handler for when the save button is clicked.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var options = new FilePickerSaveOptions()
            {
                SuggestedFileName = "Mappings.json",
                ShowOverwritePrompt = true,
            };
            var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                string path = file.Path.AbsolutePath;

                if (File.Exists(path))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(path);
                }

                string? directoryName = Path.GetDirectoryName(path);
                if (directoryName != null &&
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
        /// Event handler for when the clear button is clicked.
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterText = string.Empty;
            this.Mappings.Clear();
            this.FilteredMappings.Clear();

            return;
        }

        /// <summary>
        /// Event handler for when a mapping list action is clicked.
        /// </summary>
        private async void ActionItem_Click(object sender, RoutedEventArgs e)
        {
            if (object.ReferenceEquals(sender, this.MergeInMenuItem))
            {
                IconMappingList mappings = this.ViewToMappings();
                IconMappingList newMappings = await OpenMappingsFile();

                newMappings.MergeInto(mappings);
                this.UpdateMappings(mappings);
            }
            else if (object.ReferenceEquals(sender, this.UpdateDeprecatedIconsMenuItem))
            {
                IconMappingList mappings = this.ViewToMappings();

                IconMappingUtilities.UpdateDeprecatedIcons(mappings);

                this.UpdateMappings(mappings);
            }
            else if (object.ReferenceEquals(sender, this.BuildFontMenuItem))
            {
                var fontBuilder = new FontBuilder();
                fontBuilder.BuildFont(this.ViewToMappings());
            }

            return;
        }

        /// <summary>
        /// Event handler for when the help with match quality definitions button is clicked.
        /// </summary>
        private void MatchQualityHelpButton_Click(object sender, RoutedEventArgs e)
        {
            string url = @"https://github.com/robloo/SymbolIconManager/blob/main/Docs/IconMapping.md#match-quality-values";

            // See: https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/ and
            // https://github.com/dotnet/runtime/issues/17938
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments       = "/c start " + url,
                        UseShellExecute = true,
                        FileName        = "cmd"
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch { }

            return;
        }
    }
}
