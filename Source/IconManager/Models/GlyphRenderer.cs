using IconManager.Models;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace IconManager
{
    /// <summary>
    /// Contains methods to retrieve rendered images of a glyph.
    /// </summary>
    public static class GlyphRenderer
    {
        public const int RenderWidth  = 64; // Pixels
        public const int RenderHeight = 64; // Pixels

        private static SKPaint? cachedBackgroundPaint = null;

        private static Dictionary<string, Bitmap>  cachedGlyphs     = new Dictionary<string, Bitmap>();  // IconSet_UnicodePoint is key
        private static Dictionary<string, SKPaint> cachedTextPaints = new Dictionary<string, SKPaint>(); // IconSet/file name is key

        private static object cacheMutex = new object();

        /// <summary>
        /// Gets a preview bitmap of the glyph for the defined icon set and Unicode point.
        /// </summary>
        /// <param name="iconSet">The icon set containing the Unicode point.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>A preview bitmap of the glyph.</returns>
        public static async Task<Bitmap?> GetPreviewBitmapAsync(IconSet iconSet, uint unicodePoint)
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
                    // A work-around for this case is required using the local or remote SVG file sources

                    var font = GlyphProvider.LoadFont(iconSet.ToString());
                    var bitmap = await GlyphRenderer.RenderGlyph(font, iconSet.ToString(), unicodePoint);

                    if (bitmap != null)
                    {
                        // Use the Skia font-rendered glyph
                        return bitmap;
                    }
                    else
                    {
                        // Attempt to use an SVG image as fallback
                        // Note that the icon set determines that the format will be SVG
                        var svgStream = await GlyphProvider.GetGlyphSourceStreamAsync(iconSet, unicodePoint);

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

                using (Stream? imageStream = await GlyphProvider.GetGlyphSourceStreamAsync(iconSet, unicodePoint))
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
        /// Renders the glyph at the specified Unicode point using the given symbol font.
        /// </summary>
        public static async Task<Bitmap?> RenderGlyph(
            SKFont font,
            string fontKey,
            uint unicodePoint,
            int renderWidth = GlyphRenderer.RenderWidth,
            int renderHeight = GlyphRenderer.RenderHeight)
        {
            return await Task.Run(() =>
            {
                var textBounds = new SKRect();
                bool glyphExistsInFont;
                SKPaint? textPaint = null;
                SKBitmap bitmap = new SKBitmap(renderWidth, renderHeight);

                lock (cacheMutex)
                {
                    // Load all SKPaint objects
                    if (cachedBackgroundPaint == null)
                    {
                        cachedBackgroundPaint = new SKPaint()
                        {
                            Color = SKColors.White
                        };
                    }

                    if (cachedTextPaints.TryGetValue(fontKey, out textPaint) == false)
                    {
                        textPaint = new SKPaint(font)
                        {
                            Color = SKColors.Black
                        };

                        cachedTextPaints.Add(fontKey, textPaint);
                    }

                    // Measure the rendered text
                    // This also checks if the glyph exists in the font before continuing
                    string text = string.Empty;
                    try
                    {
                        text = char.ConvertFromUtf32((int)unicodePoint).ToString();
                    }
                    catch { }
                    textPaint.MeasureText(text, ref textBounds);

                    if (textBounds.Width == 0 ||
                        textBounds.Height == 0)
                    {
                        glyphExistsInFont = false;
                    }
                    else
                    {
                        glyphExistsInFont = true;
                    }

                    // Render the glyph using SkiaSharp
                    if (glyphExistsInFont)
                    {
                        using (SKCanvas canvas = new SKCanvas(bitmap))
                        {
                            canvas.DrawRect(
                                x: 0,
                                y: 0,
                                w: renderWidth,
                                h: renderHeight,
                                cachedBackgroundPaint);

                            canvas.DrawText(
                                text,
                                // No need to consider baseline, just center the glyph
                                x: (renderWidth / 2f) - textBounds.MidX,
                                y: (renderHeight / 2f) - textBounds.MidY,
                                font,
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

                return null;
            });
        }
    }
}
