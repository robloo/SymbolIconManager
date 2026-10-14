using Avalonia.Platform;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using IconManager.IconSets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IconManager.Models
{
    /// <summary>
    /// Contains methods to retrieve a glyph source stream or URL in various formats.
    /// </summary>
    public static class GlyphProvider
    {
        private static Dictionary<string, SKFont> _cachedFonts = new Dictionary<string, SKFont>();  // IconSet/file name is key

        private static Lock _cacheLock = new();

        /// <summary>
        /// Gets all possible glyph sources for the given icon set and Unicode point.
        /// Note that 'possible' here means it may exist, but is not guaranteed.
        /// Use an associated GetGlyphSource method to confirm.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The list of possible glyph sources.</returns>
        public static List<GlyphSource> GetPossibleGlyphSources(
            IconSet iconSet,
            uint unicodePoint)
        {
            var possibleSources = new List<GlyphSource>();

            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                    return FluentUISystemGlyphs.Instance.GetGlyphSources(iconSet, unicodePoint);
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                    return LineAwesomeGlyphs.Instance.GetGlyphSources(iconSet, unicodePoint);
                case IconSet.SegoeFluent:
                case IconSet.SegoeMDL2Assets:
                case IconSet.SegoeUISymbol:
                    possibleSources.Add(GlyphSource.RemotePngFile);
                    break;
                case IconSet.WinJSSymbols:
                    possibleSources.Add(GlyphSource.LocalFontFile);
                    break;
            }

            return possibleSources;
        }

        /// <summary>
        /// Gets the font data source URI of the defined icon set.
        /// </summary>
        /// <param name="iconSet">The icon set to get the font URI for.</param>
        /// <returns>The URI for the icon set's font data source.</returns>
        public static Uri? GetFontSourceUri(IconSet iconSet)
        {
            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                    return FluentUISystemGlyphs.Instance.GetFontSourceUri(iconSet);
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                    return LineAwesomeGlyphs.Instance.GetFontSourceUri(iconSet);
                case IconSet.WinJSSymbols:
                    return new Uri(IconSets.Paths.WinJSSymbolsFont);
            }

            return null;
        }

        /// <summary>
        /// Loads the <see cref="SKFont"/> file that matches the given name.
        /// The <paramref name="fontNameKey"/> is commonly an <see cref="IconSet"/> but may also be
        /// the actual .ttf file name.
        /// </summary>
        /// <param name="fontNameKey">The name of the font to return.</param>
        /// <param name="renderWidth">The glyph render size in pixels for the typeface.</param>
        /// <returns>The loaded <see cref="SKFont"/>; otherwise, null.</returns>
        public static SKFont? LoadFont(
            string fontNameKey,
            uint renderWidth = GlyphRenderer.RenderWidth)
        {
            string fontKey = Path.GetFileName(fontNameKey).Trim();
            Uri? fontUri = null;
            SKFont? font = null;
            var fontDirectories = new string[]
            {
                "Fonts",
                @"Source\IconManager.IconSets\FluentUISystem\DataSources",
                @"Source\IconManager.IconSets\LineAwesome\DataSources",
                @"Source\IconManager.IconSets\WinJSSymbols\DataSources"
            };

            lock (_cacheLock)
            {
                if (_cachedFonts.TryGetValue(fontKey, out font) == false)
                {
                    // Handle IconSet names first
                    if (Enum.TryParse(typeof(IconSet), fontKey, out object? parsedIconSet) &&
                        parsedIconSet is IconSet iconSet)
                    {
                        // Only some IconSet's have available font files
                        fontUri = GlyphProvider.GetFontSourceUri(iconSet);
                    }

                    // Search directories for a font by name
                    if (fontUri is null)
                    {
                        // The app should be running in debug/release mode within the bin directory
                        // Locate this folder as the root
                        string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
                        DirectoryInfo dir = new DirectoryInfo(dirPath);

                        while (dir.Exists &&
                               string.Equals(dir.Name, "SymbolIconManager", StringComparison.OrdinalIgnoreCase) == false)
                        {
                            dir = Directory.GetParent(dir.FullName) ?? new DirectoryInfo("");
                        }

                        var searchDirectories = new string[fontDirectories.Length];
                        for (int i = 0; i < fontDirectories.Length; i++)
                        {
                            searchDirectories[i] = Path.Combine(dir.FullName, fontDirectories[i]);
                        }

                        foreach (var searchDirectory in searchDirectories)
                        {
                            string[] files = Directory.GetFiles(
                                searchDirectory,
                                "*",
                                SearchOption.AllDirectories);

                            foreach (string filePath in files)
                            {
                                if (string.Equals(Path.GetFileName(filePath), fontNameKey, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileNameWithoutExtension(filePath), fontNameKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    fontUri = new Uri(filePath);
                                    break;
                                }
                            }

                            if (fontUri is not null)
                            {
                                break;
                            }
                        }
                    }

                    if (fontUri is not null)
                    {
                        // Load the SKFont (and internally the SKTypeface)
                        if (string.Equals(fontUri.Scheme, "avares", StringComparison.OrdinalIgnoreCase))
                        {
                            // Load from Avalonia assets
                            using (var sourceStream = AssetLoader.Open(fontUri))
                            {
                                var typeface = SKTypeface.FromStream(sourceStream);
                                font = new SKFont()
                                {
                                    Typeface = typeface,
                                    Size     = renderWidth // In pixels not points
                                };

                                _cachedFonts.Add(fontKey, font);
                                return font;
                            }
                        }
                        else
                        {
                            // Load from the file system
                            using (var fileStream = File.OpenRead(fontUri.AbsolutePath))
                            {
                                var typeface = SKTypeface.FromStream(fileStream);
                                font = new SKFont()
                                {
                                    Typeface = typeface,
                                    Size     = renderWidth // In pixels not points
                                };

                                _cachedFonts.Add(fontKey, font);
                                return font;
                            }
                        }
                    }
                }
            }

            return font;
        }

        /// <summary>
        /// Gets the local image data source URI of the defined glyph.
        /// This currently only supports SVG format.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The local URI of the glyph's image data source.</returns>
        public static Uri? GetLocalGlyphSourceUri(
            IconSet iconSet,
            uint unicodePoint)
        {
            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                {
                    return FluentUISystemGlyphs.Instance.GetGlyphSourceUri(
                        iconSet,
                        unicodePoint,
                        GlyphSource.LocalSvgFile);
                }
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                {
                    return LineAwesomeGlyphs.Instance.GetGlyphSourceUri(
                        iconSet,
                        unicodePoint,
                        GlyphSource.LocalSvgFile);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the remote image data source URI of the defined glyph.
        /// This may be an SVG or bitmap.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The remote URI of the glyph's image data source.</returns>
        public static Uri? GetRemoteGlyphSourceUri(
            IconSet iconSet,
            uint unicodePoint)
        {
            var possibleGlyphSources = GlyphProvider.GetPossibleGlyphSources(iconSet, unicodePoint);
            if (possibleGlyphSources.Contains(GlyphSource.RemotePngFile) == false &&
                possibleGlyphSources.Contains(GlyphSource.RemoteSvgFile) == false)
            {
                return null;
            }

            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                {
                    return FluentUISystemGlyphs.Instance.GetGlyphSourceUri(
                        iconSet,
                        unicodePoint,
                        GlyphSource.RemoteSvgFile);
                }
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                {
                    return LineAwesomeGlyphs.Instance.GetGlyphSourceUri(
                        iconSet,
                        unicodePoint,
                        GlyphSource.RemoteSvgFile);
                }
                case IconSet.SegoeFluent:
                {
                    string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/";
                    return new Uri(baseUrl + Icon.ToUnicodeHexString(unicodePoint) + ".png");
                }
                case IconSet.SegoeMDL2Assets:
                {
                    string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/";
                    return new Uri(baseUrl + Icon.ToUnicodeHexString(unicodePoint) + ".png");
                }
                case IconSet.SegoeUISymbol:
                {
                    string name = SegoeUISymbol.FindName(unicodePoint).ToLowerInvariant();

                    // Naming does not match 1-to-1 with the URL and there are some special cases
                    // Special cases are defined here, separately
                    switch (name)
                    {
                        case "account":
                            name = "accounts";
                            break;
                        case "character":
                            name = "characters";
                            break;
                        case "closedcaption":
                            name = "cc";
                            break;
                        case "mailfilled":
                            name = "mail2";
                            break;
                        case "page":
                            name = "pageicon";
                            break;
                        case "setting":
                            name = "settings";
                            break;
                    }

                    return new Uri(@"https://docs.microsoft.com/en-us/previous-versions/windows/apps/images/jj841127." + name + @"(en-us,win.10).png");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the image data stream (usually PNG or SVG format) of the defined glyph.
        /// It is up to the caller to ensure the data stream is a supported format.
        /// This will be downloaded from an online source.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The glyph source image data stream.</returns>
        public static async Task<MemoryStream?> GetGlyphSourceStreamAsync(
            IconSet iconSet,
            uint unicodePoint)
        {
            Uri? glyphUri = null;
            var possibleGlyphSources = GlyphProvider.GetPossibleGlyphSources(iconSet, unicodePoint);

            // Always prioritize any local glyph sources
            if (glyphUri is null &&
                possibleGlyphSources.Contains(GlyphSource.LocalSvgFile))
            {
                glyphUri = GlyphProvider.GetLocalGlyphSourceUri(iconSet, unicodePoint);
            }

            if (glyphUri is null &&
                possibleGlyphSources.Contains(GlyphSource.RemotePngFile) ||
                possibleGlyphSources.Contains(GlyphSource.RemoteSvgFile))
            {
                glyphUri = GlyphProvider.GetRemoteGlyphSourceUri(iconSet, unicodePoint);
            }

            if (glyphUri is not null)
            {
                return await GlyphProvider.GetGlyphSourceStreamAsync(glyphUri!);
            }

            return null;
        }

        /// <summary>
        /// Gets the image data stream (usually PNG or SVG format) of the glyph image file at the given URL.
        /// It is up to the caller to ensure the data stream is a supported format.
        /// This will be downloaded from an online source.
        /// </summary>
        /// <param name="uri">The URL of the glyph source image file to download.</param>
        /// <returns>The glyph source image data stream.</returns>
        public static async Task<MemoryStream?> GetGlyphSourceStreamAsync(Uri uri)
        {
            try
            {
                if (uri is not null)
                {
                    // Note: LocalPath MUST be used (AbsolutePath doesn't work in all cases)
                    if (uri.IsFile &&
                        File.Exists(uri.LocalPath))
                    {
                        using (FileStream fileStream = File.OpenRead(uri.LocalPath))
                        {
                            var memoryStream = new MemoryStream(); 
                            fileStream.CopyTo(memoryStream);

                            return memoryStream;
                        }
                    }
                    else
                    {
                        using (var client = new HttpClient())
                        {
                            var data = await client.GetByteArrayAsync(uri);
                            return new MemoryStream(data);
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
