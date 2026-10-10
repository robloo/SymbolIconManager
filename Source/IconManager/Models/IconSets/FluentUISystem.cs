using Avalonia.Platform;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

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

        private static IReadOnlyList<FluentUISystemIcon>? _cachedIcons = null;
        private static IReadOnlyDictionary<uint, string>? _cachedFilledNames  = null;
        private static IReadOnlyDictionary<uint, string>? _cachedRegularNames = null;
        private static IReadOnlyList<Tuple<string, string>>? _cachedDeprecatedNames = null;

        private static Lock _cacheLock           = new();
        private static Lock _deprecatedNamesLock = new();

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<FluentUISystemIcon>();
            var filledNames = new Dictionary<uint, string>();
            var regularNames = new Dictionary<uint, string>();
            var sourceDataPaths = new Tuple<IconSet, IconTheme, string>[]
            {
                Tuple.Create(
                    IconSet.FluentUISystemFilled,
                    IconTheme.Filled,
                    IconSets.Paths.FluentUISystemFilled),
                Tuple.Create(
                    IconSet.FluentUISystemRegular,
                    IconTheme.Regular,
                    IconSets.Paths.FluentUISystemRegular
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
                using (var sourceStream = AssetLoader.Open(new Uri(entry.Item3)))
                using (var reader = new StreamReader(sourceStream))
                {
                    string jsonString = reader.ReadToEnd();
                    var rawIcons = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

                    if (rawIcons is not null)
                    {
                        foreach (var rawIcon in rawIcons)
                        {
                            var icon = new FluentUISystemIcon()
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

            lock (_cacheLock)
            {
                _cachedIcons        = icons.AsReadOnly();
                _cachedFilledNames  = filledNames;
                _cachedRegularNames = regularNames;
            }

            return;
        }

        private static void RebuildDeprecatedNamesCache()
        {
            var deprecatedNames = new List<Tuple<string, string>>();

            using (var sourceStream = AssetLoader.Open(new Uri(IconSets.Paths.FluentUISystemRenamedIcons)))
            using (var reader = new StreamReader(sourceStream))
            {
                string? line = reader.ReadLine();
                while (line is not null)
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
                            originalName = FluentUISystemIconName.ToBaseNameKey(originalName);
                            updatedName  = FluentUISystemIconName.ToBaseNameKey(updatedName);

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

            lock (_deprecatedNamesLock)
            {
                _cachedDeprecatedNames = deprecatedNames;
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

            lock (_cacheLock)
            {
                if (_cachedFilledNames is null ||
                    _cachedRegularNames is null)
                {
                    RebuildCache();
                }

                if (theme == IconTheme.Filled)
                {
                    _cachedFilledNames!.TryGetValue(unicodePoint, out name);
                }
                else
                {
                    _cachedRegularNames!.TryGetValue(unicodePoint, out name);
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
                lock (_cacheLock)
                {
                    if (_cachedIcons is null)
                    {
                        RebuildCache();
                    }
                }

                return _cachedIcons!;
            }
        }

        /// <summary>
        /// Gets all icons of the defined <see cref="IconTheme"/>.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> GetIcons(IconTheme theme)
        {
            var matchingIcons = new List<FluentUISystemIcon>();

            lock (_cacheLock)
            {
                if (_cachedIcons is null)
                {
                    RebuildCache();
                }

                foreach (FluentUISystemIcon icon in _cachedIcons!)
                {
                    if (icon.Theme == theme)
                    {
                        matchingIcons.Add(icon);
                    }
                }
            }

            return matchingIcons.AsReadOnly();
        }

        public static FluentUISystemIcon? FindIcon(
            string baseNameKey,
            IconSize desiredSize,
            IconTheme desiredTheme)
        {
            lock (_cacheLock)
            {
                if (_cachedIcons is null)
                {
                    RebuildCache();
                }

                foreach (FluentUISystemIcon icon in _cachedIcons!)
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
        public static IList<FluentUISystemIcon> FindIcons(
            string baseNameKey,
            IconTheme desiredTheme)
        {
            var matchingIcons = new List<FluentUISystemIcon>();

            lock (_cacheLock)
            {
                if (_cachedIcons is null)
                {
                    RebuildCache();
                }

                foreach (FluentUISystemIcon icon in _cachedIcons!)
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
        public static IList<FluentUISystemIcon> FindIcons(
            string baseNameKey,
            IconSize desiredSize)
        {
            var matchingIcons = new List<FluentUISystemIcon>();

            lock (_cacheLock)
            {
                if (_cachedIcons is null)
                {
                    RebuildCache();
                }

                foreach (FluentUISystemIcon icon in _cachedIcons!)
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
        public static FluentUISystemIcon? FindNearestSize(
            string baseNameKey,
            IconSize desiredSize,
            IconTheme theme)
        {
            var matches = FluentUISystem.FindIcons(
                baseNameKey,
                theme);

            if (matches is not null &&
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
        public static FluentUISystemIcon? ConvertToSize(
            FluentUISystemIcon icon,
            IconSize desiredSize,
            bool allowApproximate = true)
        {
            var sourceFluentUIName = new FluentUISystemIconName(icon.Name);

            if (sourceFluentUIName.Size == desiredSize)
            {
                return icon.Clone();
            }
            else
            {
                // Attempt to find an exact size match
                FluentUISystemIcon? match = FluentUISystem.FindIcon(
                    sourceFluentUIName.BaseNameKey,
                    desiredSize,
                    sourceFluentUIName.Theme);

                if (match is not null)
                {
                    // Return the exact match
                    return match;
                }
                else if (allowApproximate)
                {
                    FluentUISystemIcon? closestMatch = FluentUISystem.FindNearestSize(
                        sourceFluentUIName.BaseNameKey,
                        desiredSize,
                        sourceFluentUIName.Theme);

                    if (closestMatch is not null)
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
            var finalMappings = new IconMappingList();

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
                    FluentUISystemIcon? convertedSourceIcon = FluentUISystem.ConvertToSize(
                        new FluentUISystemIcon()
                        {
                            Name         = mappings[i].Source.Name,
                            UnicodePoint = mappings[i].Source.UnicodePoint,
                            // IconSet is determined automatically from the name
                        },
                        desiredSize,
                        allowApproximate: true);

                    if (convertedSourceIcon is null)
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
        public static Tuple<bool, FluentUISystemIcon> UpdateDeprecated(FluentUISystemIcon icon)
        {
            string updatedBaseNameKey = FindUpdatedBaseNameKey(icon.BaseNameKey);

            if (string.IsNullOrEmpty(updatedBaseNameKey) == false)
            {
                // Attempt to find an exact match
                FluentUISystemIcon? match = FluentUISystem.FindIcon(
                    updatedBaseNameKey,
                    icon.Size,
                    icon.Theme);

                if (match is not null)
                {
                    // Return the exact match
                    return Tuple.Create(true, match.Clone());
                }
                else
                {
                    FluentUISystemIcon? closestMatch = FluentUISystem.FindNearestSize(
                        updatedBaseNameKey,
                        icon.Size,
                        icon.Theme);

                    if (closestMatch is not null)
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
                lock (_deprecatedNamesLock)
                {
                    if (_cachedDeprecatedNames is null)
                    {
                        RebuildDeprecatedNamesCache();
                    }

                    foreach (var entry in _cachedDeprecatedNames!)
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
        public static void BuildRemoteGlyphSourcePaths()
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
    }
}
