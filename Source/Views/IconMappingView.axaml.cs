using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.IO;

namespace IconManager
{
    public partial class IconMappingView : UserControl
    {
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

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

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

            // Update the user-interface collection
            this.Mappings.Clear();
            foreach (IconMapping mapping in mappings)
            {
                var viewModel = new IconMappingViewModel(mapping);
                viewModel.SourceViewModel.AddGlyphAsync();
                viewModel.DestinationViewModel.AddGlyphAsync();

                this.Mappings.Add(viewModel);
            }

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
    }
}
