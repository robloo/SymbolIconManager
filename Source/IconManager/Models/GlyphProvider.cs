using Avalonia.Platform;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using IconManager.IconSets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
