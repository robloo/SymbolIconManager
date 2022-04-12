using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace IconManager
{
    public partial class AppToolsView : UserControl
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public AppToolsView()
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
            this.SourceIconSetComboBox.Items = iconSets;
            this.SourceIconSetComboBox.SelectedIndex = 0; // Should be Undefined

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the list of icons to display.
        /// </summary>
        public ObservableCollection<IconViewModel> ListedIcons { get; } = new ObservableCollection<IconViewModel>();

        /// <summary>
        /// Gets the source code (both XAML/C#) file path.
        /// </summary>
        public string SourcePath { get; private set; } = string.Empty;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private List<IconViewModel> ListUsedIcons(IconSet iconSet)
        {
            var locatedIcons = new List<IconViewModel>();

            if (Directory.Exists(this.SourcePath))
            {
                SearchExtension("*.xaml", "Glyph=\"&#x",   ";\"");    // FontIcon
                SearchExtension("*.xaml", "Text=\"&#x",    ";\"");    // TextBlock
                SearchExtension("*.cs",   "Glyph=\"\"&#x", ";\"\"");  // FontIcon in XAML code string literal
                SearchExtension("*.cs",   "Glyph=\"&#x",   ";\"");    // FontIcon in XAML code string
                SearchExtension("*.cs",   "= \"\\u",       "\"");
            }

            // Sort by IconSet then Unicode point
            locatedIcons.Sort((x, y) =>
            {
                if (x.IconSet == y.IconSet)
                {
                    return x.UnicodePoint.CompareTo(y.UnicodePoint);
                }
                else
                {
                    return x.IconSet.CompareTo(y.IconSet);
                }
            });

#if DEBUG
            // Output all located icons to the console
            foreach (var entry in locatedIcons)
            {
                if (iconSet == IconSet.SegoeMDL2Assets)
                {
                    // Help build the mapping table
                    //System.Diagnostics.Debug.WriteLine("{\"" + entry.UnicodePoint + "\", \"" + entry.Name + "\", \"\", \"\"},");
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(entry.UnicodePoint);
                }
            }
#endif

            // Local function to search for icons by file extension
            void SearchExtension(string fileExtension, string startPattern, string endPattern)
            {
                foreach (string path in Directory.EnumerateFiles(this.SourcePath, fileExtension, SearchOption.AllDirectories))
                {
                    string fileText = File.ReadAllText(path);

                    int index = fileText.IndexOf(startPattern, 0, StringComparison.OrdinalIgnoreCase);
                    while (index >= 0)
                    {
                        int endIndex = fileText.IndexOf(endPattern, index + startPattern.Length, StringComparison.OrdinalIgnoreCase);

                        if (endIndex >= 0)
                        {
                            var entry = new IconViewModel()
                            {
                                IconSet      = iconSet,
                                UnicodePoint = Convert.ToUInt32(
                                    fileText.Substring(
                                        index + startPattern.Length,
                                        endIndex - (index + startPattern.Length)),
                                    16),
                            };

                            // Check if the icon is in the set being search for
                            bool includeIcon = true;
                            if (iconSet == IconSet.Undefined)
                            {
                                includeIcon = true;
                            }
                            else
                            {
                                var iconList = IconSetBase.GetIcons(iconSet);
                                bool existsInIconSet = false;
                                for (int i = 0; i < iconList.Count; i++)
                                {
                                    if (entry.UnicodePoint == iconList[i].UnicodePoint)
                                    {
                                        existsInIconSet = true;
                                        break;
                                    }
                                }

                                includeIcon = existsInIconSet;
                            }

                            // Only add new icons
                            if (includeIcon)
                            {
                                bool exists = false;
                                foreach (var existingEntry in locatedIcons)
                                {
                                    if (entry.UnicodePoint == existingEntry.UnicodePoint)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }

                                if (exists == false)
                                {
                                    locatedIcons.Add(entry);
                                }
                            }
                        }

                        index = fileText.IndexOf(startPattern, index + startPattern.Length, StringComparison.OrdinalIgnoreCase);
                    }
                }

                return;
            }

            return locatedIcons;
        }

        private void RemapIcons(
            IconMappingList mappings,
            IconSet originalIconSet)
        {
            if (mappings != null)
            {
                // Confirm all mappings exist before continuing
                // This avoids a partial/corrupt conversion that cannot be easily reversed
                bool allMappingsExist = true;
                var usedIcons = this.ListUsedIcons(originalIconSet);

                // Functionality is currently disabled
                /*
                foreach (var entry in usedIcons)
                {
                    bool mappingExists = false;
                    for (int i = 0; i < mappings.Count; i++)
                    {
                        if (entry.UnicodePoint == mappings[i].Source.UnicodePoint)
                        {
                            mappingExists = true;
                            break;
                        }
                    }

                    if (mappingExists == false)
                    {
                        allMappingsExist = false;
                        break;
                    }
                }
                */

                if (allMappingsExist)
                {
                    ReplaceGlyphs("*.xaml", "Glyph=\"&#x",   ";\"");    // FontIcon
                    ReplaceGlyphs("*.xaml", "Text=\"&#x",    ";\"");    // TextBlock
                    ReplaceGlyphs("*.cs",   "Glyph=\"\"&#x", ";\"\"");  // FontIcon in XAML code string literal
                    ReplaceGlyphs("*.cs",   "Glyph=\"&#x",   ";\"");    // FontIcon in XAML code string
                    ReplaceGlyphs("*.cs",   "= \"\\u",       "\"");
                }
            }

            // Local function to remap glyphs in all files of the specified extension
            void ReplaceGlyphs(string fileExtension, string startPattern, string endPattern)
            {
                foreach (string path in Directory.EnumerateFiles(this.SourcePath, fileExtension, SearchOption.AllDirectories))
                {
                    string fileText = File.ReadAllText(path);
                    bool fileModified = false;

                    int index = fileText.IndexOf(startPattern, 0, StringComparison.OrdinalIgnoreCase);
                    while (index >= 0)
                    {
                        int endIndex = fileText.IndexOf(endPattern, index + startPattern.Length, StringComparison.OrdinalIgnoreCase);

                        if (endIndex >= 0)
                        {
                            uint unicode = Convert.ToUInt32(
                                fileText.Substring(
                                    index + startPattern.Length,
                                    endIndex - (index + startPattern.Length)),
                                16);
                            string unicodeHex = fileText.Substring(
                                index + startPattern.Length,
                                endIndex - (index + startPattern.Length));

                            int mappingIndex = -1;
                            for (int i = 0; i < mappings.Count; i++)
                            {
                                if (originalIconSet == IconSet.Undefined &&
                                    unicode == mappings[i].Source.UnicodePoint)
                                {
                                    mappingIndex = i;
                                    break;
                                }
                                else if (originalIconSet == mappings[i].Source.IconSet &&
                                         unicode == mappings[i].Source.UnicodePoint)
                                {
                                    mappingIndex = i;
                                    break;
                                }
                            }

                            if (mappingIndex >= 0)
                            {
                                // Case replacing is ugly... but Regex doesn't handle \u without extra work

                                // Original case
                                fileText = fileText.Replace(
                                    startPattern + unicodeHex,
                                    startPattern + mappings[mappingIndex].Destination.UnicodeHexString);

                                // Lowercase
                                fileText = fileText.Replace(
                                    startPattern.Substring(0, startPattern.Length - 1) + startPattern.Substring(startPattern.Length - 1, 1).ToLowerInvariant() + unicodeHex.ToLowerInvariant(),
                                    startPattern + mappings[mappingIndex].Destination.UnicodeHexString);

                                // Uppercase
                                fileText = fileText.Replace(
                                    startPattern.Substring(0, startPattern.Length - 1) + startPattern.Substring(startPattern.Length - 1, 1).ToUpperInvariant() + unicodeHex.ToUpperInvariant(),
                                    startPattern + mappings[mappingIndex].Destination.UnicodeHexString);

                                fileModified = true;
                            }
                        }

                        index = fileText.IndexOf(startPattern, index + startPattern.Length, StringComparison.OrdinalIgnoreCase);
                    }

                    if (fileModified)
                    {
                        var utf8 = new System.Text.UTF8Encoding(true);
                        File.WriteAllText(path, fileText, utf8);
                    }
                }

                return;
            }

            return;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        /// <summary>
        /// Event handler for when the select source directory button is clicked.
        /// </summary>
        private async void SourceDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(App.MainWindow);

            this.SourcePath = result;
            this.SourcePathTextBlock.Text = this.SourcePath;

            return;
        }

        /// <summary>
        /// Event handler for when the list icons button is clicked.
        /// </summary>
        private void ListIconsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.SourceIconSetComboBox.SelectedItem as ComboBoxItem;
            var selectedIconSet = (IconSet?)Enum.Parse(typeof(IconSet), selectedItem?.Tag?.ToString() ?? string.Empty);

            var usedGlyphs = this.ListUsedIcons(selectedIconSet ?? IconSet.Undefined);

            // Update the listed glyphs for display to the user
            this.ListedIcons.Clear();
            foreach (var entry in usedGlyphs)
            {
                entry.UpdateGlyphAsync();
                this.ListedIcons.Add(entry);
            }

            return;
        }

        /// <summary>
        /// Event handler for when the remap icons button is clicked.
        /// </summary>
        private void RemapIconsButton_Click(object sender, RoutedEventArgs e)
        {
            // Not currently general purpose, code must be modified here for use
            /*
            this.RemapIcons(
                IconMappingList.Load(IconSet.SegoeFluent),
                IconSet.FluentUISystemRegular);
            */
            return;
        }

        private async void ExportToImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListedIcons.Count > 0)
            {
                var dialog = new OpenFolderDialog();
                var path = await dialog.ShowAsync(App.MainWindow);

                if (path != null)
                {
                    string? directoryName = Path.GetDirectoryName(path);
                    if (directoryName != null &&
                        Directory.Exists(directoryName) == false)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    foreach (IconViewModel viewModel in this.ListedIcons)
                    {
                        string filePath = Path.Combine(path, viewModel.UnicodeHexString.ToLowerInvariant() + ".png");

                        if (File.Exists(filePath))
                        {
                            // Delete the existing file, it will be replaced
                            File.Delete(filePath);
                        }

                        Bitmap? bitmap = await GlyphRenderer.GetBitmapAsync(viewModel.IconSet, viewModel.UnicodePoint);
                        bitmap?.Save(filePath);
                    }
                }
            }

            return;
        }

        private async void ExportFileToImagesButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            var paths = await openDialog.ShowAsync(App.MainWindow);
            var glyphs = new List<Tuple<string, string, string>>();

            // Load the glyphs directly from a CSV file
            // Format is "Font, UnicodePoint, ImageFileName"
            if (paths != null &&
                paths.Length > 0 &&
                File.Exists(paths[0]))
            {
                using (var fileStream = File.OpenRead(paths[0]))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        while (reader.EndOfStream == false)
                        {
                            string? line = reader.ReadLine();
                            string[] columns = line?.Split(',') ?? new string[0];

                            if (columns.Length == 3)
                            {
                                glyphs.Add(Tuple.Create(
                                    columns[0].Trim(),
                                    columns[1].Trim(),
                                    columns[2].Trim()));
                            }
                        }
                    }
                }
            }

            if (glyphs.Count > 0)
            {
                string outputDirectory;
                int currSuffix = -1;

                // Create a temp directory
                do
                {
                    currSuffix++;

                    // Start from the directory of the running application
                    outputDirectory = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Glyphs" + currSuffix.ToString(CultureInfo.InvariantCulture));

                } while (Directory.Exists(outputDirectory));

                if (outputDirectory != null &&
                    Directory.Exists(outputDirectory) == false)
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Create and export a bitmap for each glyph
                foreach (var glyph in glyphs)
                {
                    string filePath = Path.Combine(outputDirectory!, glyph.Item3);

                    if (File.Exists(filePath))
                    {
                        // Delete the existing file, it will be replaced
                        File.Delete(filePath);
                    }

                    uint unicodePoint = 0;
                    try
                    {
                        if (glyph.Item2.StartsWith("0x"))
                        {
                            unicodePoint = Convert.ToUInt32(glyph.Item2.Substring(2), 16);
                        }
                        else
                        {
                            unicodePoint = Convert.ToUInt32(glyph.Item2, 16);
                        }
                    }
                    catch { }

                    var font = GlyphRenderer.LoadFont(glyph.Item1);

                    if (font != null)
                    {
                        var bitmap = await GlyphRenderer.RenderGlyph(font, glyph.Item1, unicodePoint);
                        bitmap?.Save(filePath);
                    }
                }

                // Open the output location for the end-user
                try
                {
                    if (System.OperatingSystem.IsWindows())
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            Arguments = outputDirectory!,
                            FileName = "explorer.exe"
                        });
                    }
                }
                catch { }
            }

            return;
        }

        private async void ExportToMappingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListedIcons.Count > 0)
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

                    string? directoryName = Path.GetDirectoryName(path);
                    if (directoryName != null &&
                        Directory.Exists(directoryName) == false)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    using (var fileStream = File.OpenWrite(path))
                    {
                        var mappings = new IconMappingList();

                        foreach (IconViewModel viewModel in this.ListedIcons)
                        {
                            var mapping = new IconMapping()
                            {
                                Source               = new Icon(),
                                Destination          = viewModel.AsIcon(),
                                GlyphMatchQuality    = MatchQuality.NoMatch,
                                MetaphorMatchQuality = MatchQuality.NoMatch,
                                IsPlaceholder        = false,
                                Comments             = string.Empty
                            };

                            mappings.Add(mapping);
                        }

                        mappings.Reprocess();
                        IconMappingList.Save(mappings, fileStream);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Event handler for when the build font button is clicked.
        /// </summary>
        private void BuildFontButton_Click(object sender, RoutedEventArgs e)
        {

            return;
        }

        private void IconViewModel_Click(object sender, RoutedEventArgs e)
        {
            var entry = ((Control)sender).Tag as IconViewModel;

            // Show a list of all mappings by icon set

            /*
            var textBlock = new TextBlock()
            {
                Text = entry.Glyph,
            };

            ToolTip.SetTip((Control)sender, textBlock);
            ToolTip.SetIsOpen((Control)sender, true);
            */

            return;
        }
    }
}
