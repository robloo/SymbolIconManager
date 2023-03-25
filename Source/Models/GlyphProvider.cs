using Avalonia;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace IconManager.Models
{
    /// <summary>
    /// Contains methods to retrieve a glyph source stream or URL in various formats.
    /// </summary>
    public static class GlyphProvider
    {
        private static Dictionary<string, SKFont> cachedFonts = new Dictionary<string, SKFont>();  // IconSet/file name is key

        private static List<string>? cachedFluentUISystemGlyphSources = null;
        private static List<string>? cachedLineAwesomeGlyphSources    = null;

        private static object cacheMutex             = new object();
        private static object glyphSourcesCacheMutex = new object();

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
                    possibleSources.Add(GlyphSource.LocalFontFile);
                    possibleSources.Add(GlyphSource.RemoteSvgFile);
                    break;
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                    possibleSources.Add(GlyphSource.LocalFontFile);
                    possibleSources.Add(GlyphSource.RemoteSvgFile);
                    break;
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
                    return new Uri("avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Filled.ttf");
                case IconSet.FluentUISystemRegular:
                    return new Uri("avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Regular.ttf");
                case IconSet.LineAwesomeBrand:
                    return new Uri("avares://IconManager/Data/LineAwesome/la-brands-400.ttf");
                case IconSet.LineAwesomeRegular:
                    return new Uri("avares://IconManager/Data/LineAwesome/la-regular-400.ttf");
                case IconSet.LineAwesomeSolid:
                    return new Uri("avares://IconManager/Data/LineAwesome/la-solid-900.ttf");
                case IconSet.WinJSSymbols:
                    return new Uri("avares://IconManager/Data/WinJSSymbols/Symbols.ttf");
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
                "Source\\Data"
            };

            lock (cacheMutex)
            {
                if (cachedFonts.TryGetValue(fontKey, out font) == false)
                {
                    // Handle IconSet names first
                    if (Enum.TryParse(typeof(IconSet), fontKey, out object? parsedIconSet) &&
                        parsedIconSet is IconSet iconSet)
                    {
                        // Only the following IconSet's have available font files
                        if (iconSet == IconSet.FluentUISystemFilled ||
                            iconSet == IconSet.FluentUISystemRegular ||
                            iconSet == IconSet.LineAwesomeBrand ||
                            iconSet == IconSet.LineAwesomeRegular ||
                            iconSet == IconSet.LineAwesomeSolid ||
                            iconSet == IconSet.WinJSSymbols)
                        {
                            fontUri = GlyphProvider.GetFontSourceUri(iconSet);
                        }
                    }

                    // Search directories for a font by name
                    if (fontUri == null)
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

                            if (fontUri != null)
                            {
                                break;
                            }
                        }
                    }

                    if (fontUri != null)
                    {
                        // Load the SKFont (and internally the SKTypeface)
                        if (string.Equals(fontUri.Scheme, "avares", StringComparison.OrdinalIgnoreCase))
                        {
                            // Load from Avalonia assets
                            var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
                            using (var sourceStream = assets.Open(fontUri))
                            {
                                var typeface = SKTypeface.FromStream(sourceStream);
                                font = new SKFont()
                                {
                                    Typeface = typeface,
                                    Size     = renderWidth // In pixels not points
                                };

                                cachedFonts.Add(fontKey, font);
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

                                cachedFonts.Add(fontKey, font);
                                return font;
                            }
                        }
                    }
                }
            }

            return font;
        }

        /// <summary>
        /// Gets the image data source URL of the defined glyph.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The URL of the glyph's image data source.</returns>
        public static Uri? GetGlyphSourceUrl(
            IconSet iconSet,
            uint unicodePoint)
        {
            string nameBase;
            string relativeGlyphUrl = string.Empty;
            List<string>? relativeGlyphUrls = null;

            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                {
                    nameBase = IconSetBase.FindName(iconSet, unicodePoint);

                    if (string.IsNullOrWhiteSpace(nameBase))
                    {
                        return null;
                    }

                    lock (glyphSourcesCacheMutex)
                    {
                        if (cachedFluentUISystemGlyphSources == null)
                        {
                            // Rebuild the cache
                            var sources = new List<string>();
                            var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
                            string sourceDataPath = "avares://IconManager/Data/FluentUISystem/FluentUISystemGlyphSources.json";

                            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
                            using (var reader = new StreamReader(sourceStream))
                            {
                                string jsonString = reader.ReadToEnd();
                                var rawGlyphSources = JsonSerializer.Deserialize<string[]>(jsonString);

                                if (rawGlyphSources != null)
                                {
                                    foreach (var entry in rawGlyphSources)
                                    {
                                        sources.Add(entry);
                                    }
                                }
                            }

                            cachedFluentUISystemGlyphSources = sources;
                        }

                        relativeGlyphUrls = cachedFluentUISystemGlyphSources!.FindAll(s => s.EndsWith($@"{nameBase}.svg"));
                    }

                    // Use the relativeGlyphUrl with the smallest directory structure
                    // There are sometimes many variants with the exact same file name -- some for other cultures
                    // Each culture is usually placed in it's own folder
                    // We want the invariant culture (smallest directory structure), as best as possible
                    if (relativeGlyphUrls != null &&
                        relativeGlyphUrls.Count > 0)
                    {
                        relativeGlyphUrl = relativeGlyphUrls[0];

                        for (int i = 1; i < relativeGlyphUrls.Count; i++)
                        {
                            // Not the most efficient to keep splitting, but it's easiest
                            if (relativeGlyphUrls[i].Split('\\').Length < relativeGlyphUrl.Split('\\').Length)
                            {
                                relativeGlyphUrl = relativeGlyphUrls[i];
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(relativeGlyphUrl) == false)
                    {
                        try
                        {
                            // Note that the relative URL determined above starts with '/'
                            string baseUrl = @"https://raw.githubusercontent.com/microsoft/fluentui-system-icons/master/assets";
                            return new Uri($@"{baseUrl}{relativeGlyphUrl}");
                        }
                        catch { }
                    }

                    break;
                }
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                {
                    nameBase = IconSetBase.FindName(iconSet, unicodePoint);

                    if (string.IsNullOrWhiteSpace(nameBase))
                    {
                        return null;
                    }

                    // Naming does not match 1-to-1 with the URL and there are some special cases
                    // Special cases are defined here, separately
                    switch (nameBase)
                    {
                        case "alternate-square-root":
                            nameBase = "square-root-alt-solid";
                            break;
                        case "at":
                            nameBase = "at-solid";
                            break;
                        case "i-beam-cursor":
                            nameBase = "i-cursor-solid";
                            break;
                        case "lightning-bolt":
                            nameBase = "bolt-solid";
                            break;
                    }

                    lock (glyphSourcesCacheMutex)
                    {
                        if (cachedLineAwesomeGlyphSources == null)
                        {
                            // Rebuild the cache
                            var sources = new List<string>();
                            var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
                            string sourceDataPath = "avares://IconManager/Data/LineAwesome/LineAwesomeGlyphSources.json";

                            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
                            using (var reader = new StreamReader(sourceStream))
                            {
                                string jsonString = reader.ReadToEnd();
                                var rawGlyphSources = JsonSerializer.Deserialize<string[]>(jsonString);

                                if (rawGlyphSources != null)
                                {
                                    foreach (var entry in rawGlyphSources)
                                    {
                                        sources.Add(entry);
                                    }
                                }
                            }

                            cachedLineAwesomeGlyphSources = sources;
                        }

                        relativeGlyphUrls = cachedLineAwesomeGlyphSources!.FindAll(s => s.EndsWith($@"{nameBase}.svg"));

                        if (relativeGlyphUrls.Count == 0)
                        {
                            // Only SVG sources are available for Line Awesome so the search can remove the extension
                            // This is necessary for the Line Awesome font family because exact names are not enforced
                            // There is often some slight variation such as 'unlink' name with 'unlink-solid.svg' file
                            // In this example some file names have the style added to the end
                            relativeGlyphUrls = cachedLineAwesomeGlyphSources!.FindAll(s => s.Contains(nameBase));
                        }
                    }

                    // Use the relativeGlyphUrl with the smallest directory structure
                    // There are sometimes many variants with the exact same file name -- some for other cultures
                    // Each culture is usually placed in it's own folder
                    // We want the invariant culture (smallest directory structure), as best as possible
                    if (relativeGlyphUrls != null &&
                        relativeGlyphUrls.Count > 0)
                    {
                        relativeGlyphUrl = relativeGlyphUrls[0];

                        for (int i = 1; i < relativeGlyphUrls.Count; i++)
                        {
                            // Not the most efficient to keep splitting, but it's easiest
                            if (relativeGlyphUrls[i].Split('\\').Length < relativeGlyphUrl.Split('\\').Length)
                            {
                                relativeGlyphUrl = relativeGlyphUrls[i];
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(relativeGlyphUrl) == false)
                    {
                        try
                        {
                            // Note that the relative URL determined above starts with '/'
                            string baseUrl = @"https://raw.githubusercontent.com/icons8/line-awesome/master/svg";
                            return new Uri($@"{baseUrl}{relativeGlyphUrl}");
                        }
                        catch { }
                    }

                    break;
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
            Uri? glyphUrl = GlyphProvider.GetGlyphSourceUrl(iconSet, unicodePoint);

            if (glyphUrl != null)
            {
                return await GlyphProvider.GetGlyphSourceStreamAsync(glyphUrl!);
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
            if (uri != null)
            {
                // Always creating a new WebClient is poor performance
                // This needs to be updated in the future
                using (WebClient client = new WebClient())
                {
                    var downloadTaskResult = new TaskCompletionSource<MemoryStream?>();

                    DownloadDataCompletedEventHandler? eventHandler = null;
                    eventHandler = (sender, e) =>
                    {
                        try
                        {
                            downloadTaskResult.SetResult(new MemoryStream(e.Result));
                        }
                        catch
                        {
                            // In the future, retries could be allowed
                            downloadTaskResult.SetResult(null);
                        }
                        finally
                        {
                            client.DownloadDataCompleted -= eventHandler;
                        }
                    };

                    try
                    {
                        client.DownloadDataCompleted += eventHandler;
                        client.DownloadDataAsync(uri);

                        return await downloadTaskResult.Task;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}
