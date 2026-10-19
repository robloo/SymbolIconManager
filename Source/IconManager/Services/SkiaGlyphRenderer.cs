using Avalonia;
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

        private Dictionary<string, SKFont>      _cachedFonts  = new Dictionary<string, SKFont>();       // IconSet is key
        private Dictionary<string, GlyphBitmap> _cachedGlyphs = new Dictionary<string, GlyphBitmap>();  // IconSet_UnicodePoint is key

        private Lock _cacheLock = new();

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
                unicodePoint).ToList();

            if (iconSet == IconSet.Undefined ||
                glyphSources.Count == 0)
            {
                return null;
            }

            // Attempt to quickly load and return the bitmap from the cache
            // This handles the following cases:
            //   1. No glyph source requested                        ->  Return the cached bitmap
            //   2. Requested glyph source matches the cached bitmap ->  Return the cached bitmap
            //   3. Requested glyph source is unavailable            ->  Return the cached bitmap as a fallback
            // 
            // If the requested glyph source is available, but the cached bitmap came from another source,
            // the cached bitmap will be skipped and rendered/cached again. Only the latest bitmap for all
            // glyph source exists in the cache at a given time.
            lock (this._cacheLock)
            {
                if (this._cachedGlyphs.TryGetValue(glyphKey, out result) &&
                    (glyphSource == GlyphSource.None ||
                     result.Source == glyphSource ||
                     !glyphSources.Contains(glyphSource)))
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

            // Prioritize glyph sources
            glyphSources = glyphSources
                .Distinct()
                .OrderBy(source => source switch
                {
                    GlyphSource.LocalFontFile  => 0,
                    GlyphSource.LocalSvgFile   => 1,
                    GlyphSource.RemoteSvgFile  => 2,
                    GlyphSource.RemotePngFile  => 3,
                    _                          => 4
                })
                .ToList();

            // Prioritize any input glyph source if a request was provided
            // Note that this is ONLY a request and if it cannot be honored standard
            // glyph sources will still be used as a fallback
            if (glyphSource != GlyphSource.None &&
                glyphSources.Contains(glyphSource))
            {
                glyphSources.Remove(glyphSource);
                glyphSources.Insert(0, glyphSource);
            }

            // Render the glyph
            foreach (GlyphSource source in glyphSources)
            {
                try
                {
                    result = await this.RenderGlyphFromSourceAsync(
                        iconSet,
                        unicodePoint,
                        glyphSourceProvider,
                        source);
                }
                catch { }

                if (result is not null)
                {
                    break;
                }
            }

            // Add any new bitmap to the cache for next time
            if (result is not null)
            {
                lock (this._cacheLock)
                {
                    // It is possible that two renderers for the same glyph are running simultaneously on differing threads
                    // Therefore, within the lock, a check must be made to ensure a glyph was not already added
                    if (!this._cachedGlyphs.ContainsKey(glyphKey))
                    {
                        this._cachedGlyphs.Add(glyphKey, result);
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
        /// Renders a glyph using an explicit source.
        /// </summary>
        private async Task<GlyphBitmap?> RenderGlyphFromSourceAsync(
            IconSet iconSet,
            uint unicodePoint,
            IGlyphSourceProvider glyphSourceProvider,
            GlyphSource glyphSource)
        {
            // Attempt to render from a Font
            if (glyphSource == GlyphSource.LocalFontFile)
            {
                SKFont? font = await Task.Run(() =>
                    this.LoadFont(
                        iconSet,
                        glyphSourceProvider,
                        this.RenderWidth));

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
                        // PathViewBox is already set
                        glyphBitmap.Source       = GlyphSource.LocalFontFile;
                        glyphBitmap.SourceUri    = null;
                        glyphBitmap.UnicodePoint = unicodePoint;

                        return glyphBitmap;
                    }
                }
            }

            // Attempt to render from an SVG file
            if (glyphSource == GlyphSource.LocalSvgFile ||
                glyphSource == GlyphSource.RemoteSvgFile)
            {
                return await Task.Run(async () =>
                {
                    using GlyphStream? svgStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(
                        iconSet,
                        unicodePoint,
                        glyphSource);

                    if (svgStream is not null)
                    {
                        // Read the SVG stream into a string
                        string svgText;
                        using (var reader = new StreamReader(
                            svgStream,
                            Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: true,
                            leaveOpen: true))
                        {
                            svgText = await reader.ReadToEndAsync();
                        }

                        GlyphBitmap? glyphBitmap = await SkiaGlyphRenderer.RenderGlyphFromSvgAsync(
                            svgText,
                            this.BackgroundColor,
                            this.FillColor,
                            this.RenderWidth,
                            this.RenderHeight);

                        if (glyphBitmap is not null)
                        {
                            // Apply glyph metadata
                            glyphBitmap.IconSet      = iconSet;
                            // PathData is already set
                            // PathViewBox is already set
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
            if (glyphSource == GlyphSource.RemotePngFile)
            {
                using GlyphStream? pngStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(
                    iconSet,
                    unicodePoint,
                    glyphSource);

                if (pngStream is not null)
                {
                    // Note: Constructing from the PNG stream sets glyph metadata
                    return new GlyphBitmap(pngStream)
                    {
                        PathData    = string.Empty,  // None for PNG
                        PathViewBox = default(Rect), // None for PNG
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Renders the given SVG glyph.
        /// </summary>
        private static async Task<GlyphBitmap?> RenderGlyphFromSvgAsync(
            string svgText,
            SKColor backgroundColor,
            SKColor fillColor,
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
                // This assumes fairly simple SVG files and ONLY preserves path data
                // If a glyph has multiple paths they will all be joined together as one
                var svgXml = XDocument.Parse(svgText);
                string pathData = string.Join(
                    " ",
                    svgXml
                        .Descendants()
                        .Where(e => e.Name.LocalName == "path")
                        .Select(e => (string?)e.Attribute("d"))
                        .Where(d => !string.IsNullOrWhiteSpace(d)));

                // Modify the SVG XML text directly to change the fill color
                // The fill will be overridden on every path and will also include inline styles
                string fillHex = $"#{fillColor.Red:X2}{fillColor.Green:X2}{fillColor.Blue:X2}";
                var svgPaths = svgXml
                    .Descendants()
                    .Where(e => e.Name.LocalName == "path");

                foreach (XElement pathElement in svgPaths)
                {
                    pathElement.SetAttributeValue("fill", fillHex);

                    // Override an existing inline fill style
                    var style = (string?)pathElement.Attribute("style");
                    if (!string.IsNullOrWhiteSpace(style))
                    {
                        var declarations = style
                            .Split(';')
                            .Where(s => !s.TrimStart().StartsWith(
                                "fill:", StringComparison.OrdinalIgnoreCase));

                        pathElement.SetAttributeValue(
                            "style",
                            string.Join(";", declarations) + $";fill:{fillHex}");
                    }
                }

                // Load the SVG image
                string modifiedSvgText = svgXml.ToString(SaveOptions.DisableFormatting);
                using SKSvg svg = SKSvg.CreateFromSvg(modifiedSvgText);
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
                        PathData    = pathData,
                        PathViewBox = new Rect(
                            x: viewBox.Left,
                            y: viewBox.Top,
                            viewBox.Width,
                            viewBox.Height)
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
                        PathViewBox  = new Rect(x: 0, y: 0, renderWidth, renderHeight),
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
        private SKFont? LoadFont(
            IconSet iconSet,
            IGlyphSourceProvider glyphSourceProvider,
            uint renderWidth = SkiaGlyphRenderer.DefaultRenderWidth)
        {
            string fontKey = iconSet.ToString();
            SKFont? font;

            lock (this._cacheLock)
            {
                if (this._cachedFonts.TryGetValue(fontKey, out font))
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
                lock (this._cacheLock)
                {
                    // It's possible the font was already cached by another thread
                    if (!this._cachedFonts.ContainsKey(fontKey))
                    {
                        this._cachedFonts.Add(fontKey, font);
                    }
                }
            }

            return font;
        }
    }
}
