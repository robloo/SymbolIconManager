using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using IconManager.Core.Icons;
using IconManager.Models;
using IconManager.Utilities;
using IconManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

// --- WARNING ---
// This code is not general-purpose and should be disabled by default in the UI.
// It was originally written to migrate an existing LOB application from one icon set to another.
// That functionality is largely no longer needed since drop-in replacement fonts are now available.
// However, it still may be useful to re-enable a subset of features in the future such as listing all icons
// used by source code.

namespace IconManager.ViewModels
{
    /// <summary>
    /// The primary view model for the <see cref="AppToolsView"/>
    /// </summary>
    public partial class AppToolsViewModel : ViewModelBase
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="AppToolsViewModel"/> class.
        /// </summary>
        public AppToolsViewModel()
        {
            this.FillSourceIconSets();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets the list of icons to display.
        /// </summary>
        public ObservableCollection<IconViewModel> DisplayedIcons { get; } = [];

        /// <summary>
        /// Gets the source code (both XAML/C#) directory path selected by the user.
        /// </summary>
        public string SourceDirectory
        {
            get => field;
            private set => this.SetField(ref field, value);
        } = string.Empty;

        /// <summary>
        /// Gets the collection of all icon sets that can be selected from for source code in <see cref="SourceDirectory"/>.
        /// </summary>
        public ObservableCollection<IconSetViewModel> SourceIconSets { get; } = [];

        /// <summary>
        /// Gets or sets the selected icon set that is used by the source code in <see cref="SourceDirectory"/>.
        /// </summary>
        public IconSetViewModel? SelectedSourceIconSet
        {
            get => field;
            set => this.SetField(ref field, value);
        } = null;

        /***************************************************************************************
         *
         * Commands
         *
         ***************************************************************************************/

        /// <summary>
        /// Select the directory of the source code.
        /// This is the first step before any processing.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task SelectSourceDirectory()
        {
            var options = new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
            };
            var folders = await App.Current!.TopLevel!.StorageProvider.OpenFolderPickerAsync(options);

            if (folders is not null &&
                folders.Count > 0)
            {
                this.SourceDirectory = folders[0].Path.AbsolutePath;
            }
            else
            {
                this.SourceDirectory = string.Empty;
            }

            return;
        }

        /// <summary>
        /// Finds and lists all icons used in the source code.
        /// This may be restricted to a specified icon set using <see cref="SelectedSourceIconSet"/>.
        /// </summary>
        [RelayCommand]
        public void ListIcons()
        {
            var selectedIconSet = this.SelectedSourceIconSet?.IconSet;
            var usedGlyphs = this.ListUsedIcons(selectedIconSet ?? IconSet.Undefined);

            // Update the listed glyphs for display to the user
            this.DisplayedIcons.Clear();
            foreach (var entry in usedGlyphs)
            {
                entry.UpdateGlyphAsync();
                this.DisplayedIcons.Add(entry);
            }

            return;
        }

        /// <summary>
        /// Event handler for when the remap icons button is clicked.
        /// </summary>
        [RelayCommand]
        public void RemapIcons()
        {
            // Not currently general purpose, code must be modified here for use
            /*
            this.RemapIcons(
                IconMappingList.Load(IconSet.SegoeFluent),
                IconSet.FluentUISystemRegular);
            */
            return;
        }

        /// <summary>
        /// Creates rendered images for each icon glyph in the displayed icons.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task ExportToImages()
        {
            if (this.DisplayedIcons.Count <= 0)
            {
                return;
            }

            var options = new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
            };
            var folders = await App.Current!.TopLevel!.StorageProvider.OpenFolderPickerAsync(options);

            if (folders is not null &&
                folders.Count > 0)
            {
                string path = folders[0].Path.AbsolutePath;
                string? directoryName = Path.GetDirectoryName(path);

                if (directoryName is not null &&
                    Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                foreach (IconViewModel viewModel in this.DisplayedIcons)
                {
                    string filePath = Path.Combine(path, viewModel.UnicodeHexString.ToLowerInvariant() + ".png");

                    if (File.Exists(filePath))
                    {
                        // Delete the existing file, it will be replaced
                        File.Delete(filePath);
                    }

                    Bitmap? bitmap = await GlyphRenderer.GetPreviewBitmapAsync(viewModel.IconSet, viewModel.UnicodePoint);
                    bitmap?.Save(filePath);
                }
            }

            return;
        }

        /// <summary>
        /// Creates rendered images for each icon glyph specified in the selected CSV file.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> representing the asynchronous operation.</returns>
        [RelayCommand]
        public async Task ExportFileToImages()
        {
            var options = new FilePickerOpenOptions()
            {
                AllowMultiple  = false,
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new FilePickerFileType("Comma-separated values files")
                    {
                        Patterns = ["*.csv"],
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*"],
                    },
                },
            };
            var files = await App.Current!.TopLevel!.StorageProvider.OpenFilePickerAsync(options);
            var glyphs = new List<Tuple<string, string, string>>();

            // Load the glyphs directly from a CSV file
            // Format is "Font, UnicodePoint, ImageFileName"
            if (files is not null &&
                files.Count > 0)
            {
                string path = files[0].Path.AbsolutePath;

                if (File.Exists(path))
                {
                    using (var fileStream = File.OpenRead(path))
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

                if (outputDirectory is not null &&
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

                    var font = GlyphProvider.LoadFont(glyph.Item1);

                    if (font is not null)
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

        /// <summary>
        /// Creates a new mappings file (with placeholder data) for every displayed icon.
        /// </summary>
        /// <returns>The new mappings file.</returns>
        [RelayCommand]
        public async Task ExportToMappings()
        {
            if (this.DisplayedIcons.Count <= 0)
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
                    var mappings = new IconMappingList();

                    foreach (IconViewModel viewModel in this.DisplayedIcons)
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

                    IconMappingUtilities.Reprocess(mappings);
                    IconMappingList.Save(mappings, fileStream);
                }
            }

            return;
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Scans all supported source code files in the <see cref="SourceDirectory"/> and creates a list of
        /// all used icons (with Unicode points).
        /// </summary>
        /// <param name="iconSet">
        ///   A specific icon set that can be used to restrict matches.
        ///   Set to <see cref="IconSet.Undefined"/> to find all icons.
        /// </param>
        private List<IconViewModel> ListUsedIcons(IconSet iconSet)
        {
            var locatedIcons = new List<IconViewModel>();

            if (Directory.Exists(this.SourceDirectory))
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
                foreach (string path in Directory.EnumerateFiles(this.SourceDirectory, fileExtension, SearchOption.AllDirectories))
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
            if (mappings is not null)
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
                foreach (string path in Directory.EnumerateFiles(this.SourceDirectory, fileExtension, SearchOption.AllDirectories))
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

        /// <summary>
        /// Fills all available icon sets.
        /// </summary>
        private void FillSourceIconSets()
        {
            this.SourceIconSets.Clear();

            var iconSetEnumValues = Enum.GetValues<IconSet>();
            foreach (IconSet value in iconSetEnumValues)
            {
                // Note: IconSet.Undefined is allowed here
                this.SourceIconSets.Add(new IconSetViewModel(value));
            }

            // Should be Undefined
            this.SelectedSourceIconSet = this.SourceIconSets[0];

            return;
        }
    }
}
