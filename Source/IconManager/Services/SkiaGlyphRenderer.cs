using Avalonia.Platform;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IconManager.Services
{
    /// <summary>
    /// An icon glyph renderer implemented using SkiaSharp.
    /// </summary>
    public class SkiaGlyphRenderer : IGlyphRenderer<GlyphBitmap>
    {
        /// <summary>
        /// The default width, in pixels, of the rendered glyph.
        /// </summary>
        public const int DefaultRenderWidth  = 64; // Pixels

        /// <summary>
        /// The default height, in pixels, of the rendered glyph.
        /// </summary>
        public const int DefaultRenderHeight = 64; // Pixels

        private static Dictionary<string, SKFont>      _cachedFonts  = new Dictionary<string, SKFont>();       // IconSet is key
        private static Dictionary<string, GlyphBitmap> _cachedGlyphs = new Dictionary<string, GlyphBitmap>();  // IconSet_UnicodePoint is key

        private static Lock _cacheLock = new();

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaGlyphRenderer"/> class.
        /// </summary>
        public SkiaGlyphRenderer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaGlyphRenderer"/> class.
        /// </summary>
        /// <param name="renderWidth">The width, in pixels, of the rendered glyph.</param>
        /// <param name="renderHeight">The height, in pixels, of the rendered glyph.</param>
        public SkiaGlyphRenderer(
            uint renderWidth,
            uint renderHeight)
        {
            this.RenderHeight = renderHeight;
            this.RenderWidth  = renderWidth;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// The background canvas color to use behind the rendered glyph.
        /// </summary>
        /// <remarks>
        /// Not supported when rendered from existing bitmap sources.
        /// </remarks>
        public SKColor BackgroundColor { get; init; } = SKColors.Transparent;

        /// <summary>
        /// The path fill color to use in the rendered glyph.
        /// </summary>
        /// <remarks>
        /// Not supported when rendered from existing bitmap sources.
        /// </remarks>
        public SKColor FillColor { get; init; } = SKColors.Black;

        /// <inheritdoc/>
        public uint RenderHeight { get; init; } = DefaultRenderHeight;

        /// <inheritdoc/>
        public uint RenderWidth { get; init; } = DefaultRenderWidth;

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public async Task<GlyphBitmap?> RenderGlyphAsync(
            IconSet iconSet,
            uint unicodePoint,
            IGlyphSourceProvider glyphSourceProvider,
            GlyphSource glyphSource = GlyphSource.None)
        {
            string glyphKey = $"{iconSet}_{Icon.ToUnicodeHexString(unicodePoint)}";
            GlyphBitmap? result = null;
            var glyphSources = glyphSourceProvider.GetGlyphSources(
                iconSet,
                unicodePoint);

            if (iconSet == IconSet.Undefined ||
                glyphSources.Count == 0)
            {
                return null;
            }

            // Restrict the glyph source if an input request was given
            // Note that this is ONLY a request and if it cannot be honored standard
            // glyph sources will still be used as a fallback
            if (glyphSource != GlyphSource.None &&
                glyphSources.Contains(glyphSource))
            {
                glyphSources = new List<GlyphSource>
                {
                    glyphSource
                };
            }

            // Attempt to quickly load and return the bitmap from the cache
            // This doesn't respect any input glyph source request, but that's fine for now
            lock (_cacheLock)
            {
                if (_cachedGlyphs.TryGetValue(glyphKey, out result))
                {
                    return result;
                }
            }

            // Several icon sets have an embedded font file within the application.
            // Rendering glyphs is fastest using the embedded font itself so should always be tried first.
            // Rendering from fonts also supports customizing the fill color.
            //
            //   Note that FluentUISystem icons fonts have a bug and any glyph over 0xFFFF simply does not exist
            //   See: https://github.com/microsoft/fluentui-system-icons/issues/299
            //   A work-around for this case is required using the local or remote SVG file sources
            //
            // If rendering using a font fails, fallback to any available SVG files.
            // SVG is high fidelity and almost the same quality as fonts.
            //
            // Only if rendering from both fonts and SVG files fails will PNG files be used.
            // These can't really be rendered and are just passed-through verbatim.
            //
            //   Note that some icon sets are Microsoft proprietary and the fonts cannot be used on non-Windows systems.
            //     * IconSet.SegoeFluent
            //     * IconSet.SegoeMDL2Assets
            //     * IconSet.SegoeUISymbol
            //   These fonts are not packaged with this application and there is no guarantee they will be on the system.
            //   Source files (whether SVG or PNG) are not packaged either.
            //   To work around this, preview images are loaded from Microsoft's website on-demand.

            // Attempt to render from a Font
            if (result is null &&
                glyphSources.Contains(GlyphSource.LocalFontFile))
            {
                result = await Task.Run<GlyphBitmap?>(async () =>
                {
                    var font = SkiaGlyphRenderer.LoadFont(
                        iconSet,
                        glyphSourceProvider,
                        this.RenderWidth);

                    if (font is not null)
                    {
                        GlyphBitmap? glyphBitmap = await SkiaGlyphRenderer.RenderGlyphFromFontAsync(
                            font,
                            unicodePoint,
                            this.BackgroundColor,
                            this.FillColor,
                            this.RenderWidth,
                            this.RenderHeight);

                        if (glyphBitmap is not null)
                        {
                            // Apply glyph metadata
                            glyphBitmap.IconSet      = iconSet;
                            // PathData is already set
                            glyphBitmap.Source       = GlyphSource.LocalFontFile;
                            glyphBitmap.SourceUri    = null;
                            glyphBitmap.UnicodePoint = unicodePoint;

                            return glyphBitmap;
                        }
                    }

                    return null;
                });
            }

            // Attempt to render from an SVG file
            if (result is null &&
                (glyphSources.Contains(GlyphSource.LocalSvgFile) ||
                 glyphSources.Contains(GlyphSource.RemoteSvgFile)))
            {
                result = await Task.Run<GlyphBitmap?>(async () =>
                {
                    GlyphStream? svgStream = null;

                    if (svgStream is null &&
                        glyphSources.Contains(GlyphSource.LocalSvgFile))
                    {
                        svgStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(
                            iconSet,
                            unicodePoint,
                            GlyphSource.LocalSvgFile);
                    }

                    if (svgStream is null &&
                        glyphSources.Contains(GlyphSource.RemoteSvgFile))
                    {
                        svgStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(
                            iconSet,
                            unicodePoint,
                            GlyphSource.RemoteSvgFile);
                    }

                    if (svgStream is not null)
                    {
                        // Read the SVG stream into a string
                        string svgText;
                        using (var reader = new StreamReader(
                            svgStream,
                            Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: true,
                            leaveOpen: false))
                        {
                            svgText = reader.ReadToEnd();
                        }

                        GlyphBitmap? glyphBitmap = await SkiaGlyphRenderer.RenderGlyphFromSvgAsync(
                            svgText,
                            this.BackgroundColor,
                            this.RenderWidth,
                            this.RenderHeight);

                        if (glyphBitmap is not null)
                        {
                            // Apply glyph metadata
                            glyphBitmap.IconSet      = iconSet;
                            // PathData is already set
                            glyphBitmap.Source       = svgStream.Source;
                            glyphBitmap.SourceUri    = svgStream.SourceUri;
                            glyphBitmap.UnicodePoint = unicodePoint;

                            return glyphBitmap;
                        }
                    }

                    return null;
                });
            }

            // Attempt to use a downloadable PNG bitmap directly
            if (result is null &&
                glyphSources.Contains(GlyphSource.RemotePngFile))
            {
                using (GlyphStream? pngStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(iconSet, unicodePoint, GlyphSource.RemotePngFile))
                {
                    if (pngStream is not null)
                    {
                        result = new GlyphBitmap(pngStream)
                        {
                            PathData = string.Empty, // None for PNG
                        };
                    }
                    else
                    {
                        result = null;
                    }
                }
            }

            // Add any new bitmap to the cache for next time
            if (result is not null)
            {
                lock (_cacheLock)
                {
                    // It is possible that two renderers for the same glyph are running simultaneously on differing threads
                    // Therefore, within the lock, a check must be made to ensure a glyph was not already added
                    if (!_cachedGlyphs.ContainsKey(glyphKey))
                    {
                        _cachedGlyphs.Add(glyphKey, result);
                    }
                }
            }

            return result;
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Renders the given SVG glyph.
        /// </summary>
        private static async Task<GlyphBitmap?> RenderGlyphFromSvgAsync(
            string svgText,
            SKColor backgroundColor,
            uint renderWidth = SkiaGlyphRenderer.DefaultRenderWidth,
            uint renderHeight = SkiaGlyphRenderer.DefaultRenderHeight)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(svgText))
                {
                    return null;
                }

                if (renderWidth == 0 || renderHeight == 0 ||
                    renderWidth >= int.MaxValue || renderHeight >= int.MaxValue)
                {
                    return null;
                }

                // Extract the original path data from the SVG XML
                var svgXml = XDocument.Parse(svgText);
                string pathData = svgXml
                    .Descendants()
                    .Where(e => e.Name.LocalName == "path")
                    .Select(e => (string?)e.Attribute("d"))
                    .FirstOrDefault(d => d != null) ?? string.Empty;

                using SKSvg svg = SKSvg.CreateFromSvg(svgText);
                var svgPicture = svg.Picture;

                if (svgPicture is null)
                {
                    return null;
                }

                // Svg.Skia has already interpreted the SVG viewBox information
                // This is preserved in the CullRect property for the most part (though may not be exactly the same)
                // If ever this logic stops working the viewBox can be parsed from SVG text/XML directly as well
                SKRect viewBox = svgPicture.CullRect;

                if (viewBox.Width <= 0 || viewBox.Height <= 0)
                {
                    return null;
                }

                using SKBitmap bitmap = new SKBitmap((int)renderWidth, (int)renderHeight);
                using SKCanvas canvas = new SKCanvas(bitmap);

                // Set the canvas background
                canvas.Clear(backgroundColor);

                // Calculate the scale to fit the SVG image proportionally inside the bitmap canvas
                float scale = Math.Min(
                    renderWidth / viewBox.Width,
                    renderHeight / viewBox.Height);

                // Center the scaled glyph within the bitmap canvas
                float x = ((renderWidth  - (viewBox.Width  * scale)) / 2f) - (viewBox.Left * scale);
                float y = ((renderHeight - (viewBox.Height * scale)) / 2f) - (viewBox.Top  * scale);

                canvas.Translate(x, y);
                canvas.Scale(scale, scale);
                canvas.DrawPicture(svgPicture);

                // Convert to a PNG bitmap data stream
                using (SKImage image = SKImage.FromBitmap(bitmap))
                using (SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100))
                using (Stream stream = encoded.AsStream())
                {
                    var glyphBitmap = new GlyphBitmap(stream)
                    {
                        PathData = pathData
                    };

                    return glyphBitmap;
                }
            });
        }

        /// <summary>
        /// Renders the glyph at the specified Unicode point using the given symbol font.
        /// </summary>
        private static async Task<GlyphBitmap?> RenderGlyphFromFontAsync(
            SKFont font,
            uint unicodePoint,
            SKColor backgroundColor,
            SKColor fillColor,
            uint renderWidth = SkiaGlyphRenderer.DefaultRenderWidth,
            uint renderHeight = SkiaGlyphRenderer.DefaultRenderHeight)
        {
            return await Task.Run(() =>
            {
                var textBounds = new SKRect();
                string pathData = string.Empty;
                ushort glyphId = 0;

                if (renderWidth == 0 || renderHeight == 0 ||
                    renderWidth >= int.MaxValue || renderHeight >= int.MaxValue)
                {
                    return null;
                }

                // Check whether the font contains the glyph
                glyphId = font.GetGlyph((int)unicodePoint);
                if (glyphId == 0)
                {
                    return null;
                }

                // Extract the path data
                using (SKPath path = font.GetGlyphPath(glyphId))
                {
                    if (path is not null)
                    {
                        // Move the font's baseline-relative path down so the font's ascent is at y = 0.
                        path.Offset(0, -font.Metrics.Ascent);
                        pathData = path.ToSvgPathData();
                    }
                }

                // Measure the rendered text
                string text = string.Empty;
                try
                {
                    text = char.ConvertFromUtf32((int)unicodePoint).ToString();
                }
                catch { }
                font.MeasureText(text, out textBounds);

                // Render the glyph
                using SKBitmap bitmap = new SKBitmap((int)renderWidth, (int)renderHeight);
                using SKCanvas canvas = new SKCanvas(bitmap);
                using SKPaint textPaint = new SKPaint()
                {
                    Color = fillColor
                };

                // Set the canvas background
                canvas.Clear(backgroundColor);

                // Center the glyph within the bitmap canvas
                // No need to consider baseline, just center the glyph
                float x = (renderWidth / 2f) - textBounds.MidX;
                float y = (renderHeight / 2f) - textBounds.MidY;

                canvas.DrawText(
                    text,
                    x,
                    y,
                    font,
                    textPaint);

                // Convert to a PNG bitmap data stream
                using (SKImage image = SKImage.FromBitmap(bitmap))
                using (SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100))
                using (Stream stream = encoded.AsStream())
                {
                    return new GlyphBitmap(stream)
                    {
                        PathData     = pathData,
                        UnicodePoint = unicodePoint
                    };
                }
            });
        }

        /// <summary>
        /// Loads the <see cref="SKFont"/> that matches the given <see cref="IconSet"/>.
        /// </summary>
        /// <param name="iconSet">The <see cref="IconSet"/> of the font to load.</param>
        /// <param name="glyphSourceProvider">The glyph source data provider.</param>
        /// <param name="renderWidth">The glyph render size in pixels for the typeface.</param>
        /// <returns>The loaded <see cref="SKFont"/>; otherwise, null.</returns>
        private static SKFont? LoadFont(
            IconSet iconSet,
            IGlyphSourceProvider glyphSourceProvider,
            uint renderWidth = SkiaGlyphRenderer.DefaultRenderWidth)
        {
            string fontKey = iconSet.ToString();
            SKFont? font;

            lock (_cacheLock)
            {
                if (_cachedFonts.TryGetValue(fontKey, out font))
                {
                    return font;
                }
            }

            // Only some IconSet's have available font files
            Uri? fontUri = glyphSourceProvider.GetFontSourceUri(iconSet);

            if (fontUri is null)
            {
                return null;
            }

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
                }
            }

            if (font is not null)
            {
                lock (_cacheLock)
                {
                    // It's possible the font was already cached by another thread
                    if (!_cachedFonts.ContainsKey(fontKey))
                    {
                        _cachedFonts.Add(fontKey, font);
                    }
                }
            }

            return font;
        }
    }
}
