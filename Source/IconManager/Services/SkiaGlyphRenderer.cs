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

        private static SKPaint? _cachedTextPaint = null;

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
            //
            //   Note that FluentUISystem icons fonts have a bug and any glyph over 0xFFFF simply does not exist
            //   See: https://github.com/microsoft/fluentui-system-icons/issues/299
            //   A work-around for this case is required using the local or remote SVG file sources
            //
            // If rendering using a font fails, fallback to any available SVG files.
            // SVG is high fidelity and the same quality as fonts.
            //
            // Only if rendering from both fonts and SVG files fails will PNG files be used.
            // These can't really be rendered and are just passed-through verbatim.
            //
            //   Note that some icon sets are Microsoft proprietary and the fonts cannot be used on non-Windows systems.
            //     * IconSet.SegoeFluent
            //     * IconSet.SegoeMDL2Assets
            //     * IconSet.SegoeUISymbol
            //   The font is not packaged with this application and there is no guarantee it will be on the system.
            //   Source files (whether SVG or PNG) are not packaged either.
            //   To work around this, preview images are loaded from Microsoft's website on-demand.
            //   These preview images are never distributed with source code.

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
                        GlyphBitmap? glyphBitmap = await SkiaGlyphRenderer.RenderGlyphFromFontAsync(font, unicodePoint);

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

                        // Extract the original path data from the SVG XML
                        var svgXml = XDocument.Parse(svgText);
                        string glyphPathData = svgXml
                            .Descendants()
                            .Where(e => e.Name.LocalName == "path")
                            .Select(e => (string?)e.Attribute("d"))
                            .Where(d => d != null)
                            .ToList()
                            .FirstOrDefault() ?? string.Empty;

                        // The size here (and above) isn't taking into account device DPI
                        // It probably should in the future
                        // Also note that no background is added to the rendered SVG unlike above
                        var svg = SKSvg.CreateFromSvg(svgText);

                        // Note that the default SKImage encoding format is .png
                        using (SKImage image = SKImage.FromPicture(svg.Picture, new SKSizeI((int)this.RenderWidth, (int)this.RenderHeight)))
                        using (SKData encoded = image.Encode())
                        using (Stream stream = encoded.AsStream())
                        {
                            var glyphBitmap = new GlyphBitmap(stream)
                            {
                                IconSet      = svgStream.IconSet,
                                PathData     = glyphPathData,
                                Source       = svgStream.Source,
                                SourceUri    = svgStream.SourceUri,
                                UnicodePoint = svgStream.UnicodePoint
                            };

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
        /// Renders the glyph at the specified Unicode point using the given symbol font.
        /// </summary>
        private static async Task<GlyphBitmap?> RenderGlyphFromFontAsync(
            SKFont font,
            uint unicodePoint,
            int renderWidth = SkiaGlyphRenderer.DefaultRenderWidth,
            int renderHeight = SkiaGlyphRenderer.DefaultRenderHeight)
        {
            return await Task.Run(() =>
            {
                var textBounds = new SKRect();
                string pathData = string.Empty;
                ushort glyphId = 0;
                SKBitmap bitmap = new SKBitmap(renderWidth, renderHeight);

                lock (_cacheLock)
                {
                    // Load all SKPaint objects
                    _cachedTextPaint ??= new SKPaint()
                    {
                        Color = SKColors.Black
                    };

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
                    using (SKCanvas canvas = new SKCanvas(bitmap))
                    {
                        // Set the canvas background
                        canvas.Clear(SKColors.White);

                        // Center the glyph within the bitmap canvas
                        // No need to consider baseline, just center the glyph
                        float x = (renderWidth / 2f) - textBounds.MidX;
                        float y = (renderHeight / 2f) - textBounds.MidY;

                        canvas.DrawText(
                            text,
                            x,
                            y,
                            font,
                            _cachedTextPaint);
                    }
                }

                // Use the Skia font-rendered glyph
                // Note that the default SKImage encoding format is .png
                using (SKImage image = SKImage.FromBitmap(bitmap))
                using (SKData encoded = image.Encode())
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
