using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            // Sort by Unicode point
            locatedIcons.Sort((x, y) =>
            {
                return x.UnicodePoint.CompareTo(y.UnicodePoint);
            });

#if DEBUG
            // Output all located icons to the console
            foreach (var entry in locatedIcons)
            {
                if (iconSet == IconSet.SegoeMDL2Assets)
                {
                    // Help build the mapping table
                    System.Diagnostics.Debug.WriteLine("{\"" + entry.UnicodePoint + "\", \"" + entry.Name + "\", \"\", \"\"},");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(entry.UnicodePoint);
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
                            else if (iconSet == IconSet.SegoeMDL2Assets)
                            {
                                bool iconExistsInTable = false;
                                for (int i = 0; i < SegoeMDL2Assets.Icons.Count; i++)
                                {
                                    if (entry.UnicodePoint == SegoeMDL2Assets.Icons[i].UnicodePoint)
                                    {
                                        iconExistsInTable = true;
                                        break;
                                    }
                                }

                                includeIcon = iconExistsInTable;
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
                            var unicode = Convert.ToUInt32(
                                fileText.Substring(
                                    index + startPattern.Length,
                                    endIndex - (index + startPattern.Length)),
                                16);

                            int mappingIndex = -1;
                            for (int i = 0; i < mappings.Count; i++)
                            {
                                if (unicode == mappings[i].Source.UnicodePoint)
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
                                    startPattern + unicode,
                                    startPattern + mappings[mappingIndex].Destination.UnicodePoint);

                                // Lowercase
                                fileText = fileText.Replace(
                                    startPattern.Substring(0, startPattern.Length - 1) + startPattern.Substring(startPattern.Length - 1, 1).ToLowerInvariant() + unicode,
                                    startPattern + mappings[mappingIndex].Destination.UnicodePoint);

                                // Uppercase
                                fileText = fileText.Replace(
                                    startPattern.Substring(0, startPattern.Length - 1) + startPattern.Substring(startPattern.Length - 1, 1).ToUpperInvariant() + unicode,
                                    startPattern + mappings[mappingIndex].Destination.UnicodePoint);

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
            var usedGlyphs = this.ListUsedIcons(IconSet.SegoeMDL2Assets);

            // Update the listed glyphs for display to the user
            this.ListedIcons.Clear();
            foreach (var entry in usedGlyphs)
            {
                entry.DownloadGlyphImage();
                this.ListedIcons.Add(entry);
            }

            return;
        }

        /// <summary>
        /// Event handler for when the remap icons button is clicked.
        /// </summary>
        private void RemapIconsButton_Click(object sender, RoutedEventArgs e)
        {
            this.RemapIcons(
                SegoeMDL2AssetsToFluentUISystem.BuildMapping(FluentUISystem.IconSize.Size20),
                IconSet.SegoeMDL2Assets);

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
