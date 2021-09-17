using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

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
                        Icon.ToUnicodeString(this.Mappings[i].SourceViewModel.UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        Icon.ToUnicodeString(this.Mappings[i].DestinationViewModel.UnicodePoint).Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
                        this.Mappings[i].Comments.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.FilteredMappings.Add(this.Mappings[i]);
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
        /// Event handler for when the load button is clicked.
        /// </summary>
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var paths = await dialog.ShowAsync(App.MainWindow);
            IconMappingList mappings = new IconMappingList();

            // Load the mappings file
            if (paths != null &&
                paths.Length > 0 &&
                File.Exists(paths[0]))
            {
                using (var fileStream = File.OpenRead(paths[0]))
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

            this.UpdateMappings(mappings);

            return;
        }

        /// <summary>
        /// Event handler for when the save button is clicked.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            var path = await dialog.ShowAsync(App.MainWindow);

            if (path != null)
            {
                if (File.Exists(path))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(path);
                }

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                using (var fileStream = File.OpenWrite(path))
                {
                    var mappings = new IconMappingList();

                    foreach (IconMappingViewModel viewModel in this.Mappings)
                    {
                        mappings.Add(viewModel.AsIconMapping());
                    }

                    IconMappingList.ReprocessMappings(mappings);
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
        /// Event handler for when the build font button is clicked.
        /// </summary>
        private void BuildFontButton_Click(object sender, RoutedEventArgs e)
        {
            var mappings = new IconMappingList();

            foreach (IconMappingViewModel viewModel in this.Mappings)
            {
                mappings.Add(viewModel.AsIconMapping());
            }

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(mappings);

            return;
        }

        /// <summary>
        /// Event handler for when the merge in mappings button is clicked.
        /// </summary>
        private void MergeInButton_Click(object sender, RoutedEventArgs e)
        {
            // Available for use to select a second mapping file and then merge
            // any common mappings from the second file into the current mappings list
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
