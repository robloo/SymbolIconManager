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
            string glyphKey = iconSetKey + "_" + Icon.ToUnicodeString(unicodePoint);
            Bitmap? result = null;

            // Quickly return for unsupported icon sets
            if (iconSet == IconSet.Undefined ||
                iconSet == IconSet.SegoeUISymbol)
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
                            string fontPath = string.Empty;

                            switch (iconSet)
                            {
                                case IconSet.FluentUISystemFilled:
                                    fontPath = "avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Filled.ttf";
                                    break;
                                case IconSet.FluentUISystemRegular:
                                    fontPath = "avares://IconManager/Data/FluentUISystem/FluentSystemIcons-Regular.ttf";
                                    break;
                                case IconSet.WinJSSymbols:
                                    fontPath = "avares://IconManager/Data/WinJSSymbols/Symbols.ttf";
                                    break;
                            }

                            using (var sourceStream = assets.Open(new Uri(fontPath)))
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
                     iconSet == IconSet.SegoeMDL2Assets)
            {
                // These icon sets are Microsoft proprietary and the fonts cannot be used on non-Windows systems
                // Therefore, the font is not packaged with the application and there is no guarantee it will be on the system either
                // To work around this, preview images are loaded from Microsoft's website on-demand
                // These preview images are also never distributed with source code
                // Also note that SegoeUISymbol is also proprietary but has no website to retrieve preview images from

                using (Stream? imageStream = await GlyphRenderer.GetGlyphSourceStreamAsync(iconSet, unicodePoint))
                {
                    return new Bitmap(imageStream);
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
                case IconSet.SegoeFluent:
                case IconSet.SegoeMDL2Assets:
                    possibleSources.Add(GlyphSource.RemotePngFile);
                    break;
                case IconSet.SegoeUISymbol:
                    // Not available anywhere
                    break;
                case IconSet.WinJSSymbols:
                    possibleSources.Add(GlyphSource.LocalFontFile);
                    break;
            }

            return possibleSources;
        }

        /// <summary>
        /// Gets the image data source URL of the defined glyph.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        public static Uri? GetGlyphSourceUrl(
            IconSet iconSet,
            uint unicodePoint)
        {
            string relativeGlyphUrl = string.Empty;
            List<string>? relativeGlyphUrls = null;

            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                {
                    string svgName = string.Empty;

                    // Determine the SVG file name
                    if (string.IsNullOrWhiteSpace(svgName))
                    {
                        switch (iconSet)
                        {
                            case IconSet.FluentUISystemFilled:
                                svgName = FluentUISystem.FindName(unicodePoint, FluentUISystem.IconTheme.Filled);
                                break;
                            case IconSet.FluentUISystemRegular:
                                svgName = FluentUISystem.FindName(unicodePoint, FluentUISystem.IconTheme.Regular);
                                break;
                        }

                        svgName = $@"{svgName}.svg";
                    }

                    if (string.IsNullOrWhiteSpace(svgName))
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

                        relativeGlyphUrls = cachedFluentUISystemGlyphSources!.FindAll(s => s.EndsWith(svgName));
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
                case IconSet.SegoeFluent:
                {
                    string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/";
                    return new Uri(baseUrl + Icon.ToUnicodeString(unicodePoint) + ".png");
                }
                case IconSet.SegoeMDL2Assets:
                {
                    string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/";
                    return new Uri(baseUrl + Icon.ToUnicodeString(unicodePoint) + ".png");
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
