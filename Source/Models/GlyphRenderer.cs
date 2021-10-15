using Avalonia;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace IconManager
{
    /// <summary>
    /// Contains methods to retrieve images and details about a glyph.
    /// </summary>
    /// <remarks>
    /// Not that some functionality could be separated out in a future GlyphProvider.
    /// The GlyphProvider would retrieve raw glyph data, the GlyphRenderer would draw it.
    /// </remarks>
    public static class GlyphRenderer
    {
        public const int RenderWidth  = 50; // Pixels
        public const int RenderHeight = 50; // Pixels

        private static SKPaint? cachedBackgroundPaint = null;

        private static Dictionary<string, Bitmap>   cachedGlyphs     = new Dictionary<string, Bitmap>(); // IconSet_UnicodePoint is key
        private static Dictionary<IconSet, SKFont>  cachedFonts      = new Dictionary<IconSet, SKFont>(); // IconSet is key
        private static Dictionary<IconSet, SKPaint> cachedTextPaints = new Dictionary<IconSet, SKPaint>(); // IconSet is key

        private static List<string>? cachedFluentUISystemGlyphSources = null;
        private static List<string>? cachedLineAwesomeGlyphSources    = null;

        private static object cacheMutex             = new object();
        private static object glyphSourcesCacheMutex = new object();

        /// <summary>
        /// Gets a preview bitmap of the glyph for the defined icon set and Unicode point.
        /// </summary>
        /// <param name="iconSet">The icon set containing the Unicode point.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>A 50px-by-50px preview bitmap of the glyph.</returns>
        public static async Task<Bitmap?> GetBitmapAsync(IconSet iconSet, uint unicodePoint)
        {
            string iconSetKey = iconSet.ToString();
            string glyphKey = iconSetKey + "_" + Icon.ToUnicodeHexString(unicodePoint);
            Bitmap? result = null;

            // Quickly return for unsupported icon sets
            if (iconSet == IconSet.Undefined)
            {
                return null;
            }

            // Attempt to quickly load and return the preview from the cache
            lock (cacheMutex)
            {
                if (cachedGlyphs.TryGetValue(glyphKey, out result))
                {
                    return result;
                }
            }

            if (iconSet == IconSet.FluentUISystemFilled ||
                iconSet == IconSet.FluentUISystemRegular ||
                iconSet == IconSet.LineAwesomeBrand ||
                iconSet == IconSet.LineAwesomeRegular ||
                iconSet == IconSet.LineAwesomeSolid ||
                iconSet == IconSet.WinJSSymbols)
            {
                result = await Task.Run<Bitmap?>(async () =>
                {
                    // These icon sets have an embedded font file within the application
                    // Rendering glyphs is fastest using the embedded font itself
                    // Note that FluentSystemIcons fonts have a bug and any glyph over 0xFFFF simply does not exist
                    // See: https://github.com/microsoft/fluentui-system-icons/issues/299
                    // A work-around for this case is required using the online SVG file sources

                    var textBounds = new SKRect();
                    bool glyphExistsInFont = true;
                    SKFont? textFont = null;
                    SKPaint? textPaint = null;
                    SKBitmap bitmap = new SKBitmap(
                        GlyphRenderer.RenderWidth,
                        GlyphRenderer.RenderHeight);

                    lock (cacheMutex)
                    {
                        // Load the SKFont (and internally the SKTypeface)
                        if (cachedFonts.TryGetValue(iconSet, out textFont) == false)
                        {
                            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                            Uri? fontPath = GlyphRenderer.GetFontSourceUri(iconSet);

                            using (var sourceStream = assets.Open(fontPath))
                            {
                                var typeface = SKTypeface.FromStream(sourceStream);
                                textFont = new SKFont()
                                {
                                    Typeface = typeface,
                                    Size     = GlyphRenderer.RenderWidth // In pixels not points
                                };

                                cachedFonts.Add(iconSet, textFont);
                            }
                        }

                        // Load all SKPaint objects
                        if (cachedBackgroundPaint == null)
                        {
                            cachedBackgroundPaint = new SKPaint()
                            {
                                Color = SKColors.White
                            };
                        }

                        if (cachedTextPaints.TryGetValue(iconSet, out textPaint) == false)
                        {
                            textPaint = new SKPaint(textFont)
                            {
                                Color = SKColors.Black
                            };

                            cachedTextPaints.Add(iconSet, textPaint);
                        }

                        // Measure the rendered text
                        // This also checks if the glyph exists in the font before continuing
                        string text = char.ConvertFromUtf32((int)unicodePoint).ToString();
                        textPaint.MeasureText(text, ref textBounds);

                        if (textBounds.Width == 0 ||
                            textBounds.Height == 0)
                        {
                            glyphExistsInFont = false;
                        }

                        // Render the glyph using SkiaSharp
                        if (glyphExistsInFont)
                        {
                            using (SKCanvas canvas = new SKCanvas(bitmap))
                            {
                                canvas.DrawRect(
                                    x: 0,
                                    y: 0,
                                    w: GlyphRenderer.RenderWidth,
                                    h: GlyphRenderer.RenderHeight,
                                    cachedBackgroundPaint);

                                canvas.DrawText(
                                    text,
                                    // No need to consider baseline, just center the glyph
                                    x: (GlyphRenderer.RenderWidth / 2f) - textBounds.MidX,
                                    y: (GlyphRenderer.RenderHeight / 2f) - textBounds.MidY,
                                    textFont,
                                    textPaint);
                            }
                        }
                    }

                    if (glyphExistsInFont)
                    {
                        // Use the Skia font-rendered glyph
                        // Note that the default SKImage encoding format is .png
                        using (SKImage image = SKImage.FromBitmap(bitmap))
                        using (SKData encoded = image.Encode())
                        using (Stream stream = encoded.AsStream())
                        {
                            return new Bitmap(stream);
                        }
                    }
                    else
                    {
                        // Attempt to use an SVG image as fallback
                        // Note that the icon set determines that the format will be SVG
                        var svgStream = await GlyphRenderer.GetGlyphSourceStreamAsync(iconSet, unicodePoint);

                        if (svgStream != null)
                        {
                            // The size here (and above) isn't taking into account device DPI
                            // It probably should in the future
                            // Also note that no background is added to the rendered SVG unlike above
                            var svg = new SkiaSharp.Extended.Svg.SKSvg(
                                new SKSize(GlyphRenderer.RenderWidth, GlyphRenderer.RenderHeight));
                            svg.Load(svgStream);

                            // Note that the default SKImage encoding format is .png
                            using (SKImage image = SKImage.FromPicture(svg.Picture, svg.CanvasSize.ToSizeI()))
                            using (SKData encoded = image.Encode())
                            using (Stream stream = encoded.AsStream())
                            {
                                return new Bitmap(stream);
                            }
                        }

                        return null;
                    }
                });
            }
            else if (iconSet == IconSet.SegoeFluent ||
                     iconSet == IconSet.SegoeMDL2Assets ||
                     iconSet == IconSet.SegoeUISymbol)
            {
                // These icon sets are Microsoft proprietary and the fonts cannot be used on non-Windows systems
                // Therefore, the font is not packaged with the application and there is no guarantee it will be on the system either
                // To work around this, preview images are loaded from Microsoft's website on-demand
                // These preview images are also never distributed with source code
                // Also note that SegoeUISymbol is also proprietary but has no website to retrieve preview images from

                using (Stream? imageStream = await GlyphRenderer.GetGlyphSourceStreamAsync(iconSet, unicodePoint))
                {
                    return (imageStream == null ? null : new Bitmap(imageStream));
                }
            }

            // Add any new bitmap to the cache for next time
            // In order to get this far a new glyph bitmap was generated (or at least an attempt was made)
            // It is possible that two renderers for the same glyph are running simultaneously on differing threads
            // Therefore, within the lock, a check must be made to ensure a glyph was not already added
            if (result != null)
            {
                lock (cacheMutex)
                {
                    if (cachedGlyphs.ContainsKey(glyphKey) == false)
                    {
                        cachedGlyphs.Add(glyphKey, result);
                    }
                }
            }

            return result;
        }

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
                            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
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

                    lock (glyphSourcesCacheMutex)
                    {
                        if (cachedLineAwesomeGlyphSources == null)
                        {
                            // Rebuild the cache
                            var sources = new List<string>();
                            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
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
            Uri? glyphUrl = GlyphRenderer.GetGlyphSourceUrl(iconSet, unicodePoint);

            if (glyphUrl != null)
            {
                return await GlyphRenderer.GetGlyphSourceStreamAsync(glyphUrl!);
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
