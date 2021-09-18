using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

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

        private static IReadOnlyList<Icon>? cachedIcons = null;
        private static IReadOnlyDictionary<uint, string>? cachedBrandNames   = null;
        private static IReadOnlyDictionary<uint, string>? cachedRegularNames = null;
        private static IReadOnlyDictionary<uint, string>? cachedSolidNames   = null;

        private static object cacheMutex = new object();

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<Icon>();
            var brandNames = new Dictionary<uint, string>();
            var regularNames = new Dictionary<uint, string>();
            var solidNames = new Dictionary<uint, string>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var sourceDataPaths = new Tuple<IconSet, IconStyle, string>[]
            {
                Tuple.Create(
                    IconSet.LineAwesomeBrand,
                    IconStyle.Brand,
                    "avares://IconManager/Data/LineAwesome/la-brands-400.json"),
                Tuple.Create(
                    IconSet.LineAwesomeRegular,
                    IconStyle.Regular,
                    "avares://IconManager/Data/LineAwesome/la-regular-400.json"),
                Tuple.Create(
                    IconSet.LineAwesomeSolid,
                    IconStyle.Solid,
                    "avares://IconManager/Data/LineAwesome/la-solid-900.json")
            };

            // Load all data from JSON source files
            foreach (var entry in sourceDataPaths)
            {
                using (var sourceStream = assets.Open(new Uri(entry.Item3)))
                using (var reader = new StreamReader(sourceStream))
                {
                    string jsonString = reader.ReadToEnd();
                    var rawIcons = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                    if (rawIcons != null)
                    {
                        foreach (var rawIcon in rawIcons)
                        {
                            var icon = new Icon()
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

            lock (cacheMutex)
            {
                cachedIcons        = icons.AsReadOnly();
                cachedBrandNames   = brandNames;
                cachedRegularNames = regularNames;
                cachedSolidNames   = solidNames;
            }

            return;
        }

        public static string FindName(uint unicodePoint, IconStyle style)
        {
            string? name = null;

            lock (cacheMutex)
            {
                if (cachedBrandNames == null ||
                    cachedRegularNames == null ||
                    cachedSolidNames == null)
                {
                    RebuildCache();
                }

                if (style == IconStyle.Brand)
                {
                    cachedBrandNames!.TryGetValue(unicodePoint, out name);
                }
                else if (style == IconStyle.Solid)
                {
                    cachedSolidNames!.TryGetValue(unicodePoint, out name);
                }
                else
                {
                    cachedRegularNames!.TryGetValue(unicodePoint, out name);
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
        /// Gets all icons of the defined <see cref="IconStyle"/>.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> GetIcons(IconStyle style)
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
        public static void BuildGlyphSources()
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

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in the Line Awesome icon set.
        /// </summary>
        public class Icon : IIcon
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
                    if (this.Style == IconStyle.Brand)
                    {
                        return IconSet.LineAwesomeBrand;
                    }
                    else if (this.Style == IconStyle.Solid)
                    {
                        return IconSet.LineAwesomeSolid;
                    }
                    else
                    {
                        return IconSet.LineAwesomeRegular;
                    }
                }
                set { /* Do nothing */ }
            }

            /// <inheritdoc/>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Line Awesome style of the icon.
            /// </summary>
            public IconStyle Style { get; set; } = IconStyle.Regular;

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
                    Name         = this.Name,
                    Style        = this.Style,
                    UnicodePoint = this.UnicodePoint
                };

                return clone;
            }

            /// <summary>
            /// Converts this <see cref="LineAwesome.Icon"/> into a standard <see cref="IconManager.Icon"/>.
            /// This is sometimes needed because <see cref="LineAwesome.Icon"/> does not derive from
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
    }
}
