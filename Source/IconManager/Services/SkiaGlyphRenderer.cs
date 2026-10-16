using Avalonia.Platform;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using IconManager.IconSets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace IconManager.Services
{
    /// <summary>
    /// An icon glyph renderer implemented using SkiaSharp.
    /// </summary>
    public class SkiaGlyphRenderer : IGlyphRenderer<Bitmap>
    {
        public const int DefaultRenderWidth  = 64; // Pixels
        public const int DefaultRenderHeight = 64; // Pixels

        private static SKPaint? _cachedTextPaint = null;

        private static Dictionary<string, SKFont> _cachedFonts  = new Dictionary<string, SKFont>();  // IconSet/file name is key
        private static Dictionary<string, Bitmap> _cachedGlyphs = new Dictionary<string, Bitmap>();  // IconSet_UnicodePoint is key

        private static Lock _cacheLock = new();

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public uint RenderHeight { get; set; } = DefaultRenderHeight;

        /// <inheritdoc/>
        public uint RenderWidth { get; set; } = DefaultRenderWidth;

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public async Task<Bitmap?> RenderGlyphAsync(
            IconSet iconSet,
            uint unicodePoint,
            IGlyphSourceProvider glyphSourceProvider,
            GlyphSource glyphSource = GlyphSource.None)
        {
            string iconSetKey = iconSet.ToString();
            string glyphKey = iconSetKey + "_" + Icon.ToUnicodeHexString(unicodePoint);
            Bitmap? result = null;

            if (iconSet == IconSet.Undefined)
            {
                return null;
            }

            // Attempt to quickly load and return the bitmap from the cache
            lock (_cacheLock)
            {
                if (_cachedGlyphs.TryGetValue(glyphKey, out result))
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

                    var font = SkiaGlyphRenderer.LoadFont(iconSet.ToString());

                    if (font is not null)
                    {
                        var bitmap = await SkiaGlyphRenderer.RenderGlyphFromFontAsync(font, unicodePoint);

                        if (bitmap is not null)
                        {
                            // Use the Skia font-rendered glyph
                            return bitmap;
                        }
                    }

                    // Attempt to use an SVG image as fallback
                    // Note that the icon set determines that the format will be SVG
                    MemoryStream? svgStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(
                        iconSet,
                        unicodePoint,
                        GlyphSource.LocalSvgFile);

                    svgStream ??= await glyphSourceProvider.GetGlyphSourceStreamAsync(
                        iconSet,
                        unicodePoint,
                        GlyphSource.RemoteSvgFile);

                    if (svgStream is not null)
                    {
                        // The size here (and above) isn't taking into account device DPI
                        // It probably should in the future
                        // Also note that no background is added to the rendered SVG unlike above
                        var svg = new SkiaSharp.Extended.Svg.SKSvg(
                            new SKSize(this.RenderWidth, this.RenderHeight));
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

                using (Stream? imageStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(iconSet, unicodePoint, GlyphSource.RemotePngFile))
                {
                    return (imageStream is null ? null : new Bitmap(imageStream));
                }
            }

            // Add any new bitmap to the cache for next time
            // In order to get this far a new glyph bitmap was generated (or at least an attempt was made)
            // It is possible that two renderers for the same glyph are running simultaneously on differing threads
            // Therefore, within the lock, a check must be made to ensure a glyph was not already added
            if (result is not null)
            {
                lock (_cacheLock)
                {
                    if (_cachedGlyphs.ContainsKey(glyphKey) == false)
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
        private static async Task<Bitmap?> RenderGlyphFromFontAsync(
            SKFont font,
            uint unicodePoint,
            int renderWidth = SkiaGlyphRenderer.DefaultRenderWidth,
            int renderHeight = SkiaGlyphRenderer.DefaultRenderHeight)
        {
            return await Task.Run(() =>
            {
                var textBounds = new SKRect();
                SKBitmap bitmap = new SKBitmap(renderWidth, renderHeight);

                lock (_cacheLock)
                {
                    // Load all SKPaint objects
                    _cachedTextPaint ??= new SKPaint()
                    {
                        Color = SKColors.Black
                    };

                    // Check whether the font contains the glyph
                    if (font.GetGlyph((int)unicodePoint) == 0)
                    {
                        return null;
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
                    return new Bitmap(stream);
                }
            });
        }

        /// <summary>
        /// Loads the <see cref="SKFont"/> file that matches the given name.
        /// The <paramref name="fontNameKey"/> is commonly an <see cref="IconSet"/> but may also be
        /// the actual .ttf file name.
        /// </summary>
        /// <param name="fontNameKey">The name of the font to return.</param>
        /// <param name="renderWidth">The glyph render size in pixels for the typeface.</param>
        /// <returns>The loaded <see cref="SKFont"/>; otherwise, null.</returns>
        private static SKFont? LoadFont(
            string fontNameKey,
            uint renderWidth = SkiaGlyphRenderer.DefaultRenderWidth)
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

            var glyphSourceProvider = new GlyphSourceProviderCollection();
            glyphSourceProvider.Register(FluentUISystemGlyphs.Instance);
            glyphSourceProvider.Register(LineAwesomeGlyphs.Instance);
            glyphSourceProvider.Register(WinJSSymbolsGlyphs.Instance);

            lock (_cacheLock)
            {
                if (_cachedFonts.TryGetValue(fontKey, out font) == false)
                {
                    // Handle IconSet names first
                    if (Enum.TryParse(typeof(IconSet), fontKey, out object? parsedIconSet) &&
                        parsedIconSet is IconSet iconSet)
                    {
                        // Only some IconSet's have available font files
                        fontUri = glyphSourceProvider.GetFontSourceUri(iconSet);
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
    }
}
