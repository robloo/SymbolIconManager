using Avalonia.Platform;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Icons8 Line Awesome icons.
    /// </summary>
    public class LineAwesome : IconSetBase
    {
        /// <summary>
        /// Defines the style of a <see cref="LineAwesome"/> icon.
        /// </summary>
        public enum IconStyle
        {
            /// <summary>
            /// Company branding icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'lab'.
            /// </remarks>
            Brand,

            /// <summary>
            /// Regular styled icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'lar'.
            /// </remarks>
            Regular,

            /// <summary>
            /// Solid fill styled icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'las'.
            /// </remarks>
            Solid
        }

        private static IReadOnlyList<LineAwesomeIcon>? _cachedIcons = null;
        private static IReadOnlyDictionary<uint, string>? _cachedBrandNames   = null;
        private static IReadOnlyDictionary<uint, string>? _cachedRegularNames = null;
        private static IReadOnlyDictionary<uint, string>? _cachedSolidNames   = null;

        private static Lock _cacheLock = new();

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<LineAwesomeIcon>();
            var brandNames = new Dictionary<uint, string>();
            var regularNames = new Dictionary<uint, string>();
            var solidNames = new Dictionary<uint, string>();
            var sourceDataPaths = new Tuple<IconSet, IconStyle, string>[]
            {
                Tuple.Create(
                    IconSet.LineAwesomeBrand,
                    IconStyle.Brand,
                    IconSets.Paths.LineAwesomeBrands),
                Tuple.Create(
                    IconSet.LineAwesomeRegular,
                    IconStyle.Regular,
                    IconSets.Paths.LineAwesomeRegular),
                Tuple.Create(
                    IconSet.LineAwesomeSolid,
                    IconStyle.Solid,
                    IconSets.Paths.LineAwesomeSolid)
            };

            // Load all data from JSON source files
            foreach (var entry in sourceDataPaths)
            {
                using (var sourceStream = AssetLoader.Open(new Uri(entry.Item3)))
                using (var reader = new StreamReader(sourceStream))
                {
                    string jsonString = reader.ReadToEnd();
                    var rawIcons = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                    if (rawIcons is not null)
                    {
                        foreach (var rawIcon in rawIcons)
                        {
                            var icon = new LineAwesomeIcon()
                            {
                                // IconSet is determined automatically from Style
                                Name         = rawIcon.Value,
                                Style        = entry.Item2,
                                UnicodePoint = Convert.ToUInt32(rawIcon.Key.Substring(2), 16) // Remove '0x'
                            };

                            icons.Add(icon);

                            if (icon.Style == IconStyle.Brand)
                            {
                                brandNames.Add(icon.UnicodePoint, icon.Name);
                            }
                            else if (icon.Style == IconStyle.Solid)
                            {
                                solidNames.Add(icon.UnicodePoint, icon.Name);
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
                _cachedBrandNames   = brandNames;
                _cachedRegularNames = regularNames;
                _cachedSolidNames   = solidNames;
            }

            return;
        }

        public static string FindName(uint unicodePoint, IconStyle style)
        {
            string? name = null;

            lock (_cacheLock)
            {
                if (_cachedBrandNames is null ||
                    _cachedRegularNames is null ||
                    _cachedSolidNames is null)
                {
                    RebuildCache();
                }

                if (style == IconStyle.Brand)
                {
                    _cachedBrandNames!.TryGetValue(unicodePoint, out name);
                }
                else if (style == IconStyle.Solid)
                {
                    _cachedSolidNames!.TryGetValue(unicodePoint, out name);
                }
                else
                {
                    _cachedRegularNames!.TryGetValue(unicodePoint, out name);
                }
            }

            return name ?? string.Empty;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Line Awesome icon set family.
        /// This includes ALL styles: brand, solid and regular.
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
        /// Gets all icons of the defined <see cref="IconStyle"/>.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> GetIcons(IconStyle style)
        {
            var matchingIcons = new List<LineAwesomeIcon>();

            lock (_cacheLock)
            {
                if (_cachedIcons is null)
                {
                    RebuildCache();
                }

                foreach (LineAwesomeIcon icon in _cachedIcons!)
                {
                    if (icon.Style == style)
                    {
                        matchingIcons.Add(icon);
                    }
                }
            }

            return matchingIcons.AsReadOnly();
        }

        /// <summary>
        /// Builds the full list of glyph sources for Line Awesome icons.
        /// Source files must already be downloaded and present in the cache.
        /// </summary>
        /// <remarks>
        /// This method is NOT intended for general-purpose use.
        /// It should only be used by those who know what they are doing to re-build the glyph sources.
        /// </remarks>
        public static void BuildRemoteGlyphSourcePaths()
        {
            List<string> glyphSources = new List<string>();
            string lineAwesomeCacheDirectory = @"line-awesome";

            string searchDirectory = Path.Combine(App.IconManagerCache, lineAwesomeCacheDirectory, "svg");

            foreach (string filePath in Directory.EnumerateFiles(searchDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(filePath).ToUpperInvariant() == ".SVG")
                {
                    glyphSources.Add(filePath.Replace(searchDirectory, string.Empty));
                }
            }

            glyphSources.Sort();

            var jsonString = JsonSerializer.Serialize(
                glyphSources.ToArray(),
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                });

            using (var fileStream = File.OpenWrite(Path.Combine(App.IconManagerCache, "LineAwesomeGlyphSources.json")))
            {
                fileStream.Write(Encoding.UTF8.GetBytes(jsonString));
            }

            return;
        }
    }
}
