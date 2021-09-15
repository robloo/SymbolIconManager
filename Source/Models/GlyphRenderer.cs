using Avalonia;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace IconManager
{
    public static class GlyphRenderer
    {
        public const int RenderWidth  = 50; // Pixels
        public const int RenderHeight = 50; // Pixels

        private static SKPaint? cachedBackgroundPaint = null;

        private static Dictionary<string, Bitmap>   cachedGlyphs     = new Dictionary<string, Bitmap>(); // IconSet_UnicodePoint is key
        private static Dictionary<IconSet, SKFont>  cachedFonts      = new Dictionary<IconSet, SKFont>(); // IconSet is key
        private static Dictionary<IconSet, SKPaint> cachedTextPaints = new Dictionary<IconSet, SKPaint>(); // IconSet is key

        private static object cacheMutex = new object();

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
            lock (GlyphRenderer.cacheMutex)
            {
                if (GlyphRenderer.cachedGlyphs.TryGetValue(glyphKey, out result))
                {
                    return result;
                }
            }

            if (iconSet == IconSet.FluentUISystemFilled ||
                iconSet == IconSet.FluentUISystemRegular ||
                iconSet == IconSet.WinJSSymbols)
            {
                result = await Task.Run<Bitmap?>(() =>
                {
                    // These icon sets have an embedded font file within the application
                    // Rendering glyphs is fastest using the embedded font itself
                    // Note that FluentSystemIcons fonts have a bug and any glyph over 0xFFFF simply does not exist
                    // See: https://github.com/microsoft/fluentui-system-icons/issues/299
                    // A work-around for this case is required using the online SVG file sources

                    SKFont? textFont = null;
                    SKPaint? textPaint = null;
                    SKBitmap bitmap = new SKBitmap(
                        GlyphRenderer.RenderWidth,
                        GlyphRenderer.RenderHeight);

                    lock (GlyphRenderer.cacheMutex)
                    {
                        // Load the SKFont (and internally the SKTypeface)
                        if (GlyphRenderer.cachedFonts.TryGetValue(iconSet, out textFont) == false)
                        {
                            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                            string fontPath = string.Empty;

                            switch (iconSet)
                            {
                                case IconSet.FluentUISystemFilled:
                                    fontPath = "avares://IconManager/Data/FluentSystemIcons-Filled.ttf";
                                    break;
                                case IconSet.FluentUISystemRegular:
                                    fontPath = "avares://IconManager/Data/FluentSystemIcons-Regular.ttf";
                                    break;
                                case IconSet.WinJSSymbols:
                                    fontPath = "avares://IconManager/Data/Symbols.ttf";
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

                                GlyphRenderer.cachedFonts.Add(iconSet, textFont);
                            }
                        }

                        // Load all SKPaint objects
                        if (GlyphRenderer.cachedBackgroundPaint == null)
                        {
                            GlyphRenderer.cachedBackgroundPaint = new SKPaint()
                            {
                                Color = SKColors.White
                            };
                        }

                        if (GlyphRenderer.cachedTextPaints.TryGetValue(iconSet, out textPaint) == false)
                        {
                            textPaint = new SKPaint(textFont)
                            {
                                Color = SKColors.Black
                            };

                            GlyphRenderer.cachedTextPaints.Add(iconSet, textPaint);
                        }

                        // Render the glyph using SkiaSharp
                        using (SKCanvas canvas = new SKCanvas(bitmap))
                        {
                            string text = char.ConvertFromUtf32((int)unicodePoint).ToString();

                            var textBounds = new SKRect();
                            textPaint.MeasureText(text, ref textBounds);

                            canvas.DrawRect(
                                x: 0,
                                y: 0,
                                w: GlyphRenderer.RenderWidth,
                                h: GlyphRenderer.RenderHeight,
                                GlyphRenderer.cachedBackgroundPaint);

                            canvas.DrawText(
                                text,
                                // No need to consider baseline, just center the glyph
                                x: (GlyphRenderer.RenderWidth / 2f) - textBounds.MidX,
                                y: (GlyphRenderer.RenderHeight / 2f) - textBounds.MidY,
                                textFont,
                                textPaint);
                        }
                    }

                    // Note that the default SKImage encoding format is .png
                    using (SKImage image = SKImage.FromBitmap(bitmap))
                    using (SKData encoded = image.Encode())
                    using (Stream stream = encoded.AsStream())
                    {
                        return new Bitmap(stream);
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

                Uri? glyphUrl = null;

                // Determine the web URL to retrieve the image from
                switch (iconSet)
                {
                    case IconSet.SegoeFluent:
                        glyphUrl = new Uri(@"https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/" + Icon.ToUnicodeString(unicodePoint) + ".png");
                        break;
                    case IconSet.SegoeMDL2Assets:
                        glyphUrl = new Uri(@"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/" + Icon.ToUnicodeString(unicodePoint) + ".png");
                        break;
                }

                // Download the image
                if (glyphUrl != null)
                {
                    // Always creating a new WebClient is poor performance
                    // This needs to be updated in the future
                    using (WebClient client = new WebClient())
                    {
                        var downloadTaskResult = new TaskCompletionSource<Bitmap?>();

                        DownloadDataCompletedEventHandler? eventHandler = null;
                        eventHandler = (sender, e) =>
                        {
                            try
                            {
                                using (Stream stream = new MemoryStream(e.Result))
                                {
                                    downloadTaskResult.SetResult(new Bitmap(stream));
                                }
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

                        client.DownloadDataCompleted += eventHandler;
                        client.DownloadDataAsync(glyphUrl);

                        result = await downloadTaskResult.Task;
                    }
                }
            }

            // Add any new bitmap to the cache for next time
            // In order to get this far a new glyph bitmap was generated (or at least an attempt was made)
            // It is possible that two renderers for the same glyph are running simultaneously on differing threads
            // Therefore, within the lock, a check must be made to ensure a glyph was not already added
            if (result != null)
            {
                lock (GlyphRenderer.cacheMutex)
                {
                    if (GlyphRenderer.cachedGlyphs.ContainsKey(glyphKey) == false)
                    {
                        GlyphRenderer.cachedGlyphs.Add(glyphKey, result);
                    }
                }
            }

            return result;
        }
    }
}
