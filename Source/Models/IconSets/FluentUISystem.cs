using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Fluent UI System Icons.
    /// </summary>
    public class FluentUISystem : IconSetBase
    {
        /// <summary>
        /// Defines an icon size within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum IconSize
        {
            Size12 = 12,
            Size16 = 16,
            /// <summary>
            /// Designed specifically for desktop.
            /// </summary>
            Size20 = 20,
            /// <remarks>
            /// The 24-pixel art board uses a 1.5 pixel stroke.
            /// This will not align with the pixel grid and may appear blurry.
            /// </remarks>
            Size24 = 24,
            Size28 = 28,
            Size32 = 32,
            Size48 = 48
        }

        /// <summary>
        /// Defines an icon theme within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum IconTheme
        {
            /// <summary>
            /// Used when weight is needed.
            /// </summary>
            Filled,
            /// <summary>
            /// Standard theme for use. These have a nice, friendly weight to them.
            /// </summary>
            Regular
        }

        /// <summary>
        /// Defines an icon naming format within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum NamingFormat
        {
            Android,
            iOS
        }

        private static IReadOnlyList<Icon>? cachedIcons = null;
        private static IReadOnlyDictionary<uint, string>? cachedFilledNames  = null;
        private static IReadOnlyDictionary<uint, string>? cachedRegularNames = null;
        private static IReadOnlyList<Tuple<string, string>>? cachedDeprecatedNames = null;

        private static object cacheMutex           = new object();
        private static object deprecatedNamesMutex = new object();

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<Icon>();
            var filledNames = new Dictionary<uint, string>();
            var regularNames = new Dictionary<uint, string>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var sourceDataPaths = new Tuple<IconSet, IconTheme, string>[]
            {
                Tuple.Create(
                    IconSet.FluentUISystemFilled,
                    IconTheme.Filled,
                    "avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Filled.json"),
                Tuple.Create(
                    IconSet.FluentUISystemRegular,
                    IconTheme.Regular,
                    "avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Regular.json"
                )
            };

            // Load all data from JSON source files
            //
            // The original JSON format was similar to:
            //
            //  {
            //      "ic_fluent_access_time_24_regular": "0xf101",
            //      "ic_fluent_accessibility_16_regular": "0xf102",
            //  }
            //
            // However, between versions 1.1.162 and 1.1.193 is was changed to:
            //
            //  {
            //    "ic_fluent_access_time_24_regular": 61697,
            //    "ic_fluent_accessibility_16_regular": 61698,
            //  }
            //
            //  * Indent spacing reduced from 4 to 2
            //  * Unicode point value changed from a hex string to an int
            //
            // This required changing how the files are parsed here and it isn't
            // backwards compatible. All files must use the latest format.

            foreach (var entry in sourceDataPaths)
            {
                using (var sourceStream = assets.Open(new Uri(entry.Item3)))
                using (var reader = new StreamReader(sourceStream))
                {
                    string jsonString = reader.ReadToEnd();
                    var rawIcons = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

                    if (rawIcons != null)
                    {
                        foreach (var rawIcon in rawIcons)
                        {
                            var icon = new Icon()
                            {
                                RawName      = rawIcon.Key,
                                Name         = rawIcon.Key, // Automatically parses into components
                                UnicodePoint = (uint)rawIcon.Value
                            };

                            icons.Add(icon);

                            if (icon.Theme == IconTheme.Filled)
                            {
                                filledNames.Add(icon.UnicodePoint, icon.Name);
                            }
                            else
                            {
                                regularNames.Add(icon.UnicodePoint, icon.Name);
                            }
                        }
                    }
                }
            }

            lock (cacheMutex)
            {
                cachedIcons        = icons.AsReadOnly();
                cachedFilledNames  = filledNames;
                cachedRegularNames = regularNames;
            }

            return;
        }

        private static void RebuildDeprecatedNamesCache()
        {
            var deprecatedNames = new List<Tuple<string, string>>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            using (var sourceStream = assets.Open(new Uri("avares://IconManager/Data/FluentUISystem/FluentUISystemRenamedIcons.txt")))
            using (var reader = new StreamReader(sourceStream))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    string processedLine = line.Trim();

                    if (processedLine.Length > 0 &&
                        processedLine.StartsWith("//") == false)
                    {
                        // Remove any end-of-line comments
                        int index = processedLine.IndexOf("//", StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            processedLine = processedLine.Substring(0, index);
                        }

                        string[] namePair = processedLine.Split(
                            new string[] { "→", "->" },
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        if (namePair.Length != 2)
                        {
                            // Error parsing line, skip it
                        }
                        else
                        {
                            string originalName = namePair[0];
                            string updatedName  = namePair[1];

                            // Reduce to an interim base name
                            originalName = ExtractBaseName(originalName);
                            updatedName  = ExtractBaseName(updatedName);

                            // Standardize into a universal base name key format
                            originalName = IconName.ToBaseNameKey(originalName);
                            updatedName  = IconName.ToBaseNameKey(updatedName);

                            deprecatedNames.Add(Tuple.Create(originalName, updatedName));
                        }
                    }

                    line = reader.ReadLine();
                }
            }

            // Remove any entries that have the same original and updated names
            for (int i = (deprecatedNames.Count - 1); i >= 0; i--)
            {
                if (string.Equals(deprecatedNames[i].Item1, deprecatedNames[i].Item2, StringComparison.OrdinalIgnoreCase))
                {
                    deprecatedNames.RemoveAt(i);
                }
            }

            lock (deprecatedNamesMutex)
            {
                cachedDeprecatedNames = deprecatedNames;
            }

            // Local function to extract the base name from a Fluent UI System icon name
            string ExtractBaseName(string startingString)
            {
                string baseName = startingString.ToLowerInvariant();

                // Sometimes a file extension is given, remove it
                if (baseName.EndsWith(".pdf"))
                {
                    baseName = baseName.Replace(".pdf", string.Empty);
                }

                if (baseName.EndsWith(".svg"))
                {
                    baseName = baseName.Replace(".svg", string.Empty);
                }

                // A spelling error was corrected at one point that throws off all processing
                // This must be specially removed here for now
                // As a side-effect, this case is never handled
                if (baseName.Contains("fiiled"))
                {
                    baseName = baseName.Replace("fiiled", "filled");
                }

                // Android format
                if (baseName.StartsWith("ic_fluent_"))
                {
                    baseName = baseName.Substring("ic_fluent_".Length);
                }

                if (baseName.EndsWith("_filled") || baseName.EndsWith("_regular"))
                {
                    baseName = baseName.Replace("_filled", string.Empty);
                    baseName = baseName.Replace("_regular", string.Empty);

                    if (baseName.Contains("_"))
                    {
                        var sizeStr = baseName.Substring(baseName.LastIndexOf("_") + 1);
                        bool isSizeGiven = int.TryParse(sizeStr, out int size);

                        if (isSizeGiven)
                        {
                            baseName = baseName.Substring(0, baseName.LastIndexOf("_"));
                        }
                    }
                }

                // iOS format
                if (baseName.EndsWith("filled") || baseName.EndsWith("regular"))
                {
                    baseName = baseName.Replace("filled", string.Empty);
                    baseName = baseName.Replace("regular", string.Empty);
                }

                return baseName;
            }

            return;
        }

        /// <summary>
        /// The name will be in the Android format: "ic_fluent_panel_right_contract_16_regular"
        /// </summary>
        public static string FindName(uint unicodePoint, IconTheme theme)
        {
            string? name = null;

            lock (cacheMutex)
            {
                if (cachedFilledNames == null ||
                    cachedRegularNames == null)
                {
                    RebuildCache();
                }

                if (theme == IconTheme.Filled)
                {
                    cachedFilledNames!.TryGetValue(unicodePoint, out name);
                }
                else
                {
                    cachedRegularNames!.TryGetValue(unicodePoint, out name);
                }
            }

            return name ?? string.Empty;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Fluent UI System icon set family.
        /// This includes BOTH the regular and filled themes.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> Icons
        {
            get
            {
                lock (cacheMutex)
                {
                    if (cachedIcons == null)
                    {
                        RebuildCache();
                    }
                }

                return cachedIcons!;
            }
        }

        /// <summary>
        /// Gets all icons of the defined <see cref="IconTheme"/>.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> GetIcons(IconTheme theme)
        {
            var matchingIcons = new List<Icon>();

            lock (cacheMutex)
            {
                if (cachedIcons == null)
                {
                    RebuildCache();
                }

                foreach (Icon icon in cachedIcons!)
                {
                    if (icon.Theme == theme)
                    {
                        matchingIcons.Add(icon);
                    }
                }
            }

            return matchingIcons.AsReadOnly();
        }

        public static Icon? FindIcon(
            string baseNameKey,
            IconSize desiredSize,
            IconTheme desiredTheme)
        {
            lock (cacheMutex)
            {
                if (cachedIcons == null)
                {
                    RebuildCache();
                }

                foreach (Icon icon in cachedIcons!)
                {
                    if (string.Equals(icon.BaseNameKey, baseNameKey, StringComparison.OrdinalIgnoreCase) &&
                        icon.Size == desiredSize &&
                        icon.Theme == desiredTheme)
                    {
                        return icon.Clone();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all sizes of icons matching the base name key and desired theme.
        /// </summary>
        public static IList<Icon> FindIcons(
            string baseNameKey,
            IconTheme desiredTheme)
        {
            var matchingIcons = new List<Icon>();

            lock (cacheMutex)
            {
                if (cachedIcons == null)
                {
                    RebuildCache();
                }

                foreach (Icon icon in cachedIcons!)
                {
                    if (string.Equals(icon.BaseNameKey, baseNameKey, StringComparison.OrdinalIgnoreCase) &&
                        icon.Theme == desiredTheme)
                    {
                        matchingIcons.Add(icon.Clone());
                    }
                }
            }

            return matchingIcons;
        }

        /// <summary>
        /// Finds all themes of icons matching the base name key and desired size.
        /// </summary>
        public static IList<Icon> FindIcons(
            string baseNameKey,
            IconSize desiredSize)
        {
            var matchingIcons = new List<Icon>();

            lock (cacheMutex)
            {
                if (cachedIcons == null)
                {
                    RebuildCache();
                }

                foreach (Icon icon in cachedIcons!)
                {
                    if (string.Equals(icon.BaseNameKey, baseNameKey, StringComparison.OrdinalIgnoreCase) &&
                        icon.Size == desiredSize)
                    {
                        matchingIcons.Add(icon.Clone());
                    }
                }
            }

            return matchingIcons;
        }

        /// <summary>
        /// Finds an equivalent icon (same base name key and theme) that is closest to the desired size.
        /// </summary>
        public static Icon? FindNearestSize(
            string baseNameKey,
            IconSize desiredSize,
            IconTheme theme)
        {
            var matches = FluentUISystem.FindIcons(
                baseNameKey,
                theme);

            if (matches != null &&
                matches.Count > 0)
            {
                // To find the numerically closest match in size, simply find the difference from the desired size
                // to actual size for each item, sort from smallest to largest, then take the first item
                var closestMatch = matches.OrderBy(icon => Math.Abs((int)desiredSize - icon.NumericalSize)).First();
                        
                return closestMatch;
            }

            return null;
        }

        /// <summary>
        /// Rebuilds the given icon to match the desired size (or the next closest size available).
        /// The FluentUISystem name is used directly as an ID in order to do this - Unicode point is ignored.
        /// </summary>
        /// <param name="icon">The icon to convert.</param>
        /// <param name="desiredSize">The desired size of the icon.</param>
        /// <param name="allowApproximate">Set to true to return the next nearest size if
        /// an exact match isn't available.</param>
        /// <returns>A new icon with the desired size; otherwise,
        /// the next closest size available.</returns>
        public static FluentUISystem.Icon? ConvertToSize(
            FluentUISystem.Icon icon,
            IconSize desiredSize,
            bool allowApproximate = true)
        {
            var sourceFluentUIName = new FluentUISystem.IconName(icon.Name);

            if (sourceFluentUIName.Size == desiredSize)
            {
                return icon.Clone();
            }
            else
            {
                // Attempt to find an exact size match
                FluentUISystem.Icon? match = FluentUISystem.FindIcon(
                    sourceFluentUIName.BaseNameKey,
                    desiredSize,
                    sourceFluentUIName.Theme);

                if (match != null)
                {
                    // Return the exact match
                    return match;
                }
                else if (allowApproximate)
                {
                    FluentUISystem.Icon? closestMatch = FluentUISystem.FindNearestSize(
                        sourceFluentUIName.BaseNameKey,
                        desiredSize,
                        sourceFluentUIName.Theme);

                    if (closestMatch != null)
                    {
                        // Use the nearest numerical size
                        return closestMatch;
                    }
                    else
                    {
                        // Nothing was found, just return the input which is already the closest size
                        // Note that to get here an error must have occurred with the icon name
                        return icon.Clone();
                    }
                }
                else
                {
                    // Unable to convert the icon size
                    return null;
                }
            }
        }

        /// <summary>
        /// Rebuilds the given mapping list's source icons to match the desired size.
        /// The source icons must be within the FluentUISystem family or no changes will be made.
        /// </summary>
        /// <param name="mappings">The list of FluentUISystem mappings to convert.</param>
        /// <param name="desiredSize">The desired size of all source icons.</param>
        /// <returns>A new list of mappings with source icons changed to the desired size.</returns>
        public static IconMappingList ConvertToSize(
            IconMappingList mappings,
            IconSize desiredSize)
        {
            int nonExactMappings = 0;
            int missingMappings = 0;
            int invalidMappings = 0;
            IconMappingList finalMappings = new IconMappingList();

            for (int i = 0; i < mappings.Count; i++)
            {
                if (mappings[i].Source.IconSet != IconSet.FluentUISystemFilled &&
                    mappings[i].Source.IconSet != IconSet.FluentUISystemRegular)
                {
                    // The mapping was not sourced from the FluentUISystem family
                    // In that case, just copy it to the final mappings unchanged
                    finalMappings.Add(mappings[i].Clone());

                    invalidMappings++;
                }
                else
                {
                    FluentUISystem.Icon? convertedSourceIcon = FluentUISystem.ConvertToSize(
                        new FluentUISystem.Icon()
                        {
                            Name         = mappings[i].Source.Name,
                            UnicodePoint = mappings[i].Source.UnicodePoint,
                            // IconSet is determined automatically from the name
                        },
                        desiredSize,
                        allowApproximate: true);

                    if (convertedSourceIcon == null)
                    {
                        missingMappings++;
                    }
                    else if (convertedSourceIcon.Size != desiredSize)
                    {
                        nonExactMappings++;
                    }

                    var newMapping = new IconMapping()
                    {
                        Source               = convertedSourceIcon?.AsIcon() ?? mappings[i].Source.Clone(),
                        Destination          = mappings[i].Destination.Clone(),
                        GlyphMatchQuality    = mappings[i].GlyphMatchQuality,
                        MetaphorMatchQuality = mappings[i].MetaphorMatchQuality,
                        IsPlaceholder        = mappings[i].IsPlaceholder,
                        Comments             = mappings[i].Comments
                    };

                    finalMappings.Add(newMapping);
                }
            }

            return finalMappings;
        }

        /// <summary>
        /// Checks if the given icon is deprecated and, if so, finds the update.
        /// </summary>
        /// <param name="icon">The icon to check and get the updated version for.</param>
        /// <returns>Whether the given icon is deprecated along with any updated version.</returns>
        public static Tuple<bool, FluentUISystem.Icon> UpdateDeprecated(FluentUISystem.Icon icon)
        {
            string updatedBaseNameKey = FindUpdatedBaseNameKey(icon.BaseNameKey);

            if (string.IsNullOrEmpty(updatedBaseNameKey) == false)
            {
                // Attempt to find an exact match
                FluentUISystem.Icon? match = FluentUISystem.FindIcon(
                    updatedBaseNameKey,
                    icon.Size,
                    icon.Theme);

                if (match != null)
                {
                    // Return the exact match
                    return Tuple.Create(true, match.Clone());
                }
                else
                {
                    FluentUISystem.Icon? closestMatch = FluentUISystem.FindNearestSize(
                        updatedBaseNameKey,
                        icon.Size,
                        icon.Theme);

                    if (closestMatch != null)
                    {
                        // Use the nearest numerical size
                        // It is considered better to change the size of the icon than allow
                        // a deprecated one to remain. However, the chances of this happening are
                        // extremely rare. Upstream always renames and replaces with the same size.
                        return Tuple.Create(true, closestMatch.Clone());
                    }
                }
            }

            return Tuple.Create(false, icon.Clone());
        }

        /// <summary>
        /// Recursively finds any updated base name key for the given base name key (assumes it is deprecated).
        /// </summary>
        /// <param name="baseName">The base name key to find the update for.
        /// Warning: This must be in the universal key format.</param>
        /// <returns>The updated base name key; otherwise, an empty string.</returns>
        private static string FindUpdatedBaseNameKey(string baseNameKey)
        {
            if (string.IsNullOrEmpty(baseNameKey) == false)
            {
                // Search for an updated name
                lock (deprecatedNamesMutex)
                {
                    if (cachedDeprecatedNames == null)
                    {
                        RebuildDeprecatedNamesCache();
                    }

                    foreach (var entry in cachedDeprecatedNames!)
                    {
                        if (string.Equals(baseNameKey, entry.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            // Check for another update, these can chain together
                            string updatedName1 = entry.Item2;
                            string updatedName2 = FindUpdatedBaseNameKey(updatedName1);

                            return string.IsNullOrEmpty(updatedName2) ? updatedName1 : updatedName2;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Builds the full list of glyph sources for the Fluent UI System icons.
        /// Source files must already be downloaded and present in the cache.
        /// </summary>
        /// <remarks>
        /// This method is NOT intended for general-purpose use.
        /// It should only be used by those who know what they are doing to re-build the glyph sources.
        /// </remarks>
        public static void BuildGlyphSources()
        {
            List<string> glyphSources = new List<string>();
            List<string> searchDirectories = new List<string>();
            string fluentUISystemCacheDirectory = @"fluentui-system-icons";

            searchDirectories.Add(Path.Combine(App.IconManagerCache, fluentUISystemCacheDirectory, "assets"));
            //searchDirectories.Add(Path.Combine(new string[] {
            //    App.IconManagerCache,
            //    fluentUISystemCacheDirectory,
            //    "ios",
            //    "FluentIcons",
            //    "Assets"}));

            foreach (string searchDirectory in searchDirectories)
            {
                foreach (string filePath in Directory.EnumerateFiles(searchDirectory, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filePath).ToUpperInvariant() == ".PDF" ||
                        Path.GetExtension(filePath).ToUpperInvariant() == ".SVG")
                    {
                        glyphSources.Add(filePath.Replace(searchDirectory, string.Empty));
                    }
                }
            }

            glyphSources.Sort();

            var jsonString = JsonSerializer.Serialize(
                glyphSources.ToArray(),
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                });

            using (var fileStream = File.OpenWrite(Path.Combine(App.IconManagerCache, "FluentUISystemGlyphSources.json")))
            {
                fileStream.Write(Encoding.UTF8.GetBytes(jsonString));
            }

            return;
        }

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in the Fluent UI System.
        /// </summary>
        public class Icon : IconName, IIcon
        {
            /***************************************************************************************
             *
             * Property Accessors
             *
             ***************************************************************************************/

            ///////////////////////////////////////////////////////////
            // Data
            ///////////////////////////////////////////////////////////

            /// <inheritdoc/>
            public IconSet IconSet
            {
                get
                {
                    if (this.Theme == IconTheme.Filled)
                    {
                        return IconSet.FluentUISystemFilled;
                    }
                    else
                    {
                        return IconSet.FluentUISystemRegular;
                    }
                }
                set { /* Do nothing */ }
            }

            /// <summary>
            /// Gets or sets the raw, unparsed name or description of the icon.
            /// </summary>
            public string RawName { get; set; } = string.Empty;

            /// <inheritdoc/>
            public uint UnicodePoint { get; set; } = 0;

            ///////////////////////////////////////////////////////////
            // Calculated
            ///////////////////////////////////////////////////////////

            /// <inheritdoc/>
            public string UnicodeHexString
            {
                get => IconManager.Icon.ToUnicodeHexString(this.UnicodePoint);
            }

            /***************************************************************************************
             *
             * Methods
             *
             ***************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Icon"/> instance from this instance's values.
            /// </summary>
            /// <returns>The cloned <see cref="Icon"/>.</returns>
            public Icon Clone()
            {
                var clone = new Icon()
                {
                    RawName      = this.RawName,
                    Name         = this.Name, // Automatically parses into components
                    UnicodePoint = this.UnicodePoint
                };

                return clone;
            }

            /// <summary>
            /// Converts this <see cref="FluentUISystem.Icon"/> into a standard <see cref="IconManager.Icon"/>.
            /// This is sometimes needed because <see cref="FluentUISystem.Icon"/> does not derive from
            /// <see cref="IconManager.Icon"/> like most other icons do. It only implements the interface.
            /// </summary>
            /// <returns>A new <see cref="IconManager.Icon"/>.</returns>
            public IconManager.Icon AsIcon()
            {
                return new IconManager.Icon()
                {
                    IconSet      = this.IconSet,
                    Name         = this.Name,
                    UnicodePoint = this.UnicodePoint
                };
            }
        }

        /// <summary>
        /// Represents a single Fluent UI System icon name with all of
        /// its individual components.
        /// </summary>
        public class IconName
        {
            private string       _BaseName = string.Empty;
            private IconSize     _Size     = IconSize.Size12;
            private IconTheme    _Theme    = IconTheme.Regular;
            private NamingFormat _Format   = NamingFormat.Android;

            public IconName()
            {
            }

            /// <param name="name">The full name of the icon including all components.
            /// Example: ic_fluent_caret_up_24_filled.</param>
            public IconName(string name)
            {
                this.SetName(name);
            }

            /// <summary>
            /// Gets or sets the full name of the icon including all components.
            /// Example: ic_fluent_caret_up_24_filled.
            /// </summary>
            public string Name
            {
                get => this.GetName(
                    this.BaseName,
                    this.Size,
                    this.Theme,
                    this.Format);
                set => this.SetName(value);
            }

            /// <summary>
            /// Gets or sets the base name or description component corresponding to the
            /// metaphor the icon represents.
            /// Example: caret_up.
            /// </summary>
            public string BaseName
            {
                get => this._BaseName;
                set => this._BaseName = value;
            }

            /// <summary>
            /// Gets the base name in a universal format shared by both Android/iOS named formats.
            /// This may be used as a universal lookup key.
            /// </summary>
            public string BaseNameKey
            {
                get => IconName.ToBaseNameKey(this._BaseName);
            }

            /// <summary>
            /// Gets or sets the size component of the icon name.
            /// </summary>
            public IconSize Size
            {
                get => this._Size;
                set => this._Size = value;
            }

            /// <summary>
            /// Gets the size component of the icon name returned as an integer.
            /// </summary>
            public int NumericalSize
            {
                get => (int)this.Size;
            }

            /// <summary>
            /// Gets or sets the theme component of the icon name.
            /// </summary>
            public IconTheme Theme
            {
                get => this._Theme;
                set => this._Theme = value;
            }

            /// <summary>
            /// Gets or sets the format of the icon name.
            /// </summary>
            public NamingFormat Format
            {
                get => this._Format;
                set => this._Format = value;
            }

            /// <summary>
            /// Converts the given base name into a universal format shared by both Android/iOS
            /// named formats. This may be used as a universal lookup key.
            /// </summary>
            /// <param name="baseName">The base name to get the universal formatted key for.</param>
            /// <returns>The universally formatted base name usable as a lookup key.</returns>
            public static string ToBaseNameKey(string baseName)
            {
                baseName = baseName.ToLowerInvariant();

                if (baseName.StartsWith("ic_fluent_"))
                {
                    baseName = baseName.Substring("ic_fluent_".Length);
                }

                baseName = baseName
                    .Replace(" ", string.Empty)
                    .Replace("_", string.Empty);

                return baseName;
            }

            /// <summary>
            /// Detects the naming format of the given icon name.
            /// Only well-formed names will be detected properly; everything else will return null.
            /// </summary>
            /// <param name="name">The name to detect the format of.</param>
            /// <returns>The well-formed naming format; otherwise, null.</returns>
            public static NamingFormat? DetectFormat(string name)
            {
                string workingName = name.ToLowerInvariant().Trim();

                // iOS                  Android
                // caretUp12Filled      ic_fluent_caret_up_12_filled
                // callPark48Regular    ic_fluent_call_park_48_regular

                if (workingName.StartsWith("ic_fluent_") &&
                    (workingName.EndsWith("_filled") || workingName.EndsWith("_regular")))
                {
                    workingName = workingName.Substring("ic_fluent_".Length);
                    workingName = workingName.Replace("_filled", string.Empty);
                    workingName = workingName.Replace("_regular", string.Empty);

                    if (workingName.Contains("_"))
                    {
                        var sizeStr = workingName.Substring(workingName.LastIndexOf("_") + 1);
                        bool isSizeGiven = int.TryParse(sizeStr, out int size);

                        if (isSizeGiven)
                        {
                            if (Enum.IsDefined(typeof(IconSize), size))
                            {
                                return NamingFormat.Android;
                            }
                            else
                            {
                                // Currently allow even undefined icon sizes, just require a number
                                return NamingFormat.Android;
                            }
                        }
                    }
                }
                else if (workingName.EndsWith("filled") || workingName.EndsWith("regular"))
                {
                    // Allow anything
                    return NamingFormat.iOS;
                }

                return null;
            }

            /// <summary>
            /// Sets the icon name and extracts all components.
            /// </summary>
            /// <param name="name">The full name of the icon.</param>
            private void SetName(string name)
            {
                string workingName = name;

                // Must process formats as below:
                //
                // iOS                  Android
                // caretUp12Filled      ic_fluent_caret_up_12_filled
                // caretUp16Filled      ic_fluent_caret_up_16_filled
                // caretUp20Filled      ic_fluent_caret_up_20_filled
                // caretUp24Filled      ic_fluent_caret_up_24_filled
                //
                // callPark16Regular    ic_fluent_call_park_16_regular
                // callPark20Regular    ic_fluent_call_park_20_regular
                // callPark24Regular    ic_fluent_call_park_24_regular
                // callPark28Regular    ic_fluent_call_park_28_regular
                // callPark32Regular    ic_fluent_call_park_32_regular
                // callPark48Regular    ic_fluent_call_park_48_regular

                // Extract format
                var format = DetectFormat(workingName);
                if (format != null)
                {
                    this.Format = format.Value;
                }
                else
                {
                    // Default to iOS which is harder to detect
                    this.Format = NamingFormat.iOS;
                }

                string filledPattern1 = "_filled";
                string filledPattern2 = "Filled";
                string regularPattern1 = "_regular";
                string regularPattern2 = "Regular";

                // Extract theme
                if (workingName.EndsWith(filledPattern1, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Filled;
                    workingName = workingName.Substring(0, workingName.Length - filledPattern1.Length);
                }
                else if (workingName.EndsWith(filledPattern2, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Filled;
                    workingName = workingName.Substring(0, workingName.Length - filledPattern2.Length);
                }
                else if (workingName.EndsWith(regularPattern1, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Regular;
                    workingName = workingName.Substring(0, workingName.Length - regularPattern1.Length);
                }
                else if (workingName.EndsWith(regularPattern2, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Regular;
                    workingName = workingName.Substring(0, workingName.Length - regularPattern2.Length);
                }
                else
                {
                    // Default
                    this.Theme = IconTheme.Regular;
                }

                // Trim underscores
                if (workingName.EndsWith("_"))
                {
                    workingName = workingName.Substring(0, workingName.Length - 1);
                }

                // Extract size
                if (workingName.EndsWith("12", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size12;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("16", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size16;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("20", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size20;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("24", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size24;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("28", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size28;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("32", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size32;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("48", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size48;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else
                {
                    // Default
                    this.Size = IconSize.Size12;
                }

                // Trim underscores
                if (workingName.StartsWith("_"))
                {
                    workingName = workingName.Substring(1);
                }

                if (workingName.EndsWith("_"))
                {
                    workingName = workingName.Substring(0, workingName.Length - 1);
                }

                // Whatever is left is the base name
                this.BaseName = workingName;

                return;
            }

            /// <summary>
            /// Gets a full icon name built from individual components.
            /// </summary>
            /// <returns>The full name of the icon.</returns>
            private string GetName(
                string baseName,
                IconSize size,
                IconTheme theme,
                NamingFormat format)
            {
                StringBuilder sb = new StringBuilder();

                switch (format)
                {
                    case NamingFormat.Android:
                        {
                            if (baseName.StartsWith("ic_fluent_") == false)
                            {
                                sb.Append("ic_fluent_");
                            }

                            sb.Append(baseName);
                            sb.Append("_");
                            sb.Append(((int)size).ToString(CultureInfo.InvariantCulture));
                            sb.Append("_");

                            switch (theme)
                            {
                                case IconTheme.Filled:
                                    sb.Append("filled");
                                    break;
                                case IconTheme.Regular:
                                    sb.Append("regular");
                                    break;
                            }

                            // Failsafe
                            sb.Replace("__", "_");

                            return sb.ToString();
                        }
                    case NamingFormat.iOS:
                        {
                            sb.Append(baseName);
                            sb.Append(((int)size).ToString(CultureInfo.InvariantCulture));

                            switch (theme)
                            {
                                case IconTheme.Filled:
                                    sb.Append("Filled");
                                    break;
                                case IconTheme.Regular:
                                    sb.Append("Regular");
                                    break;
                            }

                            return sb.ToString();
                        }
                }

                return string.Empty;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return this.BaseName + " | " + this.Size + " | " + this.Theme;
            }
        }
    }
}
